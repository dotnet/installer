#!/usr/bin/env bash
set -euo pipefail
IFS=$'\n\t'

SCRIPT_ROOT="$(cd -P "$( dirname "$0" )" && pwd)"

usage() {
    echo "usage: $0"
    echo ""
    echo "  Prepares the environment to be built by downloading Private.SourceBuilt.Artifacts.*.tar.gz and"
    echo "  installing the version of dotnet referenced in global.json"
    echo "options:"
    echo "  --no-bootstrap Don't replace portable packages in the download source-built artifacts"
    echo ""
}

buildBootstrap=true
positional_args=()
while :; do
    if [ $# -le 0 ]; then
        break
    fi
    lowerI="$(echo "$1" | awk '{print tolower($0)}')"
    case $lowerI in
        "-?"|-h|--help)
            usage
            exit 0
            ;;
        --no-bootstrap)
            buildBootstrap=false
            ;;
        *)
            positional_args+=("$1")
            ;;
    esac

    shift
done

downloadArtifacts=true
downloadPrebuilts=true
installDotnet=true

# Check to make sure curl exists to download the archive files
if ! command -v curl &> /dev/null
then
    echo "  ERROR: curl not found.  Exiting..."
    exit -1
fi

# Check if Private.SourceBuilt artifacts archive exists
artifactsBaseFileName="Private.SourceBuilt.Artifacts"
packagesArchiveDir="$SCRIPT_ROOT/prereqs/packages/archive/"
if [ -f ${packagesArchiveDir}${artifactsBaseFileName}.*.tar.gz ]; then
    echo "  Private.SourceBuilt.Artifacts.*.tar.gz exists...it will not be downloaded"
    downloadArtifacts=false
fi

# Check if Private.SourceBuilt prebuilts archive exists
prebuiltsBaseFileName="Private.SourceBuilt.Prebuilts"
if [ -f ${packagesArchiveDir}${prebuiltsBaseFileName}.*.tar.gz ]; then
    echo "  Private.SourceBuilt.Prebuilts.*.tar.gz exists...it will not be downloaded"
    downloadPrebuilts=false
fi

# Check if dotnet is installed
if [ -d $SCRIPT_ROOT/.dotnet ]; then
    echo "  ./.dotnet SDK directory exists...it will not be installed"
    installDotnet=false;
fi

function DownloadArchive {
    archiveType="$1"
    baseFileName="$2"
    isRequired="$3"

    packageVersionsPath="$SCRIPT_ROOT/eng/Versions.props"
    notFoundMessage="No source-built $archiveType found to download..."

    echo "  Looking for source-built $archiveType to download..."
    archiveVersionLine=`grep -m 1 "<PrivateSourceBuilt${archiveType}Url>" "$packageVersionsPath" || :`
    versionPattern="<PrivateSourceBuilt${archiveType}Url>(.*)</PrivateSourceBuilt${archiveType}Url>"
    if [[ $archiveVersionLine =~ $versionPattern ]]; then
        archiveUrl="${BASH_REMATCH[1]}"
        echo "  Downloading source-built $archiveType from $archiveUrl..."
        (cd $packagesArchiveDir && curl --retry 5 -O $archiveUrl)
    elif [ "$isRequired" == "true" ]; then
      echo "  ERROR: $notFoundMessage"
      exit -1
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
    cp $SCRIPT_ROOT/eng/bootstrap/buildBootstrapPreviouslySB.csproj $workingDir

    # Copy NuGet.config from the installer repo to have the right feeds
    cp $SCRIPT_ROOT/src/installer/NuGet.config $workingDir

    # Get PackageVersions.props from existing prev-sb archive
    echo "  Retrieving PackageVersions.props from existing archive"
    sourceBuiltArchive=`find $packagesArchiveDir -maxdepth 1 -name 'Private.SourceBuilt.Artifacts*.tar.gz'`
    if [ -f "$sourceBuiltArchive" ]; then
        tar -xzf "$sourceBuiltArchive" -C $workingDir PackageVersions.props
    fi

    # Run restore on project to initiate download of bootstrap packages
    $DOTNET_SDK_PATH/dotnet restore $workingDir/buildBootstrapPreviouslySB.csproj /bl:artifacts/prep/bootstrap.binlog /fileLoggerParameters:LogFile=artifacts/prep/bootstrap.log /p:ArchiveDir="$packagesArchiveDir" /p:BootstrapOverrideVersionsProps="$SCRIPT_ROOT/eng/bootstrap/OverrideBootstrapVersions.props"

    # Remove working directory
    rm -rf $workingDir
}

# Check for the version of dotnet to install
if [ "$installDotnet" == "true" ]; then
    echo "  Installing dotnet..."
    (source ./eng/common/tools.sh && InitializeDotNetCli true)
fi

# Read the eng/Versions.props to get the archives to download and download them
if [ "$downloadArtifacts" == "true" ]; then
    DownloadArchive "Artifacts" $artifactsBaseFileName "true"
    if [ "$buildBootstrap" == "true" ]; then
        BootstrapArtifacts
    fi
fi

if [ "$downloadPrebuilts" == "true" ]; then
    DownloadArchive "Prebuilts" $prebuiltsBaseFileName "false"
fi
