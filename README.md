# .NET Core SDK

[![Join the chat at https://gitter.im/dotnet/cli](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/dotnet/cli?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This repo contains the source code for the cross-platform [.NET Core](http://github.com/dotnet/core) SDK. It aggregates the .NET Toolchain, the .NET Core runtime, the templates, and the .NET Core Windows Desktop runtime. It produces zip, tarballs, and native packages for various supported platforms.

Looking for released versions of the .NET Core tooling?
----------------------------------------

Download released versions of the .NET Core tools (CLI, MSBuild and the new csproj) at https://dot.net/core.

Found an issue?
---------------
You can consult the [Documents Index for the CLI repo](https://github.com/dotnet/cli/blob/master/Documentation/README.md) to find out current issues, see workarounds, and to see how to file new issues.

This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/) to clarify expected behavior in our community. For more information, see the [.NET Foundation Code of Conduct](http://www.dotnetfoundation.org/code-of-conduct).

# Build status

|All legs|
|:------:|
|[![Build Status](https://dev.azure.com/dnceng/internal/_apis/build/status/286)](https://dev.azure.com/dnceng/internal/_build?definitionId=286)|

Installers and Binaries
-----------------------

You can download the .NET Core SDK as either an installer (MSI, PKG) or a zip (zip, tar.gz). The .NET Core SDK contains both the .NET Core runtime and CLI tools.

**Note:** Be aware that the following installers are the **latest bits**. If you
want to install the latest released versions, check out the [preceding section](#looking-for-v2-of-the-net-core-tooling).
With development builds, internal NuGet feeds are necessary for some scenarios (for example, to acquire the runtime pack for self-contained apps). You can use the following NuGet.config to configure these feeds. See the following document [Configuring NuGet behavior](https://docs.microsoft.com/en-us/nuget/consume-packages/configuring-nuget-behavior) for more information on where to modify your NuGet.config to apply the changes.
> Example:

**For .NET 6 builds**

```
<configuration>
  <packageSources>
    <add key="dotnet6" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet6/nuget/v3/index.json" />
  </packageSources>
</configuration>
```
**Note:** that you may need to add the dotnet5 feed for a short period of time while .NET transitions to .NET 6

**For .NET 5 builds**

```
<configuration>
  <packageSources>
    <add key="dotnet5" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet5/nuget/v3/index.json" />
  </packageSources>
</configuration>
```

**For .NET 3.1 builds**

```
<configuration>
  <packageSources>
    <add key="dotnet3.1" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet3.1/nuget/v3/index.json" />
  </packageSources>
</configuration>
```

| Platform | Master<br>(6.0.x&nbsp;Runtime) | 5.0.100 RC 2<br>(5.0 Runtime) | 5.0.100 RC 1<br>(5.0 Runtime) | Release/3.1.4XX<br>(3.1.x Runtime) | Release/3.1.3XX<br>(3.1.x Runtime) | Release/3.1.2XX<br>(3.1.x Runtime) | Release/3.1.1XX<br>(3.1.x Runtime) | Release/3.0.1xx<br>(3.0.x Runtime) |
| :--------- | :----------: | :----------: | :----------: | :----------: | :----------: | :----------: | :----------: | :----------: |
| **Windows x64** | [![][win-x64-badge-master]][win-x64-version-master]<br>[Installer][win-x64-installer-master] - [Checksum][win-x64-installer-checksum-master]<br>[zip][win-x64-zip-master] - [Checksum][win-x64-zip-checksum-master] | [![][win-x64-badge-5.0.1XX-rc2]][win-x64-version-5.0.1XX-rc2]<br>[Installer][win-x64-installer-5.0.1XX-rc2] - [Checksum][win-x64-installer-checksum-5.0.1XX-rc2]<br>[zip][win-x64-zip-5.0.1XX-rc2] - [Checksum][win-x64-zip-checksum-5.0.1XX-rc2] | [![][win-x64-badge-5.0.1XX-rc1]][win-x64-version-5.0.1XX-rc1]<br>[Installer][win-x64-installer-5.0.1XX-rc1] - [Checksum][win-x64-installer-checksum-5.0.1XX-rc1]<br>[zip][win-x64-zip-5.0.1XX-rc1] - [Checksum][win-x64-zip-checksum-5.0.1XX-rc1] | [![][win-x64-badge-3.1.4XX]][win-x64-version-3.1.4XX]<br>[Installer][win-x64-installer-3.1.4XX] - [Checksum][win-x64-installer-checksum-3.1.4XX]<br>[zip][win-x64-zip-3.1.4XX] - [Checksum][win-x64-zip-checksum-3.1.4XX] | [![][win-x64-badge-3.1.3XX]][win-x64-version-3.1.3XX]<br>[Installer][win-x64-installer-3.1.3XX] - [Checksum][win-x64-installer-checksum-3.1.3XX]<br>[zip][win-x64-zip-3.1.3XX] - [Checksum][win-x64-zip-checksum-3.1.3XX] | [![][win-x64-badge-3.1.2XX]][win-x64-version-3.1.2XX]<br>[Installer][win-x64-installer-3.1.2XX] - [Checksum][win-x64-installer-checksum-3.1.2XX]<br>[zip][win-x64-zip-3.1.2XX] - [Checksum][win-x64-zip-checksum-3.1.2XX] | [![][win-x64-badge-3.1.1XX]][win-x64-version-3.1.1XX]<br>[Installer][win-x64-installer-3.1.1XX] - [Checksum][win-x64-installer-checksum-3.1.1XX]<br>[zip][win-x64-zip-3.1.1XX] - [Checksum][win-x64-zip-checksum-3.1.1XX] | [![][win-x64-badge-3.0.1XX]][win-x64-version-3.0.1XX]<br>[Installer][win-x64-installer-3.0.1XX] - [Checksum][win-x64-installer-checksum-3.0.1XX]<br>[zip][win-x64-zip-3.0.1XX] - [Checksum][win-x64-zip-checksum-3.0.1XX] |
| **Windows x86** | [![][win-x86-badge-master]][win-x86-version-master]<br>[Installer][win-x86-installer-master] - [Checksum][win-x86-installer-checksum-master]<br>[zip][win-x86-zip-master] - [Checksum][win-x86-zip-checksum-master] | [![][win-x86-badge-5.0.1XX-rc2]][win-x86-version-5.0.1XX-rc2]<br>[Installer][win-x86-installer-5.0.1XX-rc2] - [Checksum][win-x86-installer-checksum-5.0.1XX-rc2]<br>[zip][win-x86-zip-5.0.1XX-rc2] - [Checksum][win-x86-zip-checksum-5.0.1XX-rc2] | [![][win-x86-badge-5.0.1XX-rc1]][win-x86-version-5.0.1XX-rc1]<br>[Installer][win-x86-installer-5.0.1XX-rc1] - [Checksum][win-x86-installer-checksum-5.0.1XX-rc1]<br>[zip][win-x86-zip-5.0.1XX-rc1] - [Checksum][win-x86-zip-checksum-5.0.1XX-rc1] | [![][win-x86-badge-3.1.4XX]][win-x86-version-3.1.4XX]<br>[Installer][win-x86-installer-3.1.4XX] - [Checksum][win-x86-installer-checksum-3.1.4XX]<br>[zip][win-x86-zip-3.1.4XX] - [Checksum][win-x86-zip-checksum-3.1.4XX] | [![][win-x86-badge-3.1.3XX]][win-x86-version-3.1.3XX]<br>[Installer][win-x86-installer-3.1.3XX] - [Checksum][win-x86-installer-checksum-3.1.3XX]<br>[zip][win-x86-zip-3.1.3XX] - [Checksum][win-x86-zip-checksum-3.1.3XX] | [![][win-x86-badge-3.1.2XX]][win-x86-version-3.1.2XX]<br>[Installer][win-x86-installer-3.1.2XX] - [Checksum][win-x86-installer-checksum-3.1.2XX]<br>[zip][win-x86-zip-3.1.2XX] - [Checksum][win-x86-zip-checksum-3.1.2XX] | [![][win-x86-badge-3.1.1XX]][win-x86-version-3.1.1XX]<br>[Installer][win-x86-installer-3.1.1XX] - [Checksum][win-x86-installer-checksum-3.1.1XX]<br>[zip][win-x86-zip-3.1.1XX] - [Checksum][win-x86-zip-checksum-3.1.1XX] | [![][win-x86-badge-3.0.1XX]][win-x86-version-3.0.1XX]<br>[Installer][win-x86-installer-3.0.1XX] - [Checksum][win-x86-installer-checksum-3.0.1XX]<br>[zip][win-x86-zip-3.0.1XX] - [Checksum][win-x86-zip-checksum-3.0.1XX] |
| **macOS** | [![][osx-badge-master]][osx-version-master]<br>[Installer][osx-installer-master] - [Checksum][osx-installer-checksum-master]<br>[tar.gz][osx-targz-master] - [Checksum][osx-targz-checksum-master] | [![][osx-badge-5.0.1XX-rc2]][osx-version-5.0.1XX-rc2]<br>[Installer][osx-installer-5.0.1XX-rc2] - [Checksum][osx-installer-checksum-5.0.1XX-rc2]<br>[tar.gz][osx-targz-5.0.1XX-rc2] - [Checksum][osx-targz-checksum-5.0.1XX-rc2] | [![][osx-badge-5.0.1XX-rc1]][osx-version-5.0.1XX-rc1]<br>[Installer][osx-installer-5.0.1XX-rc1] - [Checksum][osx-installer-checksum-5.0.1XX-rc1]<br>[tar.gz][osx-targz-5.0.1XX-rc1] - [Checksum][osx-targz-checksum-5.0.1XX-rc1] | [![][osx-badge-3.1.4XX]][osx-version-3.1.4XX]<br>[Installer][osx-installer-3.1.4XX] - [Checksum][osx-installer-checksum-3.1.4XX]<br>[tar.gz][osx-targz-3.1.4XX] - [Checksum][osx-targz-checksum-3.1.4XX] | [![][osx-badge-3.1.3XX]][osx-version-3.1.3XX]<br>[Installer][osx-installer-3.1.3XX] - [Checksum][osx-installer-checksum-3.1.3XX]<br>[tar.gz][osx-targz-3.1.3XX] - [Checksum][osx-targz-checksum-3.1.3XX] | [![][osx-badge-3.1.2XX]][osx-version-3.1.2XX]<br>[Installer][osx-installer-3.1.2XX] - [Checksum][osx-installer-checksum-3.1.2XX]<br>[tar.gz][osx-targz-3.1.2XX] - [Checksum][osx-targz-checksum-3.1.2XX] | [![][osx-badge-3.1.1XX]][osx-version-3.1.1XX]<br>[Installer][osx-installer-3.1.1XX] - [Checksum][osx-installer-checksum-3.1.1XX]<br>[tar.gz][osx-targz-3.1.1XX] - [Checksum][osx-targz-checksum-3.1.1XX] | [![][osx-badge-3.0.1XX]][osx-version-3.0.1XX]<br>[Installer][osx-installer-3.0.1XX] - [Checksum][osx-installer-checksum-3.0.1XX]<br>[tar.gz][osx-targz-3.0.1XX] - [Checksum][osx-targz-checksum-3.0.1XX] |
| **Linux x64** | [![][linux-badge-master]][linux-version-master]<br>[DEB Installer][linux-DEB-installer-master] - [Checksum][linux-DEB-installer-checksum-master]<br>[RPM Installer][linux-RPM-installer-master] - [Checksum][linux-RPM-installer-checksum-master]<br>_see installer note below_<sup>1</sup><br>[tar.gz][linux-targz-master] - [Checksum][linux-targz-checksum-master] | [![][linux-badge-5.0.1XX-rc2]][linux-version-5.0.1XX-rc2]<br>[DEB Installer][linux-DEB-installer-5.0.1XX-rc2] - [Checksum][linux-DEB-installer-checksum-5.0.1XX-rc2]<br>[RPM Installer][linux-RPM-installer-5.0.1XX-rc2] - [Checksum][linux-RPM-installer-checksum-5.0.1XX-rc2]<br>_see installer note below_<sup>1</sup><br>[tar.gz][linux-targz-5.0.1XX-rc2] - [Checksum][linux-targz-checksum-5.0.1XX-rc2] | [![][linux-badge-5.0.1XX-rc1]][linux-version-5.0.1XX-rc1]<br>[DEB Installer][linux-DEB-installer-5.0.1XX-rc1] - [Checksum][linux-DEB-installer-checksum-5.0.1XX-rc1]<br>[RPM Installer][linux-RPM-installer-5.0.1XX-rc1] - [Checksum][linux-RPM-installer-checksum-5.0.1XX-rc1]<br>_see installer note below_<sup>1</sup><br>[tar.gz][linux-targz-5.0.1XX-rc1] - [Checksum][linux-targz-checksum-5.0.1XX-rc1] | [![][linux-badge-3.1.4XX]][linux-version-3.1.4XX]<br>[DEB Installer][linux-DEB-installer-3.1.4XX] - [Checksum][linux-DEB-installer-checksum-3.1.4XX]<br>[RPM Installer][linux-RPM-installer-3.1.4XX] - [Checksum][linux-RPM-installer-checksum-3.1.4XX]<br>_see installer note below_<sup>1</sup><br>[tar.gz][linux-targz-3.1.4XX] - [Checksum][linux-targz-checksum-3.1.4XX] | [![][linux-badge-3.1.3XX]][linux-version-3.1.3XX]<br>[DEB Installer][linux-DEB-installer-3.1.3XX] - [Checksum][linux-DEB-installer-checksum-3.1.3XX]<br>[RPM Installer][linux-RPM-installer-3.1.3XX] - [Checksum][linux-RPM-installer-checksum-3.1.3XX]<br>_see installer note below_<sup>1</sup><br>[tar.gz][linux-targz-3.1.3XX] - [Checksum][linux-targz-checksum-3.1.3XX] | [![][linux-badge-3.1.2XX]][linux-version-3.1.2XX]<br>[DEB Installer][linux-DEB-installer-3.1.2XX] - [Checksum][linux-DEB-installer-checksum-3.1.2XX]<br>[RPM Installer][linux-RPM-installer-3.1.2XX] - [Checksum][linux-RPM-installer-checksum-3.1.2XX]<br>_see installer note below_<sup>1</sup><br>[tar.gz][linux-targz-3.1.2XX] - [Checksum][linux-targz-checksum-3.1.2XX] | [![][linux-badge-3.1.1XX]][linux-version-3.1.1XX]<br>[DEB Installer][linux-DEB-installer-3.1.1XX] - [Checksum][linux-DEB-installer-checksum-3.1.1XX]<br>[RPM Installer][linux-RPM-installer-3.1.1XX] - [Checksum][linux-RPM-installer-checksum-3.1.1XX]<br>_see installer note below_<sup>1</sup><br>[tar.gz][linux-targz-3.1.1XX] - [Checksum][linux-targz-checksum-3.1.1XX] | [![][linux-badge-3.0.1XX]][linux-version-3.0.1XX]<br>[DEB Installer][linux-DEB-installer-3.0.1XX] - [Checksum][linux-DEB-installer-checksum-3.0.1XX]<br>[RPM Installer][linux-RPM-installer-3.0.1XX] - [Checksum][linux-RPM-installer-checksum-3.0.1XX]<br>_see installer note below_<sup>1</sup><br>[tar.gz][linux-targz-3.0.1XX] - [Checksum][linux-targz-checksum-3.0.1XX] |
| **Linux arm** | [![][linux-arm-badge-master]][linux-arm-version-master]<br>[tar.gz][linux-arm-targz-master] - [Checksum][linux-arm-targz-checksum-master] | [![][linux-arm-badge-5.0.1XX-rc2]][linux-arm-version-5.0.1XX-rc2]<br>[tar.gz][linux-arm-targz-5.0.1XX-rc2] - [Checksum][linux-arm-targz-checksum-5.0.1XX-rc2] | [![][linux-arm-badge-5.0.1XX-rc1]][linux-arm-version-5.0.1XX-rc1]<br>[tar.gz][linux-arm-targz-5.0.1XX-rc1] - [Checksum][linux-arm-targz-checksum-5.0.1XX-rc1] | [![][linux-arm-badge-3.1.4XX]][linux-arm-version-3.1.4XX]<br>[tar.gz][linux-arm-targz-3.1.4XX] - [Checksum][linux-arm-targz-checksum-3.1.4XX] | [![][linux-arm-badge-3.1.3XX]][linux-arm-version-3.1.3XX]<br>[tar.gz][linux-arm-targz-3.1.3XX] - [Checksum][linux-arm-targz-checksum-3.1.3XX] | [![][linux-arm-badge-3.1.2XX]][linux-arm-version-3.1.2XX]<br>[tar.gz][linux-arm-targz-3.1.2XX] - [Checksum][linux-arm-targz-checksum-3.1.2XX] | [![][linux-arm-badge-3.1.1XX]][linux-arm-version-3.1.1XX]<br>[tar.gz][linux-arm-targz-3.1.1XX] - [Checksum][linux-arm-targz-checksum-3.1.1XX] | [![][linux-arm-badge-3.0.1XX]][linux-arm-version-3.0.1XX]<br>[tar.gz][linux-arm-targz-3.0.1XX] - [Checksum][linux-arm-targz-checksum-3.0.1XX] |
| **Linux arm64** | [![][linux-arm64-badge-master]][linux-arm64-version-master]<br>[tar.gz][linux-arm64-targz-master] - [Checksum][linux-arm64-targz-checksum-master] | [![][linux-arm64-badge-5.0.1XX-rc2]][linux-arm64-version-5.0.1XX-rc2]<br>[tar.gz][linux-arm64-targz-5.0.1XX-rc2] - [Checksum][linux-arm64-targz-checksum-5.0.1XX-rc2] | [![][linux-arm64-badge-5.0.1XX-rc1]][linux-arm64-version-5.0.1XX-rc1]<br>[tar.gz][linux-arm64-targz-5.0.1XX-rc1] - [Checksum][linux-arm64-targz-checksum-5.0.1XX-rc1] | [![][linux-arm64-badge-3.1.4XX]][linux-arm64-version-3.1.4XX]<br>[tar.gz][linux-arm64-targz-3.1.4XX] - [Checksum][linux-arm64-targz-checksum-3.1.4XX] | [![][linux-arm64-badge-3.1.3XX]][linux-arm64-version-3.1.3XX]<br>[tar.gz][linux-arm64-targz-3.1.3XX] - [Checksum][linux-arm64-targz-checksum-3.1.3XX] | [![][linux-arm64-badge-3.1.2XX]][linux-arm64-version-3.1.2XX]<br>[tar.gz][linux-arm64-targz-3.1.2XX] - [Checksum][linux-arm64-targz-checksum-3.1.2XX] | [![][linux-arm64-badge-3.1.1XX]][linux-arm64-version-3.1.1XX]<br>[tar.gz][linux-arm64-targz-3.1.1XX] - [Checksum][linux-arm64-targz-checksum-3.1.1XX] | [![][linux-arm64-badge-3.0.1XX]][linux-arm64-version-3.0.1XX]<br>[tar.gz][linux-arm64-targz-3.0.1XX] - [Checksum][linux-arm64-targz-checksum-3.0.1XX] |
| **RHEL 6** | **N/A** | **N/A** | **N/A** | [![][rhel-6-badge-3.1.4XX]][rhel-6-version-3.1.4XX]<br>[tar.gz][rhel-6-targz-3.1.4XX] - [Checksum][rhel-6-targz-checksum-3.1.4XX] | [![][rhel-6-badge-3.1.3XX]][rhel-6-version-3.1.3XX]<br>[tar.gz][rhel-6-targz-3.1.3XX] - [Checksum][rhel-6-targz-checksum-3.1.3XX] | [![][rhel-6-badge-3.1.2XX]][rhel-6-version-3.1.2XX]<br>[tar.gz][rhel-6-targz-3.1.2XX] - [Checksum][rhel-6-targz-checksum-3.1.2XX] | [![][rhel-6-badge-3.1.1XX]][rhel-6-version-3.1.1XX]<br>[tar.gz][rhel-6-targz-3.1.1XX] - [Checksum][rhel-6-targz-checksum-3.1.1XX] | [![][rhel-6-badge-3.0.1XX]][rhel-6-version-3.0.1XX]<br>[tar.gz][rhel-6-targz-3.0.1XX] - [Checksum][rhel-6-targz-checksum-3.0.1XX] |
| **Linux-musl** | [![][linux-musl-badge-master]][linux-musl-version-master]<br>[tar.gz][linux-musl-targz-master] - [Checksum][linux-musl-targz-checksum-master] | [![][linux-musl-badge-5.0.1XX-rc2]][linux-musl-version-5.0.1XX-rc2]<br>[tar.gz][linux-musl-targz-5.0.1XX-rc2] - [Checksum][linux-musl-targz-checksum-5.0.1XX-rc2] | [![][linux-musl-badge-5.0.1XX-rc1]][linux-musl-version-5.0.1XX-rc1]<br>[tar.gz][linux-musl-targz-5.0.1XX-rc1] - [Checksum][linux-musl-targz-checksum-5.0.1XX-rc1] | [![][linux-musl-badge-3.1.4XX]][linux-musl-version-3.1.4XX]<br>[tar.gz][linux-musl-targz-3.1.4XX] - [Checksum][linux-musl-targz-checksum-3.1.4XX] | [![][linux-musl-badge-3.1.3XX]][linux-musl-version-3.1.3XX]<br>[tar.gz][linux-musl-targz-3.1.3XX] - [Checksum][linux-musl-targz-checksum-3.1.3XX] | [![][linux-musl-badge-3.1.2XX]][linux-musl-version-3.1.2XX]<br>[tar.gz][linux-musl-targz-3.1.2XX] - [Checksum][linux-musl-targz-checksum-3.1.2XX] | [![][linux-musl-badge-3.1.1XX]][linux-musl-version-3.1.1XX]<br>[tar.gz][linux-musl-targz-3.1.1XX] - [Checksum][linux-musl-targz-checksum-3.1.1XX] | [![][linux-musl-badge-3.0.1XX]][linux-musl-version-3.0.1XX]<br>[tar.gz][linux-musl-targz-3.0.1XX] - [Checksum][linux-musl-targz-checksum-3.0.1XX] |
| **Windows arm** | [![][win-arm-badge-master]][win-arm-version-master]<br>[zip][win-arm-zip-master] - [Checksum][win-arm-zip-checksum-master] | [![][win-arm-badge-5.0.1XX-rc2]][win-arm-version-5.0.1XX-rc2]<br>[zip][win-arm-zip-5.0.1XX-rc2] - [Checksum][win-arm-zip-checksum-5.0.1XX-rc2] | [![][win-arm-badge-5.0.1XX-rc1]][win-arm-version-5.0.1XX-rc1]<br>[zip][win-arm-zip-5.0.1XX-rc1] - [Checksum][win-arm-zip-checksum-5.0.1XX-rc1] | [![][win-arm-badge-3.1.4XX]][win-arm-version-3.1.4XX]<br>[zip][win-arm-zip-3.1.4XX] - [Checksum][win-arm-zip-checksum-3.1.4XX] | [![][win-arm-badge-3.1.3XX]][win-arm-version-3.1.3XX]<br>[zip][win-arm-zip-3.1.3XX] - [Checksum][win-arm-zip-checksum-3.1.3XX] | [![][win-arm-badge-3.1.2XX]][win-arm-version-3.1.2XX]<br>[zip][win-arm-zip-3.1.2XX] - [Checksum][win-arm-zip-checksum-3.1.2XX] | [![][win-arm-badge-3.1.1XX]][win-arm-version-3.1.1XX]<br>[zip][win-arm-zip-3.1.1XX] - [Checksum][win-arm-zip-checksum-3.1.1XX] | [![][win-arm-badge-3.0.1XX]][win-arm-version-3.0.1XX]<br>[zip][win-arm-zip-3.0.1XX] - [Checksum][win-arm-zip-checksum-3.0.1XX] |
| **Windows arm64** | [![][win-arm64-badge-master]][win-arm64-version-master]<br>[zip][win-arm64-zip-master] | [![][win-arm64-badge-5.0.1XX-rc2]][win-arm64-version-5.0.1XX-rc2]<br>[zip][win-arm64-zip-5.0.1XX-rc2] | [![][win-arm64-badge-5.0.1XX-rc1]][win-arm64-version-5.0.1XX-rc1]<br>[zip][win-arm64-zip-5.0.1XX-rc1] | **N/A** | **N/A** | **N/A** | **N/A** | **N/A** |
| **Constituent Repo Shas** | **N/A** | **N/A** | **N/A** | **N/A** | **N/A** | **N/A** | **N/A** | [Git SHAs][sdk-shas-2.2.1XX] |

Reference notes:
> **1**: Our Debian packages are put together slightly differently than the other OS specific installers. Instead of combining everything, we have separate component packages that depend on each other. If you're installing the SDK from the .deb file (via dpkg or similar), then you'll need to install the corresponding dependencies first:
> * [Host, Host FX Resolver, and Shared Framework](https://github.com/dotnet/runtime#daily-builds)
> * [ASP.NET Core Shared Framework](https://github.com/aspnet/AspNetCore/blob/master/docs/DailyBuilds.md)

.NET Core SDK 2.x downloads can be found here: [.NET Core SDK 2.x Installers and Binaries](Downloads2.x.md)

[win-x64-badge-master]: https://aka.ms/dotnet/net6/dev/Sdk/win_x64_Release_version_badge.svg
[win-x64-version-master]: https://aka.ms/dotnet/net6/dev/Sdk/productCommit-win-x64.txt
[win-x64-installer-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-win-x64.exe
[win-x64-installer-checksum-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-win-x64.exe.sha
[win-x64-zip-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-win-x64.zip
[win-x64-zip-checksum-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-win-x64.zip.sha

[win-x64-badge-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/win_x64_Release_version_badge.svg
[win-x64-version-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/productCommit-win-x64.txt
[win-x64-installer-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-win-x64.exe
[win-x64-installer-checksum-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-win-x64.exe.sha
[win-x64-zip-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-win-x64.zip
[win-x64-zip-checksum-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-win-x64.zip.sha

[win-x64-badge-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/win_x64_Release_version_badge.svg
[win-x64-version-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/productCommit-win-x64.txt
[win-x64-installer-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-win-x64.exe
[win-x64-installer-checksum-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-win-x64.exe.sha
[win-x64-zip-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-win-x64.zip
[win-x64-zip-checksum-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-win-x64.zip.sha

[win-x64-badge-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/win_x64_Release_version_badge.svg
[win-x64-version-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/latest.version
[win-x64-installer-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-win-x64.exe
[win-x64-installer-checksum-3.1.4XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-win-x64.exe.sha
[win-x64-zip-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-win-x64.zip
[win-x64-zip-checksum-3.1.4XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-win-x64.zip.sha

[win-x64-badge-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/win_x64_Release_version_badge.svg
[win-x64-version-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/latest.version
[win-x64-installer-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-win-x64.exe
[win-x64-installer-checksum-3.1.3XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-win-x64.exe.sha
[win-x64-zip-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-win-x64.zip
[win-x64-zip-checksum-3.1.3XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-win-x64.zip.sha

[win-x64-badge-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/win_x64_Release_version_badge.svg
[win-x64-version-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/latest.version
[win-x64-installer-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-win-x64.exe
[win-x64-installer-checksum-3.1.2XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-win-x64.exe.sha
[win-x64-zip-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-win-x64.zip
[win-x64-zip-checksum-3.1.2XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-win-x64.zip.sha

[win-x64-badge-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/win_x64_Release_version_badge.svg
[win-x64-version-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/latest.version
[win-x64-installer-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-win-x64.exe
[win-x64-installer-checksum-3.1.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-win-x64.exe.sha
[win-x64-zip-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-win-x64.zip
[win-x64-zip-checksum-3.1.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-win-x64.zip.sha

[win-x64-badge-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/win_x64_Release_version_badge.svg
[win-x64-version-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/latest.version
[win-x64-installer-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-win-x64.exe
[win-x64-installer-checksum-3.0.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-win-x64.exe.sha
[win-x64-zip-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-win-x64.zip
[win-x64-zip-checksum-3.0.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-win-x64.zip.sha

[win-x86-badge-master]: https://aka.ms/dotnet/net6/dev/Sdk/win_x86_Release_version_badge.svg
[win-x86-version-master]: https://aka.ms/dotnet/net6/dev/Sdk/productCommit-win-x86.txt
[win-x86-installer-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-win-x86.exe
[win-x86-installer-checksum-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-win-x86.exe.sha
[win-x86-zip-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-win-x86.zip
[win-x86-zip-checksum-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-win-x86.zip.sha

[win-x86-badge-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/win_x86_Release_version_badge.svg
[win-x86-version-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/productCommit-win-x86.txt
[win-x86-installer-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-win-x86.exe
[win-x86-installer-checksum-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-win-x86.exe.sha
[win-x86-zip-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-win-x86.zip
[win-x86-zip-checksum-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-win-x86.zip.sha

[win-x86-badge-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/win_x86_Release_version_badge.svg
[win-x86-version-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/productCommit-win-x86.txt
[win-x86-installer-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-win-x86.exe
[win-x86-installer-checksum-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-win-x86.exe.sha
[win-x86-zip-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-win-x86.zip
[win-x86-zip-checksum-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-win-x86.zip.sha

[win-x86-badge-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/win_x86_Release_version_badge.svg
[win-x86-version-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/latest.version
[win-x86-installer-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-win-x86.exe
[win-x86-installer-checksum-3.1.4XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-win-x86.exe.sha
[win-x86-zip-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-win-x86.zip
[win-x86-zip-checksum-3.1.4XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-win-x86.zip.sha

[win-x86-badge-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/win_x86_Release_version_badge.svg
[win-x86-version-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/latest.version
[win-x86-installer-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-win-x86.exe
[win-x86-installer-checksum-3.1.3XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-win-x86.exe.sha
[win-x86-zip-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-win-x86.zip
[win-x86-zip-checksum-3.1.3XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-win-x86.zip.sha

[win-x86-badge-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/win_x86_Release_version_badge.svg
[win-x86-version-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/latest.version
[win-x86-installer-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-win-x86.exe
[win-x86-installer-checksum-3.1.2XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-win-x86.exe.sha
[win-x86-zip-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-win-x86.zip
[win-x86-zip-checksum-3.1.2XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-win-x86.zip.sha

[win-x86-badge-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/win_x86_Release_version_badge.svg
[win-x86-version-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/latest.version
[win-x86-installer-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-win-x86.exe
[win-x86-installer-checksum-3.1.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-win-x86.exe.sha
[win-x86-zip-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-win-x86.zip
[win-x86-zip-checksum-3.1.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-win-x86.zip.sha

[win-x86-badge-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/win_x86_Release_version_badge.svg
[win-x86-version-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/latest.version
[win-x86-installer-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-win-x86.exe
[win-x86-installer-checksum-3.0.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-win-x86.exe.sha
[win-x86-zip-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-win-x86.zip
[win-x86-zip-checksum-3.0.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-win-x86.zip.sha

[osx-badge-master]: https://aka.ms/dotnet/net6/dev/Sdk/osx_x64_Release_version_badge.svg
[osx-version-master]: https://aka.ms/dotnet/net6/dev/Sdk/productCommit-osx-x64.txt
[osx-installer-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-osx-x64.pkg
[osx-installer-checksum-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-osx-x64.pkg.sha
[osx-targz-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-osx-x64.tar.gz
[osx-targz-checksum-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-osx-x64.pkg.tar.gz.sha

[osx-badge-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/osx_x64_Release_version_badge.svg
[osx-version-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/productCommit-osx-x64.txt
[osx-installer-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-osx-x64.pkg
[osx-installer-checksum-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-osx-x64.pkg.sha
[osx-targz-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-osx-x64.tar.gz
[osx-targz-checksum-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-osx-x64.pkg.tar.gz.sha

[osx-badge-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/osx_x64_Release_version_badge.svg
[osx-version-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/productCommit-osx-x64.txt
[osx-installer-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-osx-x64.pkg
[osx-installer-checksum-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-osx-x64.pkg.sha
[osx-targz-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-osx-x64.tar.gz
[osx-targz-checksum-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-osx-x64.pkg.tar.gz.sha

[osx-badge-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/osx_x64_Release_version_badge.svg
[osx-version-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/latest.version
[osx-installer-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-osx-x64.pkg
[osx-installer-checksum-3.1.4XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-osx-x64.pkg.sha
[osx-targz-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-osx-x64.tar.gz
[osx-targz-checksum-3.1.4XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-osx-x64.tar.gz.sha

[osx-badge-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/osx_x64_Release_version_badge.svg
[osx-version-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/latest.version
[osx-installer-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-osx-x64.pkg
[osx-installer-checksum-3.1.3XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-osx-x64.pkg.sha
[osx-targz-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-osx-x64.tar.gz
[osx-targz-checksum-3.1.3XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-osx-x64.tar.gz.sha

[osx-badge-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/osx_x64_Release_version_badge.svg
[osx-version-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/latest.version
[osx-installer-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-osx-x64.pkg
[osx-installer-checksum-3.1.2XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-osx-x64.pkg.sha
[osx-targz-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-osx-x64.tar.gz
[osx-targz-checksum-3.1.2XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-osx-x64.tar.gz.sha

[osx-badge-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/osx_x64_Release_version_badge.svg
[osx-version-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/latest.version
[osx-installer-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-osx-x64.pkg
[osx-installer-checksum-3.1.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-osx-x64.pkg.sha
[osx-targz-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-osx-x64.tar.gz
[osx-targz-checksum-3.1.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-osx-x64.tar.gz.sha

[osx-badge-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/osx_x64_Release_version_badge.svg
[osx-version-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/latest.version
[osx-installer-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-osx-x64.pkg
[osx-installer-checksum-3.0.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-osx-x64.pkg.sha
[osx-targz-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-osx-x64.tar.gz
[osx-targz-checksum-3.0.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-osx-x64.tar.gz.sha

[linux-badge-master]: https://aka.ms/dotnet/net6/dev/Sdk/linux_x64_Release_version_badge.svg
[linux-version-master]: https://aka.ms/dotnet/net6/dev/Sdk/productCommit-linux-x64.txt
[linux-DEB-installer-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-x64.deb
[linux-DEB-installer-checksum-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-x64.deb.sha
[linux-RPM-installer-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-x64.rpm
[linux-RPM-installer-checksum-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-x64.rpm.sha
[linux-targz-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-linux-x64.tar.gz
[linux-targz-checksum-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-linux-x64.tar.gz.sha

[linux-badge-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/linux_x64_Release_version_badge.svg
[linux-version-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/productCommit-linux-x64.txt
[linux-DEB-installer-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-x64.deb
[linux-DEB-installer-checksum-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-x64.deb.sha
[linux-RPM-installer-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-x64.rpm
[linux-RPM-installer-checksum-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-x64.rpm.sha
[linux-targz-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-linux-x64.tar.gz
[linux-targz-checksum-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-linux-x64.tar.gz.sha

[linux-badge-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/linux_x64_Release_version_badge.svg
[linux-version-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/productCommit-linux-x64.txt
[linux-DEB-installer-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-x64.deb
[linux-DEB-installer-checksum-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-x64.deb.sha
[linux-RPM-installer-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-x64.rpm
[linux-RPM-installer-checksum-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-x64.rpm.sha
[linux-targz-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-linux-x64.tar.gz
[linux-targz-checksum-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-linux-x64.tar.gz.sha

[linux-badge-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/linux_x64_Release_version_badge.svg
[linux-version-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/latest.version
[linux-DEB-installer-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-x64.deb
[linux-DEB-installer-checksum-3.1.4XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-x64.deb.sha
[linux-RPM-installer-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-x64.rpm
[linux-RPM-installer-checksum-3.1.4XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-x64.rpm.sha
[linux-targz-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-linux-x64.tar.gz
[linux-targz-checksum-3.1.4XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-linux-x64.tar.gz.sha

[linux-badge-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/linux_x64_Release_version_badge.svg
[linux-version-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/latest.version
[linux-DEB-installer-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-x64.deb
[linux-DEB-installer-checksum-3.1.3XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-x64.deb.sha
[linux-RPM-installer-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-x64.rpm
[linux-RPM-installer-checksum-3.1.3XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-x64.rpm.sha
[linux-targz-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-linux-x64.tar.gz
[linux-targz-checksum-3.1.3XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-linux-x64.tar.gz.sha

[linux-badge-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/linux_x64_Release_version_badge.svg
[linux-version-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/latest.version
[linux-DEB-installer-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-x64.deb
[linux-DEB-installer-checksum-3.1.2XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-x64.deb.sha
[linux-RPM-installer-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-x64.rpm
[linux-RPM-installer-checksum-3.1.2XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-x64.rpm.sha
[linux-targz-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-linux-x64.tar.gz
[linux-targz-checksum-3.1.2XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-linux-x64.tar.gz.sha

[linux-badge-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/linux_x64_Release_version_badge.svg
[linux-version-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/latest.version
[linux-DEB-installer-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-x64.deb
[linux-DEB-installer-checksum-3.1.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-x64.deb.sha
[linux-RPM-installer-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-x64.rpm
[linux-RPM-installer-checksum-3.1.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-x64.rpm.sha
[linux-targz-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-linux-x64.tar.gz
[linux-targz-checksum-3.1.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-linux-x64.tar.gz.sha

[linux-badge-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/linux_x64_Release_version_badge.svg
[linux-version-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/latest.version
[linux-DEB-installer-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-x64.deb
[linux-DEB-installer-checksum-3.0.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-x64.deb.sha
[linux-RPM-installer-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-x64.rpm
[linux-RPM-installer-checksum-3.0.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-x64.rpm.sha
[linux-targz-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-linux-x64.tar.gz
[linux-targz-checksum-3.0.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-linux-x64.tar.gz.sha

[linux-arm-badge-master]: https://aka.ms/dotnet/net6/dev/Sdk/linux_arm_Release_version_badge.svg
[linux-arm-version-master]: https://aka.ms/dotnet/net6/dev/Sdk/productCommit-linux-arm.txt
[linux-arm-targz-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-linux-arm.tar.gz
[linux-arm-targz-checksum-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-linux-arm.tar.gz.sha

[linux-arm-badge-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/linux_arm_Release_version_badge.svg
[linux-arm-version-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/productCommit-linux-arm.txt
[linux-arm-targz-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-linux-arm.tar.gz
[linux-arm-targz-checksum-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-linux-arm.tar.gz.sha

[linux-arm-badge-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/linux_arm_Release_version_badge.svg
[linux-arm-version-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/productCommit-linux-arm.txt
[linux-arm-targz-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-linux-arm.tar.gz
[linux-arm-targz-checksum-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-linux-arm.tar.gz.sha

[linux-arm-badge-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/linux_arm_Release_version_badge.svg
[linux-arm-version-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/latest.version
[linux-arm-targz-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-linux-arm.tar.gz
[linux-arm-targz-checksum-3.1.4XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-linux-arm.tar.gz.sha

[linux-arm-badge-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/linux_arm_Release_version_badge.svg
[linux-arm-version-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/latest.version
[linux-arm-targz-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-linux-arm.tar.gz
[linux-arm-targz-checksum-3.1.3XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-linux-arm.tar.gz.sha

[linux-arm-badge-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/linux_arm_Release_version_badge.svg
[linux-arm-version-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/latest.version
[linux-arm-targz-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-linux-arm.tar.gz
[linux-arm-targz-checksum-3.1.2XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-linux-arm.tar.gz.sha

[linux-arm-badge-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/linux_arm_Release_version_badge.svg
[linux-arm-version-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/latest.version
[linux-arm-targz-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-linux-arm.tar.gz
[linux-arm-targz-checksum-3.1.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-linux-arm.tar.gz.sha

[linux-arm-badge-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/linux_arm_Release_version_badge.svg
[linux-arm-version-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/latest.version
[linux-arm-targz-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-linux-arm.tar.gz
[linux-arm-targz-checksum-3.0.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-linux-arm.tar.gz.sha

[linux-arm64-badge-master]: https://aka.ms/dotnet/net6/dev/Sdk/linux_arm64_Release_version_badge.svg
[linux-arm64-version-master]: https://aka.ms/dotnet/net6/dev/Sdk/productCommit-linux-arm64.txt
[linux-arm64-targz-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-linux-arm64.tar.gz
[linux-arm64-targz-checksum-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-linux-arm64.tar.gz.sha

[linux-arm64-badge-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/linux_arm64_Release_version_badge.svg
[linux-arm64-version-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/productCommit-linux-arm64.txt
[linux-arm64-targz-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-linux-arm64.tar.gz
[linux-arm64-targz-checksum-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-linux-arm64.tar.gz.sha

[linux-arm64-badge-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/linux_arm64_Release_version_badge.svg
[linux-arm64-version-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/productCommit-linux-arm64.txt
[linux-arm64-targz-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-linux-arm64.tar.gz
[linux-arm64-targz-checksum-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-linux-arm64.tar.gz.sha

[linux-arm64-badge-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/linux_arm64_Release_version_badge.svg
[linux-arm64-version-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/latest.version
[linux-arm64-targz-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-linux-arm64.tar.gz
[linux-arm64-targz-checksum-3.1.4XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-linux-arm64.tar.gz.sha

[linux-arm64-badge-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/linux_arm64_Release_version_badge.svg
[linux-arm64-version-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/latest.version
[linux-arm64-targz-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-linux-arm64.tar.gz
[linux-arm64-targz-checksum-3.1.3XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-linux-arm64.tar.gz.sha

[linux-arm64-badge-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/linux_arm64_Release_version_badge.svg
[linux-arm64-version-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/latest.version
[linux-arm64-targz-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-linux-arm64.tar.gz
[linux-arm64-targz-checksum-3.1.2XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-linux-arm64.tar.gz.sha

[linux-arm64-badge-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/linux_arm64_Release_version_badge.svg
[linux-arm64-version-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/latest.version
[linux-arm64-targz-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-linux-arm64.tar.gz
[linux-arm64-targz-checksum-3.1.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-linux-arm64.tar.gz.sha

[linux-arm64-badge-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/linux_arm64_Release_version_badge.svg
[linux-arm64-version-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/latest.version
[linux-arm64-targz-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-linux-arm64.tar.gz
[linux-arm64-targz-checksum-3.0.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-linux-arm64.tar.gz.sha

[rhel-6-badge-master]: https://aka.ms/dotnet/net6/dev/Sdk/rhel.6_x64_Release_version_badge.svg
[rhel-6-version-master]: https://aka.ms/dotnet/net6/dev/Sdk/productCommit-rhel.6-x64.txt
[rhel-6-targz-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-rhel.6-x64.tar.gz
[rhel-6-targz-checksum-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-rhel.6-x64.tar.gz.sha

[rhel-6-badge-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/rhel.6_x64_Release_version_badge.svg
[rhel-6-version-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/productCommit-rhel.6-x64.txt
[rhel-6-targz-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-rhel.6-x64.tar.gz
[rhel-6-targz-checksum-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-rhel.6-x64.tar.gz.sha

[rhel-6-badge-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/rhel.6_x64_Release_version_badge.svg
[rhel-6-version-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/productCommit-rhel.6-x64.txt
[rhel-6-targz-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-rhel.6-x64.tar.gz
[rhel-6-targz-checksum-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-rhel.6-x64.tar.gz.sha

[rhel-6-badge-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/rhel.6_x64_Release_version_badge.svg
[rhel-6-version-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/latest.version
[rhel-6-targz-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-rhel.6-x64.tar.gz
[rhel-6-targz-checksum-3.1.4XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-rhel.6-x64.tar.gz.sha

[rhel-6-badge-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/rhel.6_x64_Release_version_badge.svg
[rhel-6-version-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/latest.version
[rhel-6-targz-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-rhel.6-x64.tar.gz
[rhel-6-targz-checksum-3.1.3XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-rhel.6-x64.tar.gz.sha

[rhel-6-badge-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/rhel.6_x64_Release_version_badge.svg
[rhel-6-version-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/latest.version
[rhel-6-targz-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-rhel.6-x64.tar.gz
[rhel-6-targz-checksum-3.1.2XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-rhel.6-x64.tar.gz.sha

[rhel-6-badge-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/rhel.6_x64_Release_version_badge.svg
[rhel-6-version-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/latest.version
[rhel-6-targz-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-rhel.6-x64.tar.gz
[rhel-6-targz-checksum-3.1.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-rhel.6-x64.tar.gz.sha

[rhel-6-badge-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/rhel.6_x64_Release_version_badge.svg
[rhel-6-version-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/latest.version
[rhel-6-targz-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-rhel.6-x64.tar.gz
[rhel-6-targz-checksum-3.0.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-rhel.6-x64.tar.gz.sha

[linux-musl-badge-master]: https://aka.ms/dotnet/net6/dev/Sdk/linux_musl_x64_Release_version_badge.svg
[linux-musl-version-master]: https://aka.ms/dotnet/net6/dev/Sdk/productCommit-linux-musl-x64.txt
[linux-musl-targz-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-linux-musl-x64.tar.gz
[linux-musl-targz-checksum-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-linux-musl-x64.tar.gz.sha

[linux-musl-badge-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/linux_musl_x64_Release_version_badge.svg
[linux-musl-version-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/productCommit-linux-musl-x64.txt
[linux-musl-targz-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-linux-musl-x64.tar.gz
[linux-musl-targz-checksum-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-linux-musl-x64.tar.gz.sha

[linux-musl-badge-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/linux_musl_x64_Release_version_badge.svg
[linux-musl-version-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/productCommit-linux-musl-x64.txt
[linux-musl-targz-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-linux-musl-x64.tar.gz
[linux-musl-targz-checksum-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-linux-musl-x64.tar.gz.sha

[linux-musl-badge-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/linux_musl_x64_Release_version_badge.svg
[linux-musl-version-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/latest.version
[linux-musl-targz-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-linux-musl-x64.tar.gz
[linux-musl-targz-checksum-3.1.4XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-linux-musl-x64.tar.gz.sha

[linux-musl-badge-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/linux_musl_x64_Release_version_badge.svg
[linux-musl-version-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/latest.version
[linux-musl-targz-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-linux-musl-x64.tar.gz
[linux-musl-targz-checksum-3.1.3XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-linux-musl-x64.tar.gz.sha

[linux-musl-badge-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/linux_musl_x64_Release_version_badge.svg
[linux-musl-version-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/latest.version
[linux-musl-targz-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-linux-musl-x64.tar.gz
[linux-musl-targz-checksum-3.1.2XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-linux-musl-x64.tar.gz.sha

[linux-musl-badge-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/linux_musl_x64_Release_version_badge.svg
[linux-musl-version-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/latest.version
[linux-musl-targz-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-linux-musl-x64.tar.gz
[linux-musl-targz-checksum-3.1.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-linux-musl-x64.tar.gz.sha

[linux-musl-badge-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/linux_musl_x64_Release_version_badge.svg
[linux-musl-version-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/latest.version
[linux-musl-targz-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-linux-musl-x64.tar.gz
[linux-musl-targz-checksum-3.0.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-linux-musl-x64.tar.gz.sha

[win-arm-badge-master]: https://aka.ms/dotnet/net6/dev/Sdk/win_arm_Release_version_badge.svg
[win-arm-version-master]: https://aka.ms/dotnet/net6/dev/Sdk/productCommit-win-arm.txt
[win-arm-zip-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-win-arm.zip
[win-arm-zip-checksum-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-win-arm.zip.sha

[win-arm-badge-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/win_arm_Release_version_badge.svg
[win-arm-version-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/productCommit-win-arm.txt
[win-arm-zip-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-win-arm.zip
[win-arm-zip-checksum-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-win-arm.zip.sha

[win-arm-badge-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/win_arm_Release_version_badge.svg
[win-arm-version-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/productCommit-win-arm.txt
[win-arm-zip-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-win-arm.zip
[win-arm-zip-checksum-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-win-arm.zip.sha

[win-arm-badge-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/win_arm_Release_version_badge.svg
[win-arm-version-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/latest.version
[win-arm-zip-3.1.4XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-win-arm.zip
[win-arm-zip-checksum-3.1.4XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.4xx/dotnet-sdk-latest-win-arm.zip.sha

[win-arm-badge-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/win_arm_Release_version_badge.svg
[win-arm-version-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/latest.version
[win-arm-zip-3.1.3XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-win-arm.zip
[win-arm-zip-checksum-3.1.3XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.3xx/dotnet-sdk-latest-win-arm.zip.sha

[win-arm-badge-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/win_arm_Release_version_badge.svg
[win-arm-version-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/latest.version
[win-arm-zip-3.1.2XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-win-arm.zip
[win-arm-zip-checksum-3.1.2XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.2xx/dotnet-sdk-latest-win-arm.zip.sha

[win-arm-badge-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/win_arm_Release_version_badge.svg
[win-arm-version-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/latest.version
[win-arm-zip-3.1.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-win-arm.zip
[win-arm-zip-checksum-3.1.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.1.1xx/dotnet-sdk-latest-win-arm.zip.sha

[win-arm-badge-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/win_arm_Release_version_badge.svg
[win-arm-version-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/latest.version
[win-arm-zip-3.0.1XX]: https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-win-arm.zip
[win-arm-zip-checksum-3.0.1XX]: https://dotnetclichecksums.blob.core.windows.net/dotnet/Sdk/release/3.0.1xx/dotnet-sdk-latest-win-arm.zip.sha

[win-arm64-badge-master]: https://aka.ms/dotnet/net6/dev/Sdk/win_arm64_Release_version_badge.svg
[win-arm64-version-master]: https://aka.ms/dotnet/net6/dev/Sdk/productCommit-win-arm64.txt
[win-arm64-zip-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-win-arm64.zip
[win-arm64-zip-checksum-master]: https://aka.ms/dotnet/net6/dev/Sdk/dotnet-sdk-win-arm64.zip.sha

[win-arm64-badge-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/win_arm64_Release_version_badge.svg
[win-arm64-version-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/productCommit-win-arm64.txt
[win-arm64-zip-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-win-arm64.zip
[win-arm64-zip-checksum-5.0.1XX-rc2]: https://aka.ms/dotnet/net5/rc2/Sdk/dotnet-sdk-win-arm64.zip.sha

[win-arm64-badge-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/win_arm64_Release_version_badge.svg
[win-arm64-version-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/productCommit-win-arm64.txt
[win-arm64-zip-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-win-arm64.zip
[win-arm64-zip-checksum-5.0.1XX-rc1]: https://aka.ms/dotnet/net5/rc1/Sdk/dotnet-sdk-win-arm64.zip.sha

[sdk-shas-2.2.1XX]: https://github.com/dotnet/versions/tree/master/build-info/dotnet/product/cli/release/2.2#built-repositories

Looking for dotnet-install sources?
-----------------------------------

Sources for dotnet-install.sh and dotnet-install.ps1 are in the [install-scripts repo](https://github.com/dotnet/install-scripts).

Questions & Comments
--------------------

For all feedback, use the Issues on the [.NET CLI](https://github.com/dotnet/cli) repository.

License
-------

By downloading the .zip you are agreeing to the terms in the project [EULA](https://aka.ms/dotnet-core-eula).

