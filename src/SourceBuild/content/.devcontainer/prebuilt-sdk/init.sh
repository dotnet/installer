#!/usr/bin/env bash

source="${BASH_SOURCE[0]}"
script_root="$( cd -P "$( dirname "$source" )" && pwd )"

"$script_root"/../../prep.sh

cp "$script_root/../synchronize-vmr.sh" "$script_root/../.."

# GitHub Codespaces sets this and it conflicts with source-build scripts.
unset RepositoryName

"$script_root"/../../build.sh --online --clean-while-building || exit 0
