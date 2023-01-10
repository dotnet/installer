#!/usr/bin/env bash

source="${BASH_SOURCE[0]}"
script_root="$( cd -P "$( dirname "$source" )" && pwd )"

# TODO: Move this into the image?
dnf -y install openssl1.1.x86_64

installer_root=$(realpath "$script_root/../..")
tmp_dir=$(realpath "$installer_root/../tmp")
vmr_dir=$(realpath "$installer_root/../dotnet")

git -C "$installer_root" fetch --all

"$script_root"/../../eng/vmr-sync.sh \
    --vmr "$vmr_dir" \
    --tmp "$tmp_dir" \
    --branch main    \
    --debug
