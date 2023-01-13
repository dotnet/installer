#!/usr/bin/env bash

set -ex

source="${BASH_SOURCE[0]}"
script_root="$( cd -P "$( dirname "$source" )" && pwd )"

# TODO: Move this into the image?
dnf -y install openssl1.1.x86_64

installer_dir=$(realpath "$script_root/../..")
tmp_dir=$(realpath "$installer_dir/../tmp")
vmr_dir=$(realpath "$installer_dir/../dotnet")

mkdir -p "$tmp_dir"

installer_sha=$(git -C "$installer_dir" rev-parse HEAD)
cp "$installer_dir/.git/config" "$tmp_dir/git_config"
rm -rf "$installer_dir"
mkdir -p "$installer_dir"
git -C "$installer_dir" init
mv "$tmp_dir/git_config" "$installer_dir/.git/config"
git -C "$installer_dir" fetch --all
git -C "$installer_dir" checkout "$installer_sha"

"$script_root"/../../eng/vmr-sync.sh \
    --vmr "$vmr_dir" \
    --tmp "$tmp_dir" \
    --branch main    \
    --debug
