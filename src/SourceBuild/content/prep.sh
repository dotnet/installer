#!/usr/bin/env bash

### Usage: $0
###
###   Prepares the environment to be built by downloading Private.SourceBuilt.Artifacts.*.tar.gz and
###   installing the version of dotnet referenced in global.json
###
### Options:
###   --no-artifacts                Exclude the download of the previously source-built artifacts archive
###   --no-bootstrap                Don't replace portable packages in the download source-built artifacts
###   --no-prebuilts                Exclude the download of the prebuilts archive
###   --no-sdk                      Exclude the download of the .NET SDK
###   --runtime-source-feed         URL of a remote server or a local directory, from which SDKs and
###                                 runtimes can be downloaded
###   --runtime-source-feed-key     Key for accessing the above server, if necessary
###   --smoke-test-prereqs-path     Directory where the smoke test prereqs packages should be downloaded to
###   --smoke-test-prereqs-feed     Additional NuGet package feed URL from which to download the smoke test
###                                 prereqs
###   --smoke-test-prereqs-feed-key Access token for the smoke test preqreqs NuGet package feed. If not
###                                 specified, an interactive restore will be used.

set -euo pipefail
IFS=$'\n\t'

source="${BASH_SOURCE[0]}"
SCRIPT_ROOT="$(cd -P "$( dirname "$0" )" && pwd)"

function print_help () {
    sed -n '/^### /,/^$/p' "$source" | cut -b 5-
}

buildBootstrap=true
downloadArtifacts=true
downloadPrebuilts=true
installDotnet=true
runtime_source_feed='' # IBM requested these to support s390x scenarios
runtime_source_feed_key='' # IBM requested these to support s390x scenarios
smokeTestPrereqsFeed=''
smokeTestPrereqsFeedKey=''
smokeTestPrereqsPath=''
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
    --runtime-source-feed)
      runtime_source_feed=$2
      shift
      ;;
    --runtime-source-feed-key)
      runtime_source_feed_key=$2
      shift
      ;;
    --smoke-test-prereqs-feed)
      smokeTestPrereqsFeed=$2
      shift
      ;;
    --smoke-test-prereqs-feed-key)
      smokeTestPrereqsFeedKey=$2
      shift
      ;;
    --smoke-test-prereqs-path)
      smokeTestPrereqsPath=$2
      shift
      ;;
    *)
      positional_args+=("$1")
      ;;
  esac

  shift
done

DOTNET_SDK_PATH="$SCRIPT_ROOT/.dotnet"

# Attempting to bootstrap without an SDK will fail. So either the --no-sdk flag must be passed
# or a pre-existing .dotnet SDK directory must exist.
if [ "$buildBootstrap" == true ] && [ "$installDotnet" == false ] && [ ! -d $DOTNET_SDK_PATH ]; then
  echo "  ERROR: --no-sdk requires --no-bootstrap or a pre-existing .dotnet SDK directory.  Exiting..."
  exit 1
fi

# Downloading smoke test prereq packages requires a .NET installation
if [ -n "$smokeTestPrereqsPath" ] && [ "$installDotnet" == false ] && [ ! -d $DOTNET_SDK_PATH ]; then
  echo "  ERROR: --smoke-test-prereqs-path requires --no-sdk to be unset or a pre-existing .dotnet SDK directory.  Exiting..."
  exit 1
fi

# If the smoke test prereqs feed key is set, then smoke test prereqs feed must also be set
if [ -n "$smokeTestPrereqsFeedKey" ] && [ -z "$smokeTestPrereqsFeed" ]; then
  echo "  ERROR: --smoke-test-prereqs-feed must be set if --smoke-test-prereqs-feed-key is set.  Exiting..."
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
fi

# Check if Private.SourceBuilt prebuilts archive exists
prebuiltsBaseFileName="Private.SourceBuilt.Prebuilts"
if [ "$downloadPrebuilts" == true ] && [ -f ${packagesArchiveDir}${prebuiltsBaseFileName}.*.tar.gz ]; then
  echo "  Private.SourceBuilt.Prebuilts.*.tar.gz exists...it will not be downloaded"
  downloadPrebuilts=false
fi

# Check if dotnet is installed
if [ "$installDotnet" == true ] && [ -d $DOTNET_SDK_PATH ]; then
  echo "  ./.dotnet SDK directory exists...it will not be installed"
  installDotnet=false;
fi

function DownloadArchive {
  archiveType="$1"
  isRequired="$2"

  packageVersionsPath="$SCRIPT_ROOT/eng/Versions.props"
  notFoundMessage="No source-built $archiveType found to download..."

  echo "  Looking for source-built $archiveType to download..."
  archiveVersionLine=$(grep -m 1 "<PrivateSourceBuilt${archiveType}Url>" "$packageVersionsPath" || :)
  versionPattern="<PrivateSourceBuilt${archiveType}Url>(.*)</PrivateSourceBuilt${archiveType}Url>"
  if [[ $archiveVersionLine =~ $versionPattern ]]; then
      archiveUrl="${BASH_REMATCH[1]}"
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
  "$DOTNET_SDK_PATH/dotnet" restore "$workingDir/buildBootstrapPreviouslySB.csproj" /bl:artifacts/prep/bootstrap.binlog /fileLoggerParameters:LogFile=artifacts/prep/bootstrap.log /p:ArchiveDir="$packagesArchiveDir" /p:BootstrapOverrideVersionsProps="$SCRIPT_ROOT/eng/bootstrap/OverrideBootstrapVersions.props"

  # Remove working directory
  rm -rf "$workingDir"
}

# Check for the version of dotnet to install
if [ "$installDotnet" == true ]; then
  echo "  Installing dotnet..."
  (source ./eng/common/tools.sh && InitializeDotNetCli true)
fi

# Read the eng/Versions.props to get the archives to download and download them
if [ "$downloadArtifacts" == true ]; then
  DownloadArchive Artifacts true
  if [ "$buildBootstrap" == true ]; then
      BootstrapArtifacts
  fi
fi

if [ "$downloadPrebuilts" == true ]; then
  DownloadArchive Prebuilts false
fi

if [ -n "$smokeTestPrereqsPath" ] ; then
  smokeTestPrereqsProjPath="test/Microsoft.DotNet.SourceBuild.SmokeTests/assets"
  smokeTestPrereqsTmp="/tmp/smoke-test-prereqs"
  smokeTestsNuGetConfigPath="$smokeTestPrereqsTmp/nuget.config"
  smokeTestsFeedName="smoke-test-prereqs"

  # Generate a nuget.config file. If a feed was provided, include that first. Also include nuget.org feed by default.

  mkdir -p $smokeTestPrereqsTmp

  echo "<?xml version=\"1.0\" encoding=\"utf-8\"?>
<configuration>
  <packageSources>
    <clear />" > "$smokeTestsNuGetConfigPath"

  if [ -n "$smokeTestPrereqsFeed" ] ; then
    echo "<add key=\"$smokeTestsFeedName\" value=\"%SMOKE_TEST_PREREQS_FEED%\" />" >> "$smokeTestsNuGetConfigPath"
  fi

  echo "<add key=\"nuget\" value=\"https://api.nuget.org/v3/index.json\" />
  </packageSources>" >> "$smokeTestsNuGetConfigPath"

  # If the caller specified a PAT for accessing the feed, generate a credentials section
  if [ -n "$smokeTestPrereqsFeedKey" ] ; then
    echo "<packageSourceCredentials>
        <$smokeTestsFeedName>
        <add key=\"Username\" value=\"smoke-test-prereqs\" />
        <add key=\"ClearTextPassword\" value=\"%SMOKE_TEST_PREREQS_FEED_KEY%\" />
        </$smokeTestsFeedName>
    </packageSourceCredentials>" >> "$smokeTestsNuGetConfigPath"
  fi

  echo "</configuration>" >> "$smokeTestsNuGetConfigPath"
  
  # Gather the versions of various components so they can be passed as MSBuild properties

  function getPackageVersion() {
    # Extract the package version from the props XML file and trim the servicing label suffix if it exists
    sed -n 's:.*<OutputPackageVersion>\(.*\)</OutputPackageVersion>.*:\1:p' $1 | sed 's/-servicing.*//'
  }

  runtimeVersion=$(getPackageVersion prereqs/git-info/runtime.props)
  aspnetCoreVersion=$(getPackageVersion prereqs/git-info/aspnetcore.props)
  fsharpVersion=$(getPackageVersion prereqs/git-info/fsharp.props)

  SMOKE_TEST_PREREQS_FEED=$smokeTestPrereqsFeed \
  SMOKE_TEST_PREREQS_FEED_KEY=$smokeTestPrereqsFeedKey \
  "$DOTNET_SDK_PATH/dotnet" msbuild \
    "$smokeTestPrereqsProjPath/prereqs.csproj" \
    /t:DownloadPrereqs \
    /bl:artifacts/prep/smokeTestPrereqs.binlog \
    /fileLoggerParameters:LogFile=artifacts/prep/smokeTestPrereqs.log \
    /p:RestorePackagesPath="$smokeTestPrereqsPath" \
    /p:RuntimeVersion=$runtimeVersion \
    /p:AspnetCoreVersion=$aspnetCoreVersion \
    /p:FsharpVersion=$fsharpVersion \
    /p:RestoreConfigFile=$smokeTestsNuGetConfigPath

fi
