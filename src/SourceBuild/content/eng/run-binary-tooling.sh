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

        # Use temp directory to store the package versions
        packagesDir=$(mktemp -d -p "$REPO_ROOT" "temp.XXXXXX")
        touch "$packagesDir/PackageVersions.props"

        # Create a PackageVersions.props file with the package versions
        packageReferences=$(grep -oP '<PackageReference Include=".*"' "$BINARY_TOOL/BinaryToolKit.csproj")
        echo "<Project>" > "$packagesDir/PackageVersions.props"
        echo "  <PropertyGroup>" >> "$packagesDir/PackageVersions.props"
        for line in $packageReferences; do
            package_name=$(echo "$line" | grep -oP '(?<=Include=")[^"]*')
            package_version=$(echo "$line" | grep -oP '(?<=Version="\$\()[^)]*')
            version=$(GetPackageVersion "$package_name")
            echo "    <$package_version>$version</$package_version>" >> "$packagesDir/PackageVersions.props"
        done
        echo "  </PropertyGroup>" >> "$packagesDir/PackageVersions.props"
        echo "</Project>" >> "$packagesDir/PackageVersions.props"
    else
        export ARTIFACTS_PATH=$packagesDir
    fi
}

function GetPackageVersion
{
  packageName=$1

  url="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/flat2/${packageName}/index.json"

  # If package is system.commandline, use the dotnet-libraries feed
  if [ "$packageName" == "System.CommandLine" ]; then
    url="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-libraries/nuget/v3/flat2/${packageName}/index.json"

    # Add dotnet-libraries feed to the NuGet.config if it doesn't exist in the config file already
    if ! grep -q "dotnet-libraries" "$BINARY_TOOL/NuGet.config"; then
      dotnet nuget add source "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-libraries/nuget/v3/index.json" --name "dotnet-libraries" --configfile "$BINARY_TOOL/NuGet.config" > /dev/null 2>&1
    fi
  fi

  # Send the GET request
  response=$(curl -s "${url}")

  # Check if the request was successful
  if [ $? -ne 0 ]; then
    echo "ERROR: Failed to get the package version for ${packageName}."
    exit 1
  fi

  # Extract the latest version of the package
  latest_version=$(echo "${response}" | jq -r '.versions[0]')

  # Print the latest version
  echo "${latest_version}"
}

function CleanUp() {

  # Remove the temp directory if it was created
  if [[ "$packagesDir" =~ temp\..{6} ]]; then
    rm -rf "$packagesDir"
  fi

  # Undo the NuGet.config changes if they were made
  if [ "$packagesDir" != "$defaultDotnetSdk" ]; then
    sed -i '/dotnet-libraries/d' "$BINARY_TOOL/NuGet.config"
  fi
}

function RunBinaryTool
{
  targetDir="$REPO_ROOT"
  outputDir="$REPO_ROOT/artifacts/log/binary-report"

  trap CleanUp EXIT
  
  # Run the BinaryDetection tool
  "$REPO_ROOT/.dotnet/dotnet" run --project "$BINARY_TOOL" -c Release -p PackagesPropsDirectory="$packagesDir" $mode "$targetDir" -o "$outputDir" -b "$baseline" -l "$logLevel"
}

ParseBinaryArgs
RunBinaryTool