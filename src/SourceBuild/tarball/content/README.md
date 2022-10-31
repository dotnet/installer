# dotnet/dotnet - Home of the .NET VMR

This repository is a **Virtual Monolithic Repository (VMR)**. It includes all the code needed to build the .NET SDK, mirrored from many atomic repos (like `dotnet/runtime`). It also includes [source-build](https://github.com/dotnet/source-build), our whole-product build system. The VMR is currently experimental.

What this means:
- **Monolithic** - a join of multiple individual repositories that make up the whole product, such as [dotnet/runtime](https://github.com/dotnet/runtime) or [dotnet/sdk](https://github.com/dotnet/sdk).
- **Virtual** - a mirror (not replacement) of product repos where sources from those repositories are synchronized into.
- **Experimental** - not to be depended on as we reserve the right to delete the current instance and create a new, different one in its stead.

In the VMR, you can find:
- [source files of each product repository](#list-of-components) which are mirrored inside of their respective directories under [`src/`](src/),
- tooling that enables [building the whole .NET product from source](https://github.com/dotnet/source-build) on Linux platforms,
- small customizations, in the form of patches, applied on top of the original code to make the build possible,
- *[in future]* E2E tests for the whole .NET product.

More in-depth documentation about the VMR can be found in [VMR Design And Operation](https://github.com/dotnet/arcade/blob/main/Documentation/UnifiedBuild/VMR-Design-And-Operation.md#layout).  
See also [dotnet/source-build](https://github.com/dotnet/source-build) for more information about our whole-product source-build.

## Limitations

**This is a work-in-progress and an experiment.**
There are considerable limitations to what is possible at the moment. For an extensive list of current limitations, please see [Temporary Mechanics](https://github.com/dotnet/arcade/blob/main/Documentation/UnifiedBuild/VMR-Design-And-Operation.md#temporary-mechanics).

### Supported platforms / builds

.NET Source-Build is supported on the oldest available .NET SDK feature update for each major release, and on Linux only.
For example, if .NET `6.0.1xx`, `6.0.2xx`, and `7.0.1xx` feature updates are available from [dotnet.microsoft.com](https://dotnet.microsoft.com/en-us/download/dotnet/6.0), Source-Build will support `6.0.1xx` and `7.0.1xx`.

For the latest information about Source-Build support for new .NET versions, please check our [GitHub Discussions page](https://github.com/dotnet/source-build/discussions) for announcements.

It is expected that other platforms will be supported in the future.

### Online build only

Building the product offline is not fully working at the moment. The `--online` switch is needed when [building](#building) as not all dependencies are currently built from source.

### Code flow
For the time being, the source code only flows one way - from the individual repos into the VMR.
Changes done to the VMR are not automatically mirrored back. More details on this process:

- [Source Synchronization Process](https://github.com/dotnet/arcade/blob/main/Documentation/UnifiedBuild/VMR-Design-And-Operation.md#source-synchronization-process)
- [Moving Code and Dependencies between the VMR and Development Repos](https://github.com/dotnet/arcade/blob/main/Documentation/UnifiedBuild/VMR-Design-And-Operation.md#moving-code-and-dependencies-between-the-vmr-and-development-repos)

### Contribution

At this time, the VMR will not accept any changes outside of those necessary for enabling the supported builds to work.
Please, make the changes in the respective development repositories (e.g., [dotnet/runtime](https://github.com/dotnet/runtime) or [dotnet/sdk](https://github.com/dotnet/sdk)) and they will get synchronized into the VMR automatically.

## Goals

This repository eventually aims to become the place from which we release and service future versions of .NET.
The goal is to reduce complexity of the product construction process and thus enable partners and 3rd parties to easily build, test and modify .NET using their custom infrastructure.

Furthermore, we hope to solve other problems that the current multi-repo setup brings:
- Enable developers to make and test changes spanning multiple repositories.
- Fulfill requirements of .NET distro builders such as RedHat or Canonical to natively include .NET in their distribution repositories.
- Make it possible for anyone to build the product without the need for currently used CI systems and inter-repo code flow orchestration. This should simplify scenarios such as client-run testing of bug fixes and improvements. The build should work in an offline environment too for certain platforms.
- Enable the standard [down-/up-stream open-source model](https://github.com/dotnet/arcade/blob/main/Documentation/UnifiedBuild/VMR-Upstream-Downstream.md).
- More efficient pipeline for security fixes during the CVE pre-disclosure process.

## Dev instructions

Please note that **this repository is an experiment and a work-in-progress so it is possible that the build is broken**.
For the latest information about Source-Build support, please watch for announcements posted on our [GitHub Discussions page](https://github.com/dotnet/source-build/discussions).

### Prerequisites

The dependencies for building .NET from source can be found [here](https://github.com/dotnet/runtime/blob/main/docs/workflow/requirements/linux-requirements.md).

### Building

1. Clone the VMR.

   ```bash
   git clone https://github.com/dotnet/dotnet dotnet-dotnet
   ```

2. Prep the source to build on your distro. This downloads a .NET SDK and a number of .NET packages needed to build .NET from source.

    ```bash
    cd dotnet-dotnet
    ./prep.sh
    ```

3. Build the .NET SDK

    ```bash
    ./build.sh --clean-while-building --online
    ```

    This builds the entire .NET SDK from source.
    The resulting SDK is placed at `artifacts/x64/Release/dotnet-sdk-7.0.100-your-RID.tar.gz`.

    Currently, the `--online` flag is required to allow NuGet restore from online sources during the build.
    This is useful for testing unsupported releases that don't yet build without downloading pre-built binaries from the internet.

    Run `./build.sh --help` to see more information about supported build options.

4. (Optional) Unpack and install the .NET SDK

    ```bash
    mkdir -p $HOME/dotnet
    tar zxf artifacts/x64/Release/dotnet-sdk-7.0.100-your-RID.tar.gz -C $HOME/dotnet
    ln -s $HOME/dotnet/dotnet /usr/bin/dotnet
    ```
    
    To test your source-built SDK, run the following:

    ```bash
    dotnet --info
    ```

## List of components

To enable full offline source-building of the VMR, we have no other choice than to synchronize all the necessary code into the VMR. This also includes any code referenced via git submodules. More details on why we need to do and how we do that can be found here:
- [Strategy for managing external source dependencies](https://github.com/dotnet/arcade/blob/main/Documentation/UnifiedBuild/VMR-Strategy-For-External-Source.md)
- [Source Synchronization Process](https://github.com/dotnet/arcade/blob/main/Documentation/UnifiedBuild/VMR-Design-And-Operation.md#source-synchronization-process)

### Detailed list

<!-- component list beginning -->

> Auto-generated list of components will go here

<!-- component list end -->

The repository also contains a [JSON manifest](https://github.com/dotnet/dotnet/blob/main/src/source-manifest.json) listing all components in a machine-readable format.

## Filing Issues

This repo should only contain issues that are tied to the VMR construction/build itself. For other issues, please file them to their appropriate development repos.

## Useful Links

- Design documentation for the VMR - a set of documents describing the high-level design and the why's and how's
  - [Design and Operation](https://github.com/dotnet/arcade/blob/main/Documentation/UnifiedBuild/VMR-Design-And-Operation.md)
  - [Upstream/Downstream Relationships](https://github.com/dotnet/arcade/blob/main/Documentation/UnifiedBuild/VMR-Upstream-Downstream.md)
  - [Code and Build Workflow](https://github.com/dotnet/arcade/blob/main/Documentation/UnifiedBuild/VMR-Code-And-Build-Workflow.md)
  - [Strategy for Managing External Source Dependencies](https://github.com/dotnet/arcade/blob/main/Documentation/UnifiedBuild/VMR-Strategy-For-External-Source.md)
- [.NET Source-Build](https://github.com/dotnet/source-build)
- [What is .NET](https://dotnet.microsoft.com)

## .NET Foundation

.NET Runtime is a [.NET Foundation](https://www.dotnetfoundation.org/projects) project.

## License

.NET (including the runtime repo) is licensed under the [MIT](LICENSE.TXT) license.
