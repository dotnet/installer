# Source-Build

This directory contains files necessary to generate a tarball that can be used
to build .NET from source.

For more information, see
[dotnet/source-build](https://github.com/dotnet/source-build).

## Local development workflow

These are the steps used by some members of the .NET source-build team to build
a tarball and build it on a local machine as part of the development cycle:

1. Check out this repository and open a command line in the directory.
1. `./build.sh /p:ArcadeBuildTarball=true /p:TarballDir=/repos/tarball1 /p:PreserveTarballGitFolders=true`
    * The `TarballDir` can be anywhere you want outside of the repository.
1. `cd /repos/tarball1`
1. `./prep.sh`
1. `./build.sh --online`
1. Examine results and make changes to the source code in the tarball. The
   `.git` folders are preserved, so you can commit changes and save them as
   patches.
1. When a repo builds, source-build places a `.complete` file to prevent it from
   rebuilding again. This allows you to incrementally retry a build if there's a
   transient failure. But it also prevents you from rebuilding a repo after
   you've modified it.
    * To force a repo to rebuild with your new changes, run:  
      `rm -f ./artifacts/obj/semaphores/<repo>/Build.complete`
1. Run `./build.sh --online` again, and continue to repeat as necessary.

When developing a prebuilt removal change, examine the results of the build,
specifically:

* Prebuilt report. For example:  
  `./src/runtime.733a3089ec6945422caf06035c18ff700c9d51be/artifacts/source-build/self/prebuilt-report`
