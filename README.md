# .NET SDK Installers

[![GitHub release](https://img.shields.io/github/release/dotnet/installer.svg)](https://GitHub.com/dotnet/installer/releases/)
[![GitHub repo size](https://img.shields.io/github/repo-size/dotnet/installer)](https://github.com/dotnet/installer)
[![GitHub issues-opened](https://img.shields.io/github/issues/dotnet/installer.svg)](https://GitHub.com/dotnet/installer/issues?q=is%3Aissue+is%3Aopened)
[![GitHub issues-closed](https://img.shields.io/github/issues-closed/dotnet/installer.svg)](https://GitHub.com/dotnet/installer/issues?q=is%3Aissue+is%3Aclosed)
[![GitHub pulls-opened](https://img.shields.io/github/issues-pr/dotnet/installer.svg)](https://GitHub.com/dotnet/installer/pulls?q=is%3Aissue+is%3Aopened)
[![GitHub pulls-merged](https://img.shields.io/github/issues-search/dotnet/installer?label=merged%20pull%20requests&query=is%3Apr%20is%3Aclosed%20is%3Amerged&color=darkviolet)](https://github.com/dotnet/installer/pulls?q=is%3Apr+is%3Aclosed+is%3Amerged)
[![GitHub pulls-unmerged](https://img.shields.io/github/issues-search/dotnet/installer?label=unmerged%20pull%20requests&query=is%3Apr%20is%3Aclosed%20is%3Aunmerged&color=red)](https://github.com/dotnet/installer/pulls?q=is%3Apr+is%3Aclosed+is%3Aunmerged)
[![GitHub contributors](https://img.shields.io/github/contributors/dotnet/installer.svg)](https://GitHub.com/dotnet/installer/graphs/contributors/)
[![Commit Activity](https://img.shields.io/github/commit-activity/m/dotnet/installer)]()

This repo contains the source code for the cross-platform [.NET](http://github.com/dotnet/core) SDK. It aggregates the .NET toolchain, the .NET runtime, the templates, and the .NET  Windows Desktop runtime. It produces zip, tarballs, and native packages for various supported platforms.

For the latest daily builds, see [.NET SDK daily builds table](https://github.com/dotnet/sdk/blob/main/documentation/package-table.md#table).

Looking for released versions of the .NET tooling?
----------------------------------------

The links below are for preview versions of .NET tooling. Prefer to use released versions of the .NET tools? Go to https://dot.net/download.

Looking for .NET Framework downloads?
----------------------------------------

.NET Framework is the product from which the .NET Core project originated. .NET Core (mostly just called ".NET" here) adds many features and improvements and supports many more platforms than .NET Framework. .NET Framework remains fully supported and you can find the downloads on the [.NET website](https://dotnet.microsoft.com/download/dotnet-framework). For new projects, we recommend you use .NET Core.

Want to contribute or find out more about the .NET project?
----------------------------------------

This repo is for the installers. Most of the implementation is in other repos, such as the [dotnet/runtime repo](https://github.com/dotnet/runtime) or the [dotnet/aspnetcore repo](https://github.com/dotnet/aspnetcore) and [many others](https://github.com/dotnet/core/blob/main/Documentation/core-repos.md). We welcome you to join us there!

Found an issue?
---------------
You can consult the [Documents Index for the SDK repo](https://github.com/dotnet/sdk/blob/main/documentation/README.md) to find out current issues, see workarounds, and to see how to file new issues.

This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/) to clarify expected behavior in our community. For more information, see the [.NET Foundation Code of Conduct](http://www.dotnetfoundation.org/code-of-conduct).

# Build .NET installer

The repository contains native code project required for the Windows installer. If you intend to build it locally on Windows, you will need to ensure that you have the following items installed.
- Install CMAKE 3.21.0 is required if you're building VS 17.0. Make sure to add CMAKE to your PATH (the installer will prompt you).
- Install MSVC Build tools for x86/x64/arm64, v14.28-16.9

- `build` for basic build
- `build -pack` to build the installer
- To build in VS, run a command line build first, then run `artifacts\core-sdk-build-env.bat` from a VS command prompt and then `devenv Microsoft.DotNet.Cli.sln`
- To test different languages of the installer, run `artifacts\packages\Debug\Shipping>dotnet-sdk-3.1.412-win-x64.exe /lang 1046` using the LCID of the language you want to test

# Build .NET from source (source-build)

Please see the [dotnet/source-build](https://github.com/dotnet/source-build) repo for more information.

## Support

.NET Source-Build is supported on the oldest available .NET SDK feature update for each major release, and on Linux only.
For example, if .NET 6.0.1xx, 6.0.2xx, and 7.0.1xx feature updates are available from [dotnet.microsoft.com](https://dotnet.microsoft.com/en-us/download/dotnet/6.0), Source-Build will support 6.0.1xx and 7.0.1xx.
For the latest information about Source-Build support for new .NET versions, please check our [GitHub Discussions page](https://github.com/dotnet/source-build/discussions) for announcements.

## Building .NET 8.0

.NET 8.0 (currently in prerelease) and newer will be built from the [dotnet/dotnet](https://github.com/dotnet/dotnet) repo.
Clone the dotnet/dotnet repo and check out the tag for the desired release.
Then, follow the instructions in [dotnet/dotnet's README](https://github.com/dotnet/dotnet/blob/main/README.md#dev-instructions) to build .NET from source.

## Building .NET 7.0 and .NET 6.0

1. Create a .NET source tarball.

   ```bash
   ./build.sh /p:ArcadeBuildTarball=true /p:TarballDir=/path/to/place/complete/dotnet/sources
   ```

   This fetches the complete .NET source code and creates a tarball at `artifacts/packages/<Release|Debug>/Shipping/`.
   The extracted source code is also placed at `/path/to/place/complete/dotnet/sources`.
   The source directory should be outside (and not somewhere under) the installer directory.

2. Prep the source to build on your distro. This downloads a .NET SDK and a number of .NET packages needed to build .NET from source.

    ```bash
    cd /path/to/complete/dotnet/sources
    ./prep.sh --bootstrap
    ```

3. Build the .NET SDK

    ```bash
    ./build.sh --clean-while-building
    ```

    This builds the entire .NET SDK from source.
    The resulting SDK is placed at `artifacts/x64/Release/dotnet-sdk-7.0.100-your-RID.tar.gz`.

    Optionally add the `--online` flag to add online NuGet restore sources to the build.
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

# Build status

Visibility|All legs|
|:------|:------|
|Public|[![Status](https://dev.azure.com/dnceng-public/public/_apis/build/status%2Fdotnet%2Finstaller%2Finstaller?branchName=main)](https://dev.azure.com/dnceng-public/public/_build/latest?definitionId=20&branchName=main)|
|Microsoft Internal|[![Status](https://dev.azure.com/dnceng/internal/_apis/build/status/286)](https://dev.azure.com/dnceng/internal/_build?definitionId=286)|

## Installers and Binaries

You can download the .NET SDK as either an installer (MSI, PKG) or a zip (zip, tar.gz). The .NET SDK contains both the .NET runtime and CLI tools.

**Note:** Be aware that the following installers are the **latest bits**. If you
want to install the latest released versions, check out the [preceding section](#looking-for-released-versions-of-the-net-core-tooling).
With development builds, internal NuGet feeds are necessary for some scenarios (for example, to acquire the runtime pack for self-contained apps). You can use the following NuGet.config to configure these feeds. See the following document [Configuring NuGet behavior](https://docs.microsoft.com/en-us/nuget/consume-packages/configuring-nuget-behavior) for more information on where to modify your NuGet.config to apply the changes.

**For .NET 9 builds**

```xml
<configuration>
  <packageSources>
    <add key="dotnet9" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet9/nuget/v3/index.json" />
  </packageSources>
</configuration>
```

**For .NET 8 builds**

```xml
<configuration>
  <packageSources>
    <add key="dotnet8" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet8/nuget/v3/index.json" />
  </packageSources>
</configuration>
```

**For .NET 7 builds**

```xml
<configuration>
  <packageSources>
    <add key="dotnet7" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet7/nuget/v3/index.json" />
  </packageSources>
</configuration>
```

Do not directly edit the table below. Use https://github.com/dotnet/installer/tree/main/tools/sdk-readme-table-generator to help you generate it. Make sure to run the table generator test and make any changes to the generator along with your changes to the table. Daily servicing builds aren't shown here because they may contain upcoming security fixes. All public servicing builds can be downloaded at http://aka.ms/dotnet-download.

### .NET SDK daily builds table

[.NET SDK daily builds table](https://github.com/dotnet/sdk/blob/main/documentation/package-table.md#table).

Looking for dotnet-install sources?
-----------------------------------

Sources for dotnet-install.sh and dotnet-install.ps1 are in the [install-scripts repo](https://github.com/dotnet/install-scripts).

Questions & Comments
--------------------

For all feedback, use the Issues on the [.NET SDK](https://github.com/dotnet/sdk) repository.

License
-------

The .NET project uses the [MIT license](LICENSE).

The LICENSE and ThirdPartyNotices in any downloaded archives are authoritative.
