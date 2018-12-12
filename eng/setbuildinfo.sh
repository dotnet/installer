#!/bin/bash

shopt -s nocasematch

if [[ "$1" == ARM* ]]
then
    echo "ARM"
    echo "##vso[task.setvariable variable=TestParameter]"
    echo "##vso[task.setvariable variable=RunTests]false"
else
    echo "NOT ARM"
    echo "##vso[task.setvariable variable=TestParameter]--test"
    echo "##vso[task.setvariable variable=RunTests]true"
fi

if [[ "$AdditionalBuildParameters" == '$(_AdditionalBuildParameters)' ]]
then
    echo "##vso[task.setvariable variable=AdditionalBuildParameters]"
fi