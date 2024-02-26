#!/usr/bin/env bash

### Usage: $0
###
###   Prepares the environment to be built by downloading Private.SourceBuilt.Artifacts.*.tar.gz and
###   installing the version of dotnet referenced in global.json
###
### Options:
###   --no-artifacts              Exclude the download of the previously source-built artifacts archive
###   --no-bootstrap              Don't replace portable packages in the download source-built artifacts
###   --no-prebuilts              Exclude the download of the prebuilts archive
###   --no-sdk                    Exclude the download of the .NET SDK
###   --artifacts-rid             The RID of the previously source-built artifacts archive to download
###                               Default is centos.8-x64
###   --runtime-source-feed       URL of a remote server or a local directory, from which SDKs and
###                               runtimes can be downloaded
###   --runtime-source-feed-key   Key for accessing the above server, if necessary
###   --no-binary-removal         Exclude the removal of binaries
###   --binaries-keep-file        Path to the file containing the list of allowed binaries to keep. Default is
###                               src/installer/src/VirtualMonoRepo/allowed-binaries.txt
###   --binaries-remove-file      Path to the file containing the list of allowed binaries to remove.
###                               Default is null.

set -euo pipefail
IFS=$'\n\t'

source="${BASH_SOURCE[0]}"
SCRIPT_ROOT="$(cd -P "$( dirname "$0" )" && pwd)"

function print_help () {
    sed -n '/^### /,/^$/p' "$source" | cut -b 5-
}

defaultArtifactsRid='centos.8-x64'
defaultBinariesKeepFile="$SCRIPT_ROOT/src/installer/src/VirtualMonoRepo/allowed-binaries.txt"
defaultBinariesRemoveFile=''

buildBootstrap=true
downloadArtifacts=true
downloadPrebuilts=true
installDotnet=true
removeBinaries=true
binariesKeepFile=$defaultBinariesKeepFile
binariesRemoveFile=$defaultBinariesRemoveFile
artifactsRid=$defaultArtifactsRid
runtime_source_feed='' # IBM requested these to support s390x scenarios
runtime_source_feed_key='' # IBM requested these to support s390x scenarios
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
    --no-bootstrap)
      buildBootstrap=false
      ;;
    --no-artifacts)
      downloadArtifacts=false
      ;;
    --no-prebuilts)
      downloadPrebuilts=false
      ;;
    --no-sdk)
      installDotnet=false
      ;;
    --artifacts-rid)
      artifactsRid=$2
      ;;
    --runtime-source-feed)
      runtime_source_feed=$2
      shift
      ;;
    --runtime-source-feed-key)
      runtime_source_feed_key=$2
      shift
      ;;
    --no-binary-removal)
      removeBinaries=false
      ;;
    --binaries-keep-file)
      binariesKeepFile=$2
      shift
      ;;
    --binaries-remove-file)
      binariesRemoveFile=$2
      shift
      ;;
    *)
      positional_args+=("$1")
      ;;
  esac

  shift
done

# Attempting to bootstrap without an SDK will fail. So either the --no-sdk flag must be passed
# or a pre-existing .dotnet SDK directory must exist.
if [ "$buildBootstrap" == true ] && [ "$installDotnet" == false ] && [ ! -d "$SCRIPT_ROOT/.dotnet" ]; then
  echo "  ERROR: --no-sdk requires --no-bootstrap or a pre-existing .dotnet SDK directory.  Exiting..."
  exit 1
fi

## Attemping to remove binaries without an SDK will fail. So either the --no-binary-removal flag must be passed
# or a pre-existing .dotnet SDK directory must exist.
if [ "$removeBinaries" == true ] && [ "$installDotnet" == false ] && [ ! -d "$SCRIPT_ROOT/.dotnet" ]; then
  echo "  ERROR: --no-sdk requires --no-binary-removal or a pre-existing .dotnet SDK directory.  Exiting..."
  exit 1
fi

# Check to make sure curl exists to download the archive files
if ! command -v curl &> /dev/null
then
  echo "  ERROR: curl not found.  Exiting..."
  exit 1
fi

# Check if Private.SourceBuilt artifacts archive exists
artifactsBaseFileName="Private.SourceBuilt.Artifacts"
packagesArchiveDir="$SCRIPT_ROOT/prereqs/packages/archive/"
if [ "$downloadArtifacts" == true ] && [ -f ${packagesArchiveDir}${artifactsBaseFileName}.*.tar.gz ]; then
  echo "  Private.SourceBuilt.Artifacts.*.tar.gz exists...it will not be downloaded"
  downloadArtifacts=false

  if [ -z "$runtime_source_feed" ]; then
  runtime_source_feed="file://$packagesArchiveDir"
  fi
fi

# Check if Private.SourceBuilt prebuilts archive exists
prebuiltsBaseFileName="Private.SourceBuilt.Prebuilts"
if [ "$downloadPrebuilts" == true ] && [ -f ${packagesArchiveDir}${prebuiltsBaseFileName}.*.tar.gz ]; then
  echo "  Private.SourceBuilt.Prebuilts.*.tar.gz exists...it will not be downloaded"
  downloadPrebuilts=false
fi

# Check if dotnet is installed
if [ "$installDotnet" == true ] && [ -d "$SCRIPT_ROOT/.dotnet" ]; then
  echo "  ./.dotnet SDK directory exists...it will not be installed"
  installDotnet=false;
fi

function DownloadArchive {
  archiveType="$1"
  isRequired="$2"
  artifactsRid="$3"

  packageVersionsPath="$SCRIPT_ROOT/eng/Versions.props"
  notFoundMessage="No source-built $archiveType found to download..."

  echo "  Looking for source-built $archiveType to download..."
  archiveVersionLine=$(grep -m 1 "<PrivateSourceBuilt${archiveType}Version>" "$packageVersionsPath" || :)
  versionPattern="<PrivateSourceBuilt${archiveType}Version>(.*)</PrivateSourceBuilt${archiveType}Version>"
  if [[ $archiveVersionLine =~ $versionPattern ]]; then
    archiveVersion="${BASH_REMATCH[1]}"

    if [ "$archiveType" == "Prebuilts" ]; then
        archiveRid=$defaultArtifactsRid
    else
        archiveRid=$artifactsRid
    fi

    archiveUrl="https://dotnetcli.azureedge.net/source-built-artifacts/assets/Private.SourceBuilt.$archiveType.$archiveVersion.$archiveRid.tar.gz"

    echo "  Downloading source-built $archiveType from $archiveUrl..."
    (cd "$packagesArchiveDir" && curl --retry 5 -O "$archiveUrl")
  elif [ "$isRequired" == true ]; then
    echo "  ERROR: $notFoundMessage"
    exit 1
  else
    echo "  $notFoundMessage"
  fi
}

function BootstrapArtifacts {
  DOTNET_SDK_PATH="$SCRIPT_ROOT/.dotnet"

  # Create working directory for running bootstrap project
  workingDir=$(mktemp -d)
  echo "  Building bootstrap previously source-built in $workingDir"

  # Copy bootstrap project to working dir
  cp "$SCRIPT_ROOT/eng/bootstrap/buildBootstrapPreviouslySB.csproj" "$workingDir"

  # Copy NuGet.config from the installer repo to have the right feeds
  cp "$SCRIPT_ROOT/src/installer/NuGet.config" "$workingDir"

  # Get PackageVersions.props from existing prev-sb archive
  echo "  Retrieving PackageVersions.props from existing archive"
  sourceBuiltArchive=$(find "$packagesArchiveDir" -maxdepth 1 -name 'Private.SourceBuilt.Artifacts*.tar.gz')
  if [ -f "$sourceBuiltArchive" ]; then
      tar -xzf "$sourceBuiltArchive" -C "$workingDir" PackageVersions.props
  fi

  # Run restore on project to initiate download of bootstrap packages
  "$DOTNET_SDK_PATH/dotnet" restore "$workingDir/buildBootstrapPreviouslySB.csproj" /bl:artifacts/log/prep-bootstrap.binlog /fileLoggerParameters:LogFile=artifacts/log/prep-bootstrap.log /p:ArchiveDir="$packagesArchiveDir" /p:BootstrapOverrideVersionsProps="$SCRIPT_ROOT/eng/bootstrap/OverrideBootstrapVersions.props"

  # Remove working directory
  rm -rf "$workingDir"
}

function RemoveBinaries {
  DOTNET_SDK_PATH="$SCRIPT_ROOT/.dotnet"
  BinaryDetectionTool="$SCRIPT_ROOT/eng/tools/BinaryToolKit/BinaryToolCli"

  # Reconfigure the runtime source feed if it is not set or is set to the default
  previouslySourceBuiltDir="$SCRIPT_ROOT/prereqs/packages/previously-source-built"
  if [ -z "$runtime_source_feed" ] || [ "$runtime_source_feed" == "file://$packagesArchiveDir" ]; then
    runtime_source_feed="file://$previouslySourceBuiltDir"
    
    # Unpack the PSB artifacts if they haven't been unpacked
    if [ ! -d "$previouslySourceBuiltDir" ]; then
      echo "  Unpacking Private.SourceBuilt.Artifacts.*.tar.gz to $previouslySourceBuiltDir"
      sourceBuiltArchive=$(find "$packagesArchiveDir" -maxdepth 1 -name 'Private.SourceBuilt.Artifacts*.tar.gz')
      mkdir -p "$previouslySourceBuiltDir"
      tar -xzf "$sourceBuiltArchive" -C "$previouslySourceBuiltDir"
    fi
  fi

  export ARTIFACTS_PATH="$runtime_source_feed"

  # Set the key for the runtime source feed if it is provided
  if [ -n "$runtime_source_feed_key" ]; then
    export ARTIFACTS_KEY="$runtime_source_feed_key"
  else
    export ARTIFACTS_KEY="previously-source-built"
  fi

  # Get the runtime version
  runtimeVersion=$("$DOTNET_SDK_PATH/dotnet" --list-runtimes | tail -n 1 | awk '{print $2}')

  # Check binary files
  if [ -n "$binariesKeepFile" ] && [ ! -f "$binariesKeepFile" ]; then
    echo "  ERROR: Allowed binaries keep file not found.  Exiting..."
    exit 1
  fi
  if [ -n "$binariesRemoveFile" ] && [ ! -f "$binariesRemoveFile" ]; then
    echo "  ERROR: Allowed binaries remove file not found.  Exiting..."
    exit 1
  fi

  # Run the BinaryDetection tool
  "$DOTNET_SDK_PATH/dotnet" run --project "$BinaryDetectionTool" -c Release -p RuntimeVersion="$runtimeVersion" "$SCRIPT_ROOT" "$SCRIPT_ROOT/artifacts/binary-report" --keep "$binariesKeepFile" --remove "$binariesRemoveFile"
}

# Check for the version of dotnet to install
if [ "$installDotnet" == true ]; then
  echo "  Installing dotnet..."
  use_installed_dotnet_cli=false
  (source ./eng/common/tools.sh && InitializeDotNetCli true)

  # TODO: Remove once runtime dependency is gone (https://github.com/dotnet/runtime/issues/93666)
  bash .dotnet/dotnet-install.sh --install-dir "$SCRIPT_ROOT/.dotnet" --channel 8.0 --runtime dotnet
fi

# Read the eng/Versions.props to get the archives to download and download them
if [ "$downloadArtifacts" == true ]; then
  DownloadArchive Artifacts true $artifactsRid
  if [ "$buildBootstrap" == true ]; then
      BootstrapArtifacts
  fi
fi

if [ "$downloadPrebuilts" == true ]; then
  DownloadArchive Prebuilts false $artifactsRid
fi

if [ "$removeBinaries" == true ]; then
  RemoveBinaries
fi
