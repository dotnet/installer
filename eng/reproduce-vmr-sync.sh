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
###       Path to the 'dotnet/installer' repo (needs to have the PR commit)
###   --tmp-dir
###       Path to the temporary folder where the repositories will be cloned
###   --vmr-branch
###       Branch of the 'dotnet/dotnet' repo to synchronize to, defaults to 'main'

function print_help
{
    sed -n '/^### /,/^$/p' "${BASH_SOURCE[0]}" | cut -b 5-
}

installer_dir=''
temp_dir=''
vmr_branch='main'

while [[ $# -gt 0 ]]; do
  opt="$(echo "$1" | tr "[:upper:]" "[:lower:]")"
  case "$opt" in
    --tmp-dir)
      temp_dir=$2
      shift
      ;;
    --installer-dir)
      installer_dir=$2
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
      echo "Invalid argument: $1"
      usage
      exit 1
      ;;
  esac

  shift
done


