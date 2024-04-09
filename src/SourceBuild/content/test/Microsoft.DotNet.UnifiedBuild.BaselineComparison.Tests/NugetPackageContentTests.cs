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
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.UnifiedBuild.BaselineComparison.Tests;


[Trait("Category", "SdkContent")]
public class NugetPackageContentTests : TestBase
{
    public class ExtractedPackage : IDisposable
    {
        public string _extractedFolder;

        public ExtractedPackage(ZipArchive archive)
        {
            _extractedFolder = Path.GetTempFileName();
            Directory.CreateDirectory(_extractedFolder);
            archive.ExtractToDirectory(_extractedFolder);
        }

        public IEnumerable<string> GetFilePaths()
        {
            return Directory.EnumerateFiles(_extractedFolder, "*", SearchOption.AllDirectories);
        }

        public void Dispose()
        {
            Directory.Delete(_extractedFolder, true);
        }
    }
    public static readonly ImmutableArray<string> ExcludedFileExtensions = [".psmdcp", ".p7s"];
    public static readonly ImmutableArray<string> NugetIndices = [
        "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet9/nuget/v3/index.json",
        "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json",
        "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-tools/nuget/v3/index.json",
        "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json",
        "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-libraries/nuget/v3/index.json",
        "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-libraries-transport/nuget/v3/index.json",
        "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet9-transport/nuget/v3/index.json",
    ];

    public NugetPackageContentTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    public static IEnumerable<object[]> GetPackagePaths()
    {
        var packagesSwitch = Config.RuntimeConfigSwitchPrefix + "Packages";
        var packages = Config.GetRuntimeConfig(packagesSwitch) ?? throw new InvalidOperationException($"RuntimeConfig value '{packagesSwitch}' must be set");
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
        var ct = CancellationToken.None;
        using PackageArchiveReader testPackageReader = new PackageArchiveReader(File.OpenRead(nugetPackagePath));
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
        IEnumerable<string> baselineFiles = (await packageReader.GetFilesAsync(ct)).Where(f => !ExcludedFileExtensions.Contains(Path.GetExtension(f)));
        IEnumerable<string> testFiles = (await testPackageReader.GetFilesAsync(ct)).Where(f => !ExcludedFileExtensions.Contains(Path.GetExtension(f)));

        var testPackageContentsFileName = Path.Combine(LogsDirectory, packageName + "_ub_files.txt");
        await File.WriteAllLinesAsync(testPackageContentsFileName, testFiles);
        var baselinePackageContentsFileName = Path.Combine(LogsDirectory, packageName + "_msft_files.txt");
        await File.WriteAllLinesAsync(testPackageContentsFileName, baselineFiles);

        string diff = BaselineHelper.DiffFiles(baselinePackageContentsFileName, testPackageContentsFileName, OutputHelper);
        diff = SdkContentTests.RemoveDiffMarkers(diff);
        BaselineHelper.CompareBaselineContents($"MsftToUb-{packageName}-Files.diff", diff, Config.LogsDirectory, OutputHelper, Config.WarnOnSdkContentDiffs);
    }

    [Theory]
    [MemberData(nameof(GetPackagePaths))]
    public async Task CompareAssemblyVersions(string nugetPackagePath)
    {
        using PackageArchiveReader testPackageReader = new PackageArchiveReader(File.OpenRead(nugetPackagePath));
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

        using PackageArchiveReader baselinePackageReader = new PackageArchiveReader(packageStream);
        IEnumerable<string> baselineFiles = (await baselinePackageReader.GetFilesAsync(CancellationToken.None)).Where(f => !ExcludedFileExtensions.Contains(Path.GetExtension(f)));
        IEnumerable<string> testFiles = (await testPackageReader.GetFilesAsync(CancellationToken.None)).Where(f => !ExcludedFileExtensions.Contains(Path.GetExtension(f)));
        Dictionary<string, Version?> baselineAssemblyVersions = new();
        Dictionary<string, Version?> testAssemblyVersions = new();
        foreach (var fileName in baselineFiles.Intersect(testFiles))
        {
            string baselineFileName = Path.GetTempFileName();
            string testFileName = Path.GetTempFileName();
            using (FileStream baselineFile = File.OpenWrite(baselineFileName))
            using (FileStream testFile = File.OpenWrite(testFileName))
            {
                await baselinePackageReader.GetEntry(fileName).Open().CopyToAsync(baselineFile);
                await testPackageReader.GetEntry(fileName).Open().CopyToAsync(testFile);
            }
            try
            {
                var baselineAssemblyVersion = AssemblyName.GetAssemblyName(testFileName);
                baselineAssemblyVersions.Add(fileName, baselineAssemblyVersion.Version);
            }
            catch (BadImageFormatException)
            {
                Assert.Throws<BadImageFormatException>(() => AssemblyName.GetAssemblyName(baselineFileName));
                break;
            }
            var testAssemblyVersion = AssemblyName.GetAssemblyName(baselineFileName);
            testAssemblyVersions.Add(fileName, testAssemblyVersion.Version);

            File.Delete(baselineFileName);
            File.Delete(testFileName);
        }

        string UbVersionsFileName = packageName + "_ub_assemblyversions.txt";
        AssemblyVersionHelpers.WriteAssemblyVersionsToFile(testAssemblyVersions, UbVersionsFileName);

        string MsftVersionsFileName = packageName + "_msft_assemblyversions.txt";
        AssemblyVersionHelpers.WriteAssemblyVersionsToFile(baselineAssemblyVersions, MsftVersionsFileName);

        string diff = BaselineHelper.DiffFiles(MsftVersionsFileName, UbVersionsFileName, OutputHelper);
        diff = SdkContentTests.RemoveDiffMarkers(diff);
        BaselineHelper.CompareBaselineContents($"MsftToUb_{packageName}.diff", diff, Config.LogsDirectory, OutputHelper, Config.WarnOnSdkContentDiffs);
    }

    public async Task<MemoryStream?> TryDownloadPackage(string packageId, NuGetVersion packageVersion)
    {
        bool found = false;
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;
        SourceCacheContext cache = new SourceCacheContext();
        MemoryStream? packageStream = null;
        foreach (var nugetRepository in NugetIndices)
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

