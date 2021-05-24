# SdkTests do not currently work with globally installed CLI as they use dotnet-install.ps1 to install more runtimes

$script:useInstalledDotNetCli = $false

# Add CMake to the path.
$env:PATH = "$PSScriptRoot\..\.tools\bin;$env:PATH"

if ($msbuildEngine -eq 'vs')
{
    $globalJson = Get-Content -Raw global.json | ConvertFrom-Json
    $env:MSBuildSdksPath = [IO.Path]::Combine($RepoRoot, ".dotnet\sdk", $globalJson.tools.dotnet, "sdks")
}
