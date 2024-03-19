#!/usr/bin/env bash

### Usage: $0
###
###   Prepares and runs the binary tooling to detect or remove binaries not in the specified baseline.
###
### Options:
###   --clean                    Clean the VMR of binaries not in the specified baseline.
###   --validate                 Validate the VMR for binaries not in the specified baseline.
###   --baseline <path>          Path to the baseline file.
###                              Defaults to eng/allowed-vmr-binaries.txt for validate.
###                              Defaults to eng/allowed-sb-binaries.txt for clean.
###   --log-level <level>        Set the log level for the binary tooling. Defaults to Debug.
###   --with-packages            Use the specified directory as the packages source feed.
###                              Defaults to online dotnet-public and dotnet-libraries feeds.
###   --with-sdk                 Use the specified directory as the dotnet SDK.
###                              Defaults to .dotnet.

set -euo pipefail
IFS=$'\n\t'

source="${BASH_SOURCE[0]}"
REPO_ROOT="$( cd -P "$( dirname "$0" )/../" && pwd )"
BINARY_TOOL="$REPO_ROOT/eng/tools/BinaryToolKit"

function print_help () {
    sed -n '/^### /,/^$/p' "$source" | cut -b 5-
}

defaultDotnetSdk="$REPO_ROOT/.dotnet"

# Set default values
baseline=''
mode=''
logLevel='Debug'
propsDir=''
packagesDir=''
dotnetSdk=$defaultDotnetSdk

positional_args=()
while :; do
  if [ $# -le 0 ]; then
    break
  fi
  lowerI="$(echo "$1" | awk '{print tolower($0)}')"
  case $lowerI in
    "-?"|-h|--help)
      print_help
      exit 0
      ;;
    --clean)
      mode="clean"
      if [ -z "$baseline" ]; then
        baseline="$REPO_ROOT/eng/allowed-sb-binaries.txt"
      fi
      ;;
    --validate)
      mode="validate"
      if [ -z "$baseline" ]; then
        baseline="$REPO_ROOT/eng/allowed-vmr-binaries.txt"
      fi
      ;;
    --baseline)
      baseline=$2
      if [ ! -f "$baseline" ]; then
        echo "ERROR: The specified baseline file does not exist."
        exit 1
      fi
      shift
      ;;
    --log-level)
      logLevel=$2
      shift
      ;;
    --with-packages)
        packagesDir=$2
        if [ ! -d "$packagesDir" ]; then
            echo "ERROR: The specified packages directory does not exist."
            exit 1
        elif [ ! -f "$packagesDir/PackageVersions.props" ]; then
            echo "ERROR: The specified packages directory does not contain PackageVersions.props."
            exit 1
        fi
        shift
        ;;
    --with-sdk)
        dotnetSdk=$2
        if [ ! -d "$dotnetSdk" ]; then
            echo "Custom SDK directory '$dotnetSdk' does not exist"
            exit 1
        fi
        if [ ! -x "$dotnetSdk/dotnet" ]; then
            echo "Custom SDK '$dotnetSdk/dotnet' does not exist or is not executable"
            exit 1
        fi
        shift
        ;;
    *)
      positional_args+=("$1")
      ;;
  esac

  shift
done

function ParseBinaryArgs
{
    if [ -z "$mode" ]; then
        echo "ERROR: --clean or --validate must be specified."
        print_help
        exit 1
    fi

    # Check dotnet sdk
    if [ "$dotnetSdk" == "$defaultDotnetSdk" ]; then
        if [ ! -d "$dotnetSdk" ]; then
            . "$REPO_ROOT/eng/common/tools.sh"
            InitializeDotNetCli true
        fi
        else if [ ! -x "$dotnetSdk/dotnet" ]; then
            echo "'$dotnetSdk/dotnet' does not exist or is not executable"
            exit 1
        fi
    fi

    # Check the packages directory
    if [ -z "$packagesDir" ]; then
        # Use dotnet-public feed as the default packages source feed
        export ARTIFACTS_PATH="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json"

        # Add dotnet-libraries feed to the NuGet.config. This is needed for System.CommandLine.
        "$dotnetSdk/dotnet" nuget add source "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-libraries/nuget/v3/index.json" --name "dotnet-libraries" --configfile "$BINARY_TOOL/NuGet.config" > /dev/null 2>&1
    else
        packagesDir=$(realpath ${packagesDir})
        export ARTIFACTS_PATH=$packagesDir
    fi
}

function CleanUp
{
  # Undo the NuGet.config changes if they were made
  if [ -z "$packagesDir" ]; then
    sed -i '/dotnet-libraries/d' "$BINARY_TOOL/NuGet.config"
  fi
}

function RunBinaryTool
{
  targetDir="$REPO_ROOT"
  outputDir="$REPO_ROOT/artifacts/log/binary-report"
  BinaryToolCommand=""$dotnetSdk/dotnet" run --project "$BINARY_TOOL" -c Release "$mode" "$targetDir" -o "$outputDir" -b "$baseline" -l "$logLevel""

  trap CleanUp EXIT

  if [ -n "$packagesDir" ]; then
    BinaryToolCommand=""$BinaryToolCommand" -p CustomPackageVersionsProps="$packagesDir/PackageVersions.props""
  fi

  # Run the Binary Tool
  eval "$BinaryToolCommand"
}

ParseBinaryArgs
RunBinaryTool