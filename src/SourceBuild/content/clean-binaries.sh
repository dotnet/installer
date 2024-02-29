#!/usr/bin/env bash

### Usage: $0
###
###   Cleans binaries in the VMR.
###
### Options:
###   --binaries-keep-file <FILE>     Path to the file containing the list of allowed binaries to keep. Default is
###                                   src/installer/src/VirtualMonoRepo/allowed-binaries.txt
###   --binaries-remove-file <FILE>   Path to the file containing the list of allowed binaries to remove.
###                                   Default is null.
###   --with-sdk <DIR>                Use the SDK in the specified directory
###   --with-packages <DIR>           Use the specified directory of previously-built packages
###   --packages-source-feed <URL>    Use the specified URL as the source feed for packages

set -euo pipefail
IFS=$'\n\t'

source="${BASH_SOURCE[0]}"
SCRIPT_ROOT="$(cd -P "$( dirname "$0" )" && pwd)"

function print_help () {
    sed -n '/^### /,/^$/p' "$source" | cut -b 5-
}

defaultBinariesKeepFile="$SCRIPT_ROOT/src/installer/src/VirtualMonoRepo/allowed-binaries.txt"
defaultBinariesRemoveFile=''
defaultDotnetSdk="$SCRIPT_ROOT/.dotnet"
defaultPackagesDir="$SCRIPT_ROOT/prereqs/packages"
defaultPackagesSourceFeed=''

# Parse arguments
binariesKeepFile=$defaultBinariesKeepFile
binariesRemoveFile=$defaultBinariesRemoveFile
dotnetSdk=$defaultDotnetSdk
packagesDir=$defaultPackagesDir
packagesSourceFeed=$defaultPackagesSourceFeed
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
    --binaries-keep-file)
      binariesKeepFile=$2
      if [ ! -f "$binariesKeepFile" ]; then
        echo "Allowed binaries keep file '$binariesKeepFile' does not exist"
        exit 1
      fi
      shift
      ;;
    --binaries-remove-file)
      binariesRemoveFile=$2
      if [ ! -f "$binariesRemoveFile" ]; then
        echo "Allowed binaries remove file '$binariesRemoveFile' does not exist"
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
    --with-packages)
      packagesDir=$2
      if [ ! -d "$packagesDir" ]; then
        echo "Custom prviously built packages directory '$packagesDir' does not exist"
        exit 1
      fi
      shift
      ;;
    --packages-source-feed)
      packagesSourceFeed=$2
      shift
      ;;
    *)
      positional_args+=("$1")
      ;;
  esac

  shift
done

function ParsePositionalArgs {
  # Attempting to run the binary tooling without an SDK will fail. So either the --with-sdk flag must be passed
  # or a pre-existing .dotnet SDK directory must exist.
  if [ "$dotnetSdk" == "$defaultDotnetSdk" ] && [ ! -d "$dotnetSdk" ]; then
    echo "  ERROR: A pre-existing .dotnet SDK directory is needed if --with-sdk is not provided. \
    Please either supply an SDK using --with-sdk or execute ./prep.sh before proceeding. Exiting..."
    exit 1
  fi

  ## Attemping to run the binary tooling without a packages directory or source-feed will fail.
  ## So either the --with-packages flag must be passed with a valid directory or the source feed must be set using --packages-source-feed
  if [ "$packagesDir" == "$defaultPackagesDir" ] && [ ! -d "$packagesDir" ] && [ -z "$packagesSourceFeed" ]; then
    echo "  ERROR: A pre-existing packages directory is needed if --with-packages or --packages-source-feed is not provided. \
    Please either supply a packages directory using --with-packages, supply a source-feed using --packages-source-feed, or execute ./prep.sh before proceeding. Exiting..."
    exit 1
  fi

  # Unpack the previously built packages if the previously built packages directory is empty
  previouslyBuiltPackagesDir="$packagesDir/previously-source-built"
  packageArtifacts="$packagesDir/archive/Private.SourceBuilt.Artifacts.*.tar.gz"
  if [ "$packagesDir" == "$defaultPackagesDir" ] && [ ! -d "$previouslyBuiltPackagesDir" ]; then
    if [ -f ${packageArtifacts} ]; then
      echo "Unpacking previously built artifacts from ${packageArtifacts} to $previouslyBuiltPackagesDir"
      mkdir -p "$previouslyBuiltPackagesDir"
      tar -xzf ${packageArtifacts} -C "$previouslyBuiltPackagesDir"
    else
      echo "  ERROR: A pre-existing package archive is needed if --with-packages or --packages-source-feed is not provided. \
      Please either supply a packages directory using --with-packages, supply a source-feed using --packages-source-feed, or execute ./prep.sh before proceeding. Exiting..."
      exit 1
    fi
  fi

  # Set the source feed for the packages if it is not already set
  if [ -z "$packagesSourceFeed" ]; then
    packagesSourceFeed="file://$packagesDir"
  fi
}

function RunBinaryTool {
  BinaryDetectionTool="$SCRIPT_ROOT/eng/tools/BinaryToolKit"
  TargetDir="$SCRIPT_ROOT"
  OutputDir="$SCRIPT_ROOT/artifacts/binary-report"

  # Set the environment variable for the packages source feed
  export ARTIFACTS_PATH="$packagesSourceFeed"

  # Get the runtime version
  runtimeVersion=$("$dotnetSdk/dotnet" --list-runtimes | tail -n 1 | awk '{print $2}')

  "$dotnetSdk/dotnet" clean "$SCRIPT_ROOT/eng/tools/BinaryToolKit"

  export LOG_LEVEL=Debug

  # Run the BinaryDetection tool
  "$dotnetSdk/dotnet" run --project "$BinaryDetectionTool" -c Release -p RuntimeVersion="$runtimeVersion" "$TargetDir" "$OutputDir" -k "$binariesKeepFile" -r "$binariesRemoveFile" -m c
}

ParsePositionalArgs
RunBinaryTool
