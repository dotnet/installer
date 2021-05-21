# SdkTests do not currently work with globally installed CLI as they use dotnet-install.ps1 to install more runtimes

$script:useInstalledDotNetCli = $true

# Add CMake to the path.
$env:PATH = "$PSScriptRoot\..\.tools\bin;$env:PATH"

if ($msbuildEngine -eq 'vs')
{
    Write-Host "msbuildengine is vs"
    if (-not $env:DOTNET_INSTALL_DIR -eq $null)
    {
        $localSdkRoot = Join-Path $env:DOTNET_INSTALL_DIR "sdk"
        Write-Host "dotnet_install_dir: $localSdkRoot"
        $env:MSBuildSdksPath = $localSdkRoot
    }
}