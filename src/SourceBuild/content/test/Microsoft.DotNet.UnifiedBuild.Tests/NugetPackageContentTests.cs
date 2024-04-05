// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NuGet.Common;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.UnifiedBuild.BaselineTests;


[Trait("Category", "SdkContent")]
public class NugetPackageContentTests : TestBase, IClassFixture<NugetPackageContentTests.Config>
{
    public class Config
    {
        public string[] ExcludedFileExtensions = [".psmdcp", ".p7s"];
        public string PackageBaseUrl = "https://pkgs.dev.azure.com/dnceng/9ee6d478-d288-47f7-aacc-f6e6d082ae6d/_packaging/a54510f9-4b2c-4e69-b96a-6096683aaa1f/nuget/v3/flat2";
        public string NugetIndexUrl = "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet9/nuget/v3/index.json";
        public string[] NugetIndices = [
            "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet9/nuget/v3/index.json",
            "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json",
            "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-tools/nuget/v3/index.json",
            "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json",
            "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-libraries/nuget/v3/index.json",
            "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-libraries-transport/nuget/v3/index.json",
            "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet9-transport/nuget/v3/index.json",
        ];
    }

    Config _config;

    public NugetPackageContentTests(ITestOutputHelper outputHelper, Config config) : base(outputHelper)
    {
        _config = config;
    }

    public static IEnumerable<object[]> GetPackagePaths()
    {
        var packages = (string)(AppContext.GetData("Microsoft.DotNet.UnifiedBuild.Tests.Packages") ?? throw new InvalidOperationException("RuntimeConfig value 'Microsoft.DotNet.UnifiedBuild.Tests.Packages' must be set"));
        var packagesArray = packages.Split(";")
            // Nuget is unable to find fsharp or command-line-api packages for some reason
            .Where(p => !(Path.GetFileName(Path.GetDirectoryName(p)) is "fsharp" or "command-line-api"))
            .Select<string, object[]>(p => [p]);
        return packagesArray;
    }

    /// <Summary>
    /// Verifies the file layout of the source built sdk tarball to the Microsoft build.
    /// The differences are captured in baselines/MsftToUbSdkDiff.txt.
    /// Version numbers that appear in paths are compared but are stripped from the baseline.
    /// This makes the baseline durable between releases.  This does mean however, entries
    /// in the baseline may appear identical if the diff is version specific.
    /// </Summary>
    [Theory]
    [MemberData(nameof(GetPackagePaths))]
    public async Task CompareFileContents(string nugetPackagePath)
    {
        var fileName = Path.GetFileName(nugetPackagePath);
        var repoName = Path.GetFileName(Path.GetDirectoryName(nugetPackagePath))!;
        using PackageArchiveReader testPackageReader= new PackageArchiveReader(File.OpenRead(nugetPackagePath));
        NuspecReader testNuspecReader = await testPackageReader.GetNuspecReaderAsync(CancellationToken.None);
        var packageName = testNuspecReader.GetId();
        var testPackageVersion = testNuspecReader.GetVersion().ToFullString();

        NuGetVersion packageVersion = new NuGetVersion(testPackageVersion);
        var packageStream = await TryDownloadPackage(packageName, packageVersion);
        if (packageStream is null)
        {
            OutputHelper.LogWarningMessage($"Could not find package '{packageName}' with version '{packageVersion}'");
            return;
        }
        OutputHelper.WriteLine($"Found package '{packageName}' with version '{packageVersion}'");

        using PackageArchiveReader packageReader = new PackageArchiveReader(packageStream);
        NuspecReader nuspecReader = await packageReader.GetNuspecReaderAsync(CancellationToken.None);
        ImmutableHashSet<string> baselineFiles = packageReader.GetFiles().Where(f => !_config.ExcludedFileExtensions.Contains(Path.GetExtension(f))).ToImmutableHashSet();
        ImmutableHashSet<string> testFiles = testPackageReader.GetFiles().Where(f => !_config.ExcludedFileExtensions.Contains(Path.GetExtension(f))).ToImmutableHashSet();
        foreach(var baseline in baselineFiles)
        {
            if (!testFiles.Contains(baseline))
            {
                OutputHelper.LogWarningMessage($"Unified build package '{packageName}' is missing file '{baseline}'");
            }
        }
        foreach(var testFile in testFiles)
        {
            if (!baselineFiles.Contains(testFile))
            {
                OutputHelper.LogWarningMessage($"Unified build package '{packageName}' has additional file '{testFile}'");
            }
        }
    }

    public async Task<MemoryStream?> TryDownloadPackage(string packageId, NuGetVersion packageVersion)
    {
        bool found = false;
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;
        SourceCacheContext cache = new SourceCacheContext();
        MemoryStream? packageStream = null;
        foreach (var nugetRepository in _config.NugetIndices)
        {
            SourceRepository repository = Repository.Factory.GetCoreV3(nugetRepository);
            FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();
            packageStream = new MemoryStream();

            found = await resource.CopyNupkgToStreamAsync(
                packageId,
                packageVersion,
                packageStream,
                cache,
                logger,
                cancellationToken);

            if (found)
            {
                OutputHelper.WriteLine($"Found '{packageId}' in '{nugetRepository}'");
                break;
            }
            packageStream.Dispose();
        }
        if (!found)
            packageStream = null;

        return packageStream;
    }
}

