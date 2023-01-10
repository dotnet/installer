#!/usr/bin/env bash

source="${BASH_SOURCE[0]}"
script_root="$( cd -P "$( dirname "$source" )" && pwd )"

repo_root=$(realpath "$script_root/../..")
tmp_dir="$repo_root/artifacts/tmp"
vmr_dir="$tmp_dir/artifacts/tmp/dotnet"

git -C "$repo_root" fetch --all

"$script_root"/../../eng/vmr-sync.sh \
    --vmr "$vmr_dir" \
    --tmp "$tmp_dir" \
    --branch main    \
    --debug
