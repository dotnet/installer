#!/usr/bin/env bash

source="${BASH_SOURCE[0]}"
script_root="$( cd -P "$( dirname "$source" )" && pwd )"

repo_root=$(realpath "$script_root/../..")
tmp_dir="$repo_root/artifacts/tmp"
vmr_dir="$tmp_dir/artifacts/tmp/dotnet"

# We will try to figure out, which branch is the current (PR) branch based off of
# We need this to figure out, which VMR branch to use
vmr_branch=$(git log --pretty=format:'%D' HEAD^ | grep 'origin/' | head -n1 | sed 's@origin/@@' | sed 's@,.*@@')

"$script_root"/../../eng/vmr-sync.sh \
    --vmr "$vmr_dir"                \
    --tmp "$tmp_dir"                \
    --branch "$vmr_branch"
