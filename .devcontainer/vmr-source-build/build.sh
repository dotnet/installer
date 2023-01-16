#!/usr/bin/env bash

set -ex

source="${BASH_SOURCE[0]}"
script_root="$( cd -P "$( dirname "$source" )" && pwd )"

# TODO: Remove once we have a new version of the image
dnf -y install openssl1.1.x86_64

installer_dir=$(realpath "$script_root/../..")
workspace_dir=$(realpath "$installer_dir/../")
tmp_dir=$(realpath "$workspace_dir/tmp")
vmr_dir=$(realpath "$workspace_dir/dotnet")

mkdir -p "$tmp_dir"

# installer_sha=$(git -C "$installer_dir" rev-parse HEAD)
# cp "$installer_dir/.git/config" "$tmp_dir/git_config"
# rm -rf "$installer_dir"
# mkdir -p "$installer_dir"
# pushd "$installer_dir"
# git init
# mv "$tmp_dir/git_config" ./.git/config
# git fetch --all
# git checkout "$installer_sha"

./eng/vmr-sync.sh    \
    --vmr "$vmr_dir" \
    --tmp "$tmp_dir" \
    --branch main    \
    --debug

popd
