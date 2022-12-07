#!/bin/bash

### This script helps to reproduce potential failures of the 'Synchronize dotnet/dotnet'
### build step from the **VMR Source-Build** job locally.
### The following scenario is assumed:
### - There is a PR in dotnet/installer
### - The PR is failing on the 'VMR Source-Build' job in the 'Synchronize dotnet/dotnet' step
###
### There are a few possible reasons but usually this happens because there is a mismatch between
### source patches that are applied on top of the files that are being synchronized from repositories
### into the 'dotnet/dotnet' repo and new changes in the repositories.
### This manifests as the following error:
###     fail: Failed to synchronize repo installer
###           Failed to apply the patch for src/aspnetcore
###           Exit code: 1
###           Std err:
###           error: patch failed: src/aspnetcore/eng/SourceBuild.props:55
###           error: src/aspnetcore/eng/SourceBuild.props: patch does not apply
###
### where 'src/aspnetcore/eng/SourceBuild.props' would be the file that is being patched and new
### changes to it are conflicting with the patch that is being applied.
###
### The whole process can be reproduced locally easily by running this script.
### The patches are located in a folder in the 'dotnet/installer' repository.
### At the moment of writing, the location is 'src/SourceBuild/tarball/patches' but to get
### the up-to-date location, please see the 'patchesPath' property in
### https://github.com/dotnet/dotnet/blob/main/src/source-mappings.json
###
### You will need to compare what is in the patch and what is in the file and fix the patch.
###
### The tooling that synchronizes the VMR will need to clone the various repositories.
### It clones them into a temporary folder and re-uses them on future runs so it is advised
### you dedicate a folder to this to speed up your re-runs.
###
### This script will synchronize the 'dotnet/dotnet' repo locally and let you inspect the changes.
###
### USAGE:
###     ./reproduce-vmr-sync.sh --installer "$HOME/repos/installer" --tmp "$HOME/repos/tmp"
### Options:
###   --installer-dir
###       Path to the 'dotnet/installer' repo which is checked out at the PR commit
###   --tmp-dir
###       Path to the temporary folder where the repositories will be cloned
###   --vmr-branch
###       Branch of the 'dotnet/dotnet' repo to synchronize to, defaults to 'main'

source="${BASH_SOURCE[0]}"

# resolve $source until the file is no longer a symlink
while [[ -h "$source" ]]; do
  scriptroot="$( cd -P "$( dirname "$source" )" && pwd )"
  source="$(readlink "$source")"
  # if $source was a relative symlink, we need to resolve it relative to the path where the
  # symlink file was located
  [[ $source != /* ]] && source="$scriptroot/$source"
done
scriptroot="$( cd -P "$( dirname "$source" )" && pwd )"

function print_help () {
    sed -n '/^### /,/^$/p' "$source" | cut -b 5-
}

COLOR_RED=$(tput setaf 1 2>/dev/null || true)
COLOR_CYAN=$(tput setaf 6 2>/dev/null || true)
COLOR_CLEAR=$(tput sgr0 2>/dev/null || true)
COLOR_RESET=uniquesearchablestring
FAILURE_PREFIX='# '

function fail () {
  echo "${COLOR_RED}$FAILURE_PREFIX${1//${COLOR_RESET}/${COLOR_RED}}${COLOR_CLEAR}"
}

function highlight () {
  echo "${COLOR_CYAN}$FAILURE_PREFIX${1//${COLOR_RESET}/${COLOR_CYAN}}${COLOR_CLEAR}"
}

installer_dir=''
tmp_dir=''
vmr_dir=''
vmr_branch='main'
# hashed name coming from the VMR tooling
INSTALLER_TMP_DIR_NAME='03298978DFFFCD23'

while [[ $# -gt 0 ]]; do
  opt="$(echo "$1" | tr "[:upper:]" "[:lower:]")"
  case "$opt" in
    --installer-dir)
      installer_dir=$2
      shift
      ;;
    --vmr-dir)
      vmr_dir=$2
      shift
      ;;
    --tmp-dir)
      tmp_dir=$2
      shift
      ;;
    --vmr-branch)
      vmr_branch=$2
      shift
      ;;
    --help)
      print_help
      exit 0
      ;;
    -h)
      print_help
      exit 0
      ;;
    *)
      fail "Invalid argument: $1"
      usage
      exit 1
      ;;
  esac

  shift
done

if [[ -z "$installer_dir" ]]; then
  fail "Missing --installer-dir argument"
  exit 1
fi

if [[ ! -d "$installer_dir" ]]; then
  fail "Directory '$installer_dir' does not exist"
  exit 1
fi

if [[ -z "$tmp_dir" ]]; then
  fail "Missing --tmp-dir argument"
  exit 1
fi

if [[ -z "$vmr_dir" ]]; then
  vmr_dir="$tmp_dir/dotnet"
fi

if [[ ! -d "$tmp_dir" ]]; then
  mkdir -p "$tmp_dir"
fi

if [[ ! -d "$vmr_dir" ]]; then
  highlight "Cloning 'dotnet/dotnet' into $vmr_dir.."
  git clone https://github.com/dotnet/dotnet "$vmr_dir"
else
  # This makes sure we don't leave any local changes in the VMR
  highlight "Resetting $vmr_dir"
  git -C "$vmr_dir" reset --hard
  git -C "$vmr_dir" checkout "$vmr_branch"
  git -C "$vmr_dir" pull
fi

set -e

# These lines makes sure the temp dir (which the tooling would clone)
# has the synchronized commit inside as well
highlight 'Preparing the temporary directory..'
rm -rf "${tmp_dir:?}/$INSTALLER_TMP_DIR_NAME"
git clone "$installer_dir" "${tmp_dir:?}/$INSTALLER_TMP_DIR_NAME"

# Prepare darc
highlight 'Installing .NET, preparing the tooling..'
source "$scriptroot/common/tools.sh"
InitializeDotNetCli true
dotnet="$scriptroot/../.dotnet/dotnet"
"$dotnet" tool restore

# Run the sync
target_sha=$(git -C "$installer_dir" rev-parse HEAD)
highlight "Starting the synchronization to $target_sha.."
"$dotnet" darc vmr update --vmr "$vmr_dir" --tmp "$tmp_dir" --debug --recursive installer:$target_sha
