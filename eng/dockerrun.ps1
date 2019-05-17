[CmdletBinding(PositionalBinding=$false)]
Param(
  [bool] $noninteractive = $false,
  [string] $dockerImageName,
  [string] $dockerContainerTag = "dotnetcli-build",
  [string] $dockerContainerName = "dotnetcli-build-container",
  [Parameter(ValueFromRemainingArguments=$true)][String[]]$additionalArgs
)

# sample command line: .\eng\dockerrun.ps1 -dockerImageName ubuntu.18.04 /p:DisableSourceLink=true --test --pack --publish

Write-Host "Docker image name: $dockerImageName"
Write-Host "Additional args: $additionalArgs"

. $PSScriptRoot\common\tools.ps1

# docker build -f old\scripts\docker\rhel\Dockerfile --build-arg USER_ID=1000 -t redhat .
# docker run -it -v c:\git\core-sdk-arcade:/opt/code redhat bash

$dockerFile = Resolve-Path (Join-Path $RepoRoot "eng\docker\$dockerImageName")

docker build --build-arg USER_ID=1000 -t "$dockerContainerTag" $dockerFile

$interactiveFlag = "-i"
if ($noninteractive)
{
  $interactiveFlag = ""
}

` # -e DOTNET_INSTALL_DIR=/opt/code/artifacts/docker/$dockerImageName/.dotnet `

docker run $interactiveFlag -t --rm --sig-proxy=true `
  --name "$dockerContainerName" `
  -v "${RepoRoot}:/opt/code" `
  -e DOTNET_CORESDK_IGNORE_TAR_EXIT_CODE=1 `
  -e CHANNEL `
  -e DOTNET_BUILD_SKIP_CROSSGEN `
  -e PUBLISH_TO_AZURE_BLOB `
  -e NUGET_FEED_URL `
  -e NUGET_API_KEY `
  -e ARTIFACT_STORAGE_ACCOUNT `
  -e ARTIFACT_STORAGE_CONTAINER `
  -e CHECKSUM_STORAGE_ACCOUNT `
  -e CHECKSUM_STORAGE_CONTAINER `
  -e BLOBFEED_STORAGE_CONTAINER `
  -e CLIBUILD_SKIP_TESTS `
  -e COMMITCOUNT `
  -e DROPSUFFIX `
  -e RELEASESUFFIX `
  -e COREFXAZURECONTAINER `
  -e AZUREACCOUNTNAME `
  -e RELEASETOOLSGITURL `
  -e CORESETUPBLOBROOTURL `
  -e PB_ASSETROOTURL `
  -e PB_PACKAGEVERSIONPROPSURL `
  -e PB_PUBLISHBLOBFEEDURL `
  -e EXTERNALRESTORESOURCES `
  -e ARCADE_PARTITION="${dockerImageName}" `
  $dockerContainerTag `
  /opt/code/run-build.sh @additionalArgs