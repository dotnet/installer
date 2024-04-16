#!/usr/bin/env bash

### Usage: $0
###
###   Publishes SDK and License Scan test results to a PR.
###
### Parameters:
###
### Required:
###   --pipelineId, -p             Pipeline ID of the pipeline that generated the test results.
###   --buildId, -B                Build ID of the pipeline that generated the test results.
###   --updatedResultsPath, -u     Path to the updated test results.
###
### Optional:
###   --title, -T                  Title of the PR. Defaults to 'Create Test Results PR'.
###   --targetBranch, -b           Target branch of the PR. Defaults to 'main'.
###   --forkRepo, -f               Fork repo to create the PR in. Defaults to 'dotnet/installer'.
###   --targetRepo, -t             Target repo to create the PR in. Defaults to 'dotnet/installer'.
###   --targetResultsPath, -r      Path to the test results. Defaults to 'src/SourceBuild/content/test/Microsoft.DotNet.SourceBuild.SmokeTests/assets'.

set -euo pipefail

function print_help () {
    sed -n '/^### /,/^$/p' "$source" | cut -b 5-
}

SOURCE_BUILD_SDK_DIFF_TESTS_PIPELINE_ID=1231
SOURCE_BUILD_LICENSE_SCAN_PIPELINE_ID=1301

# Set default values
pipeline_id=''
build_id=''
title='Update Test Baselines'
target_branch='main'
fork_repo='dotnet/installer'
target_repo='dotnet/installer'
pr_branch_name=''
body="Updates baselines from an internal Microsoft build."
updated_results_path=''
target_results_path='src/SourceBuild/content/test/Microsoft.DotNet.SourceBuild.SmokeTests/assets'

function IsSdkDiffTestsPipeline() {
    if [[ "$pipeline_id" == "$SOURCE_BUILD_SDK_DIFF_TESTS_PIPELINE_ID" ]]; then
        echo "true"
    else
        echo "false"
    fi
}

function IsLicenseScanPipeline() {
    if [[ "$pipeline_id" == "$SOURCE_BUILD_LICENSE_SCAN_PIPELINE_ID" ]]; then
        echo "true"
    else
        echo "false"
    fi
}

positional_args=()
while :; do
  if [ $# -le 0 ]; then
    break
  fi
  lowerI="$(echo "$1" | awk '{print tolower($0)}')"
  case $lowerI in
    "-?"|-h|--help)
      print_help
      exit 0
      ;;
    --pipelineId|-p)
      pipeline_id=$2
      shift
      if [[ $(IsSdkDiffTestsPipeline) == 'false' ]] && [[ $(IsLicenseScanPipeline) == 'false' ]]; then
        echo "This script does not support pipeline $pipeline_id."
        exit 1
      fi
      ;;
    --buildId|-b)
      build_id=$2
      shift
      ;;
    --title|-T)
      pullRequestTitle=$2
      shift
      ;;
    --targetBranch|-B)
      # Removes the internal/ prefix from the target branch if it is present.
      target_branch=$(echo "$2" | sed 's/internal\///')
      shift
      ;;
    --updatedResultsPath|-u)
        updated_results_path=$2
        if [[ ! -d "$updated_results_path" ]]; then
          echo "Test results path $updated_results_path does not exist."
          exit 1
        fi
        shift
        ;;
    --targetResultsPath|-r)
        target_results_path=$2
        shift
        ;;
    --forkRepo|-f)
        forkRepo=$2
        shift
        ;;
    --targetRepo|-t)
        targetRepo=$2
        shift
        ;;
    *)
      positional_args+=("$1")
      ;;
  esac

  shift
done

if [[ -z "$pipeline_id" ]]; then
    echo "Pipeline ID is required."
    exit 1
fi

if [[ -z "$build_id" ]]; then
    echo "Build ID is required."
    exit 1
fi

if [[ -z "$updated_results_path" ]]; then
    echo "Updated results path is required."
    exit 1
fi

function ConfigureGitRepo() {
    time=$(date +%s)
    monthDayYear=$(date +"%m-%d-%y")

    gh auth setup-git

    fork_url="https://github.com/${fork_repo}"
    target_url="https://github.com/${target_repo}"

    repo_dir="repo-${time}"
    git clone "${fork_url}" "${repo_dir}" --depth 1
    cd "${repo_dir}"
    git remote add upstream "${target_url}"

    git fetch upstream "${target_branch}"

    # Ensure that the target path exists
    if [[ ! -d "$target_results_path" ]]; then
        echo "Target results path $target_results_path does not exist."
        exit 1
    fi

    pr_branch_name="${monthDayYear}-source-build-tests-${time}"
    git checkout -b "${pr_branch_name}" "upstream/${target_branch}"

    git config --global user.name "dotnet-sb-bot"
    git config --global user.email "dotnet-sb-bot@microsoft.com"

    body="Updates baselines from $pipeline_id build [$build_id](https://dev.azure.com/dnceng/internal/_build/results?buildId=$build_id&view=results) (internal Microsoft link)."
}

function GetOriginalFileName() {
    updated_file=$1

    # Remove "Updated" from the filename.
    echo $(basename $updated_file | sed 's/Updated//g')
}

# Combines all the lines from the updated test files removes duplicates, and 
# removes any lines from the original test file that are in the updated test files.
# Assumes that the original test file exists and all updated files are subsets of the original file.
function UnionExclusions() {
    original_test_file=$1
    shift
    updated_test_files=$@

    # Combine all the lines from the updated test files and remove duplicates.
    cat "${updated_test_files[@]}" | sort | uniq > temp.txt

    # Remove any lines from the original test file that are in the updated test files.
    awk 'NR==FNR { lines[$0] = 1; next } $0 in lines' temp.txt $original_test_file > temp2.txt && mv temp2.txt $original_test_file

    rm temp.txt

    git add $original_test_file
}

# Removes lines from the original test file that are not in all the updated test files.
# Assumes that the original test file exists and all updated files are subsets of the original file.
function IntersectionExclusions() {
    original_test_file=$1
    shift
    updated_test_files=$@

    for file in $updated_test_files; do
        # Remove any lines from the original test file that are not in the updated test file.
        grep -F -x -f $file $original_test_file > temp.txt && mv temp.txt $original_test_file
    done

    git add $original_test_file
}

# Copy new files to the destination path and update git.
# Assumes that the updated files are named "Updated<original_filename>"
# and that the original file does not exist in the destination path.
function CopyNewFilesAndUpdateGit() {
    destination_path=$1
    shift
    updated_files="$@"

    relative_path=$(realpath --relative-to="$(pwd)" "$destination_path")

    for updated_file in $updated_files; do
        original_filename=$(GetOriginalFileName $updated_file)
        cp $updated_file $destination_path/$original_filename
        git add $destination_path/$original_filename

        body+=$'\n'
        body+=" - Could not find $original_filename in $relative_path."
        body+=" The updated file has been added in this PR."
        body+=$'\n'
    done
}

# Create a new license baseline file if the updated test file is not the default baseline content.
function CreateLicenseBaseline() {
    destination_path=$1
    updated_test_file=$2
    default_baseline_content=$'{\n  "files": []\n}'
    baseline_path="$destination_path/baselines/licenses"

    if [[ "$(cat $updated_test_file)" != $default_baseline_content ]]; then
        CopyNewFilesAndUpdateGit $baseline_path $updated_test_file
    fi
}

function MakePrChanges() {
    relative_target_path="src/SourceBuild/content/test/Microsoft.DotNet.SourceBuild.SmokeTests/assets"
    absolute_target_path=$(realpath $relative_target_path)

    # Create an associative array to group updated test files by their original filename.
    # The key is the original filename before the first period (e.g. "Exclusions" for "Exclusions.Sample.txt")
    declare -A files
    updated_test_files=$(find "$updated_results_path" -name "Updated*");
    for test_file in $updated_test_files; do
        filename=$(GetOriginalFileName $test_file | sed 's/\..*$//')
        files["$filename"]+=" $test_file"
    done

    for filename in "${!files[@]}"; do
        read -a updated_test_files <<< "${files[$filename]}"

        if [[ $filename == *"Exclusions"* ]]; then
            # Exclusion files get combined.
            # There can only be one exclusion file per key, so we need to check for a file matching the key.

            original_test_file=$(find "$absolute_target_path" -name "${filename}.*")
            if [ -z "$original_test_file" ]; then
                CopyNewFilesAndUpdateGit $absolute_target_path $updated_test_files
            elif [[ $(IsSdkDiffTestsPipeline) == 'true' ]]; then
                UnionExclusions $original_test_file $updated_test_files
            elif [[ $(IsLicenseScanPipeline) == 'true' ]]; then
                IntersectionExclusions $original_test_file $updated_test_files
            else
                echo "Unsure of how to combine exclusions files for pipeline type. Exiting."
                exit 1
            fi
        else
        for updated_test_file in $updated_test_files; do
            # Non-exclusion files get copied over.
            # There can be multiple files per key, so we need to check for a file matching each updated file.

            original_test_filename=$(GetOriginalFileName $updated_test_file)
            original_test_filepath=$(find "$absolute_target_path" -name "$original_test_filename")
            if [ -z "$original_test_filepath" ]; then
                if [[ $(IsLicenseScanPipeline) == 'true' ]]; then
                    CreateLicenseBaseline $absolute_target_path $updated_test_file
                else
                    CopyNewFilesAndUpdateGit $absolute_target_path $updated_test_file
                fi
            else
                cp $updated_test_file $original_test_filepath
                git add $original_test_filepath
            fi
        done
        fi
    done
}

function CreatePr() {
    if [[ -z $(git status --porcelain) ]]; then
        echo "No changes to commit. Exiting."
        exit 0
    fi

    git commit -m "Update test baselines from pipeline $pipeline_id build $build_id."
    git push -u origin "${pr_branch_name}"

    readarray -d '/' -t fork_repo_split <<< "${fork_repo}"
    fork_owner="${fork_repo_split[0]}"

    echo "TargetRepo: $target_repo"
    echo "ForkRepo: $fork_repo"
    echo "Title: $title"
    echo "Body: $body"

    # create pull request
    gh pr create \
        --head "${fork_owner}:${pr_branch_name}" \
        --repo "${target_repo}" \
        --base "${target_branch}" \
        --title "${title}" \
        --body "${body}"
}

ConfigureGitRepo
MakePrChanges
CreatePr