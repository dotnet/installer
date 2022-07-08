#!/usr/bin/env bash

./.devcontainer/source-build-scripts/createTarball.sh

cd $(realpath ..)/dotnet-source/

./prep.sh
./build.sh --online --clean-while-building || true