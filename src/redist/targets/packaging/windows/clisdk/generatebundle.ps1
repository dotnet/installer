# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

param(
    [Parameter(Mandatory=$true)][string]$CLISDKMSIFile,
    [Parameter(Mandatory=$true)][string]$ASPNETRuntimeWixLibFile,
    [Parameter(Mandatory=$true)][string]$SharedFxMSIFile,
    [Parameter(Mandatory=$true)][string]$HostFxrMSIFile,
    [Parameter(Mandatory=$true)][string]$SharedHostMSIFile,
    [Parameter(Mandatory=$true)][string]$WinFormsAndWpfMSIFile,
    [Parameter(Mandatory=$true)][string]$NetCoreAppTargetingPackMSIFile,
    [Parameter(Mandatory=$true)][string]$NetCoreAppHostPackMSIFile,
    [Parameter(Mandatory=$true)][string]$NetStandardTargetingPackMSIFile,
    [Parameter(Mandatory=$true)][string]$AspNetTargetingPackMSIFile,
    [Parameter(Mandatory=$true)][string]$WindowsDesktopTargetingPackMSIFile,
    [Parameter(Mandatory=$true)][string]$DotnetBundleOutput,
    [Parameter(Mandatory=$true)][string]$WixRoot,
    [Parameter(Mandatory=$true)][string]$ProductMoniker,
    [Parameter(Mandatory=$true)][string]$DotnetMSIVersion,
    [Parameter(Mandatory=$true)][string]$SDKBundleVersion,
    [Parameter(Mandatory=$true)][string]$DotnetCLINugetVersion,
    [Parameter(Mandatory=$true)][string]$WindowsDesktopVersion,
    [Parameter(Mandatory=$true)][string]$UpgradeCode,
    [Parameter(Mandatory=$true)][string]$Architecture,
    [Parameter(Mandatory=$true)][string]$DotNetRuntimeVersion,
    [Parameter(Mandatory=$true)][string]$AspNetCoreVersion,
    [Parameter(Mandatory=$true)][string]$SDKProductBandVersion,
    [Parameter(Mandatory=$true)][string]$LocalizedContentDir
)

function RunCandleForBundle
{
    $result = $true
    pushd "$WixRoot"

    Write-Information "Running candle for bundle.."
    $AuthWsxRoot =  $PSScriptRoot

    $candleOutput = .\candle.exe -nologo `
        -dDotnetSrc="$inputDir" `
        -dMicrosoftEula="$PSScriptRoot\dummyeula.rtf" `
        -dProductMoniker="$ProductMoniker" `
        -dBuildVersion="$DotnetMSIVersion" `
        -dSDKBundleVersion="$SDKBundleVersion" `
        -dSDKProductBandVersion="$SDKProductBandVersion" `
        -dNugetVersion="$DotnetCLINugetVersion" `
        -dCLISDKMsiSourcePath="$CLISDKMSIFile" `
        -dUpgradeCode="$UpgradeCode" `
        -dSharedFXMsiSourcePath="$SharedFxMSIFile" `
        -dHostFXRMsiSourcePath="$HostFxrMSIFile" `
        -dSharedHostMsiSourcePath="$SharedHostMSIFile" `
        -dWinFormsAndWpfMsiSourcePath="$WinFormsAndWpfMSIFile" `
        -dNetCoreAppTargetingPackMsiSourcePath="$NetCoreAppTargetingPackMSIFile" `
        -dNetCoreAppHostPackMsiSourcePath="$NetCoreAppHostPackMSIFile" `
        -dNetStandardTargetingPackMsiSourcePath="$NetStandardTargetingPackMSIFile" `
        -dAspNetTargetingPackMsiSourcePath="$AspNetTargetingPackMSIFile" `
        -dWindowsDesktopTargetingPackMsiSourcePath="$WindowsDesktopTargetingPackMSIFile" `
        -dWinFormsAndWpfVersion="$WindowsDesktopVersion" `
        -dAdditionalSharedFXMsiSourcePath="$AdditionalSharedFxMSIFile" `
        -dAdditionalHostFXRMsiSourcePath="$AdditionalHostFxrMSIFile" `
        -dAdditionalSharedHostMsiSourcePath="$AdditionalSharedHostMSIFile" `
        -dDotNetRuntimeVersion="$DotNetRuntimeVersion" `
        -dAspNetCoreVersion="$AspNetCoreVersion" `
        -dLocalizedContentDir="$LocalizedContentDir" `
        -arch "$Architecture" `
        -ext WixBalExtension.dll `
        -ext WixUtilExtension.dll `
        -ext WixTagExtension.dll `
        "$AuthWsxRoot\bundle.wxs"

    Write-Information "Candle output: $candleOutput"

    if($LastExitCode -ne 0)
    {
        $result = $false
        Write-Information "Candle failed with exit code $LastExitCode."
    }

    popd
    return $result
}

function RunLightForBundle
{
    $result = $true
    pushd "$WixRoot"

    Write-Information "Running light for bundle.."
    $AuthWsxRoot =  $PSScriptRoot

    $lightOutput = .\light.exe -nologo `
        -cultures:en-us `
        bundle.wixobj `
        $ASPNETRuntimeWixlibFile `
        -ext WixBalExtension.dll `
        -ext WixUtilExtension.dll `
        -ext WixTagExtension.dll `
        -b "$AuthWsxRoot" `
        -out $DotnetBundleOutput

    Write-Information "Light output: $lightOutput"

    if($LastExitCode -ne 0)
    {
        $result = $false
        Write-Information "Light failed with exit code $LastExitCode."
    }

    popd
    return $result
}


if(!(Test-Path $CLISDKMSIFile))
{
    throw "$CLISDKMSIFile not found"
}

if(!(Test-Path $ASPNETRuntimeWixLibFile))
{
    throw "$ASPNETRuntimeWixLibFile not found"
}

Write-Information "Creating dotnet Bundle at $DotnetBundleOutput"

if([string]::IsNullOrEmpty($WixRoot))
{
    Exit -1
}

if(-Not (RunCandleForBundle))
{
    Exit -1
}

if(-Not (RunLightForBundle))
{
    Exit -1
}

if(!(Test-Path $DotnetBundleOutput))
{
    throw "Unable to create the dotnet bundle."
    Exit -1
}

Write-Information "Successfully created dotnet bundle - $DotnetBundleOutput"

exit $LastExitCode
