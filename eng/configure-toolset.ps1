# SdkTests do not currently work with globally installed CLI as they use dotnet-install.ps1 to install more runtimes

$script:useInstalledDotNetCli = $false

# Add CMake to the path.
$env:PATH = "$PSScriptRoot\..\.tools\bin;$env:PATH"

if ($msbuildEngine -eq 'vs')
{
    $env:MSBuildSdksPath = $(Get-ChildItem -Path (Join-Path $RepoRoot ".dotnet\sdk\") -Recurse -Directory -Filter Sdks | Select-Object -First 1 FullName).FullName
}
