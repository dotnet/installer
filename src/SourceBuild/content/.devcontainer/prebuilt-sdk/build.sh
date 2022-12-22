#!/usr/bin/env bash

./prep.sh

# GitHub Codespaces automatically sets RepositoryName, which conflicts with source-build scripts.
unset RepositoryName

./build.sh --online --clean-while-building || true
