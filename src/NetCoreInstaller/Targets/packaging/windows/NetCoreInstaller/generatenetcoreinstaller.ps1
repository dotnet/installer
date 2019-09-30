# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

param(
    [Parameter(Mandatory=$true)][string]$NetCoreInstallerFile,
    [Parameter(Mandatory=$true)][string]$AspNetCoreSharedFrameworkX64InstallerFileName,
    [Parameter(Mandatory=$true)][string]$AspNetCoreSharedFrameworkX86InstallerFileName,
    [Parameter(Mandatory=$true)][string]$AspNetCoreV2ModuleX64InstallerFileName,
    [Parameter(Mandatory=$true)][string]$AspNetCoreV2ModuleX86InstallerFileName,
    [Parameter(Mandatory=$true)][string]$AspNetTargetingPackX64InstallerFileName,
    [Parameter(Mandatory=$true)][string]$AspNetTargetingPackX86InstallerFileName,
    [Parameter(Mandatory=$true)][string]$HostFxrX64InstallerFileName,
    [Parameter(Mandatory=$true)][string]$HostFxrX86InstallerFileName,
    [Parameter(Mandatory=$true)][string]$NetCoreAppHostPackX64InstallerFileName,
    [Parameter(Mandatory=$true)][string]$NetCoreAppHostPackX64_ArmInstallerFileName,
    [Parameter(Mandatory=$true)][string]$NetCoreAppHostPackX64_Arm64InstallerFileName,
    [Parameter(Mandatory=$true)][string]$NetCoreAppHostPackX64_X86InstallerFileName,
    [Parameter(Mandatory=$true)][string]$NetCoreAppHostPackX86InstallerFileName,
    [Parameter(Mandatory=$true)][string]$NetCoreAppHostPackX86_ArmInstallerFileName,
    [Parameter(Mandatory=$true)][string]$NetCoreAppHostPackX86_Arm64InstallerFileName,
    [Parameter(Mandatory=$true)][string]$NetCoreAppHostPackX86_X64InstallerFileName,
    [Parameter(Mandatory=$true)][string]$NetCoreAppTargetingPackX64InstallerFileName,
    [Parameter(Mandatory=$true)][string]$NetCoreAppTargetingPackX86InstallerFileName,
    [Parameter(Mandatory=$true)][string]$NetCoreTemplatesX64InstallerFileName,
    [Parameter(Mandatory=$true)][string]$NetCoreTemplatesX86InstallerFileName,
    [Parameter(Mandatory=$true)][string]$NetStandardTargetingPackX64InstallerFileName,
    [Parameter(Mandatory=$true)][string]$NetStandardTargetingPackX86InstallerFileName,
    [Parameter(Mandatory=$true)][string]$SdkToolsetX64InstallerFileName,
    [Parameter(Mandatory=$true)][string]$SdkToolsetX86InstallerFileName,
    [Parameter(Mandatory=$true)][string]$SharedFrameworkX64InstallerFileName,
    [Parameter(Mandatory=$true)][string]$SharedFrameworkX86InstallerFileName,
    [Parameter(Mandatory=$true)][string]$SharedHostX64InstallerFileName,
    [Parameter(Mandatory=$true)][string]$SharedHostX86InstallerFileName,
    [Parameter(Mandatory=$true)][string]$WindowsDesktopTargetingPackX64InstallerFileName,
    [Parameter(Mandatory=$true)][string]$WindowsDesktopTargetingPackX86InstallerFileName,
    [Parameter(Mandatory=$true)][string]$WinFormsAndWpfSharedFrameworkX64InstallerFileName,
    [Parameter(Mandatory=$true)][string]$WinFormsAndWpfSharedFrameworkX86InstallerFileName,
    [Parameter(Mandatory=$true)][string]$NetCoreVersion,
    [Parameter(Mandatory=$true)][string]$AspNetCoreVersion,
    [Parameter(Mandatory=$true)][string]$WindowsDesktopVersion,
    [Parameter(Mandatory=$true)][string]$PermanentToolsetBaseURL,
    [Parameter(Mandatory=$true)][string]$PermanentNetCoreBaseURL,
    [Parameter(Mandatory=$true)][string]$PermanentAspNetCoreBaseURL,
    [Parameter(Mandatory=$true)][string]$PermanentWindowsDesktopBaseURL,
    [Parameter(Mandatory=$true)][string]$WixRoot,
    [Parameter(Mandatory=$true)][string]$InstallerName,
    [Parameter(Mandatory=$true)][string]$InstallerVersion,
    [Parameter(Mandatory=$true)][string]$UpgradeAndProviderCode,
    [Parameter(Mandatory=$true)][string]$SourceMSIDirectory,
    [Parameter(Mandatory=$true)][string]$NetCoreInstallerArchitecture
)

function RunCandleForNetCoreInstaller
{
    $result = $true
    pushd "$WixRoot"

    Write-Information "Running candle for NetCore Installer.."

    $candleOutput = .\candle.exe -nologo `
        -dDotnetSrc="$inputDir" `
        -dAspNetCoreSharedFrameworkX64InstallerFileName="$AspNetCoreSharedFrameworkX64InstallerFileName" `
        -dAspNetCoreSharedFrameworkX86InstallerFileName="$AspNetCoreSharedFrameworkX86InstallerFileName" `
        -dAspNetCoreV2ModuleX64InstallerFileName="$AspNetCoreV2ModuleX64InstallerFileName" `
        -dAspNetCoreV2ModuleX86InstallerFileName="$AspNetCoreV2ModuleX86InstallerFileName" `
        -dAspNetTargetingPackX64InstallerFileName="$AspNetTargetingPackX64InstallerFileName" `
        -dAspNetTargetingPackX86InstallerFileName="$AspNetTargetingPackX86InstallerFileName" `
        -dHostFxrX64InstallerFileName="$HostFxrX64InstallerFileName" `
        -dHostFxrX86InstallerFileName="$HostFxrX86InstallerFileName" `
        -dNetCoreAppHostPackX64InstallerFileName="$NetCoreAppHostPackX64InstallerFileName" `
        -dNetCoreAppHostPackX64_ArmInstallerFileName="$NetCoreAppHostPackX64_ArmInstallerFileName" `
        -dNetCoreAppHostPackX64_Arm64InstallerFileName="$NetCoreAppHostPackX64_Arm64InstallerFileName" `
        -dNetCoreAppHostPackX64_X86InstallerFileName="$NetCoreAppHostPackX64_X86InstallerFileName" `
        -dNetCoreAppHostPackX86InstallerFileName="$NetCoreAppHostPackX86InstallerFileName" `
        -dNetCoreAppHostPackX86_ArmInstallerFileName="$NetCoreAppHostPackX86_ArmInstallerFileName" `
        -dNetCoreAppHostPackX86_Arm64InstallerFileName="$NetCoreAppHostPackX86_Arm64InstallerFileName" `
        -dNetCoreAppHostPackX86_X64InstallerFileName="$NetCoreAppHostPackX86_X64InstallerFileName" `
        -dNetCoreAppTargetingPackX64InstallerFileName="$NetCoreAppTargetingPackX64InstallerFileName" `
        -dNetCoreAppTargetingPackX86InstallerFileName="$NetCoreAppTargetingPackX86InstallerFileName" `
        -dNetCoreTemplatesX64InstallerFileName="$NetCoreTemplatesX64InstallerFileName" `
        -dNetCoreTemplatesX86InstallerFileName="$NetCoreTemplatesX86InstallerFileName" `
        -dNetStandardTargetingPackX64InstallerFileName="$NetStandardTargetingPackX64InstallerFileName" `
        -dNetStandardTargetingPackX86InstallerFileName="$NetStandardTargetingPackX86InstallerFileName" `
        -dSdkToolsetX64InstallerFileName="$SdkToolsetX64InstallerFileName" `
        -dSdkToolsetX86InstallerFileName="$SdkToolsetX86InstallerFileName" `
        -dSharedFrameworkX64InstallerFileName="$SharedFrameworkX64InstallerFileName" `
        -dSharedFrameworkX86InstallerFileName="$SharedFrameworkX86InstallerFileName" `
        -dSharedHostX64InstallerFileName="$SharedHostX64InstallerFileName" `
        -dSharedHostX86InstallerFileName="$SharedHostX86InstallerFileName" `
        -dWindowsDesktopTargetingPackX64InstallerFileName="$WindowsDesktopTargetingPackX64InstallerFileName" `
        -dWindowsDesktopTargetingPackX86InstallerFileName="$WindowsDesktopTargetingPackX86InstallerFileName" `
        -dWinFormsAndWpfSharedFrameworkX64InstallerFileName="$WinFormsAndWpfSharedFrameworkX64InstallerFileName" `
        -dWinFormsAndWpfSharedFrameworkX86InstallerFileName="$WinFormsAndWpfSharedFrameworkX86InstallerFileName" `
        -dNetCoreVersion="$NetCoreVersion" `
        -dAspNetCoreVersion="$AspNetCoreVersion" `
        -dWindowsDesktopVersion="$WindowsDesktopVersion" `
        -dPermanentToolsetBaseURL="$PermanentToolsetBaseURL" `
        -dPermanentNetCoreBaseURL="$PermanentNetCoreBaseURL" `
        -dPermanentAspNetCoreBaseURL="$PermanentAspNetCoreBaseURL" `
        -dPermanentWindowsDesktopBaseURL="$PermanentWindowsDesktopBaseURL" `
        -dWixRoot="$WixRoot" `
        -dInstallerName="$InstallerName" `
        -dInstallerVersion="$InstallerVersion" `
        -dUpgradeAndProviderCode="$UpgradeAndProviderCode" `
        -dSdkDependencyKeyName="$SdkDependencyKeyName" `
        -dLocalizedContentDirs="$LocalizedContentDirs" `
        -dSourceMSIDirectory="$SourceMSIDirectory" `
        -arch "$NetCoreInstallerArchitecture" `
        -ext WixBalExtension.dll `
        -ext WixUtilExtension.dll `
        -ext WixDependencyExtension.dll `
        -ext WixNetFxExtension.dll `
        -ext WixDotNetCoreBootstrapperExtension\WixDotNetCoreBootstrapperExtension.dll `
        "$AuthWsxRoot\netcoreinstaller.wxs"

    Write-Information "Candle output: $candleOutput"

    if($LastExitCode -ne 0)
    {
        $result = $false
        Write-Information "Candle failed with exit code $LastExitCode."
    }

    popd
    return $result
}

function RunLightForNetCoreInstaller
{
    $result = $true
    pushd "$WixRoot"

    Write-Information "Running light for NetCore Installer.."

    $lightOutput = .\light.exe -nologo `
        -cultures:en-us `
        netcoreinstaller.wixobj `
        -ext WixBalExtension.dll `
        -ext WixUtilExtension.dll `
        -ext WixDependencyExtension.dll `
        -ext WixNetFxExtension.dll `
        -ext WixDotNetCoreBootstrapperExtension\WixDotNetCoreBootstrapperExtension.dll `
        -b "$AuthWsxRoot" `
        -out $NetCoreInstallerFile

    Write-Information "Light output: $lightOutput"

    if($LastExitCode -ne 0)
    {
        $result = $false
        Write-Information "Light failed with exit code $LastExitCode."
    }

    popd
    return $result
}

if([string]::IsNullOrEmpty($WixRoot))
{
    Exit -1
}

Write-Information "Creating NetCore Installer at $NetCoreInstallerFile"

$AuthWsxRoot = $PSScriptRoot
$LocalizedContentDirs = (Get-ChildItem "$AuthWsxRoot\LCID\*\dncmba.wxl").Directory.Name -join ';'

if(-Not (RunCandleForNetCoreInstaller))
{
    Exit -1
}

if(-Not (RunLightForNetCoreInstaller))
{
    Exit -1
}

if(!(Test-Path $NetCoreInstallerFile))
{
    throw "Unable to create the NetCore Installer."
    Exit -1
}

Write-Information "Successfully created NetCore Installer - $NetCoreInstallerFile"

exit $LastExitCode
