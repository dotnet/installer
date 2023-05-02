// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.SourceBuild.SmokeTests;

/// <summary>
/// The purpose of these tests are to ensure the prereqs project (which is used by the prep script to download the smoke test prereq
/// packages) is up-to-date with the actual packages that that smoke tests are dependent on.
/// 
/// These tests rely on all the smoke tests having already been executed in a previous test pass so that the packages output directory
/// is populated with all the packages that are required by the smoke tests. In an ideal world, the prereqs project could be validated
/// by just restoring it, passing the necessary runtime/aspnet versions as MSBuild properties. But that doesn't work during the
/// development cycle of a release because you're typically dealing with build incoherency where there isn't consistency amongst the
/// packages with respect to the versions being used. For that reason, we need to statically read the packages listed in the prereqs
/// project and compare them to the packages contained in the packages output directory.
/// </summary>
/// <remarks>
/// This class intentionally does not derive from the SmokeTests class.
/// Because these tests need to be executed in separate test pass after all the other smoke tests have been executed, the test
/// configuration makes use of Trait attributes so that the "--filter" option of "dotnet test" can be used to control which
/// tests get executed for each test pass. This class defines a special "Prereqs" category whereas all the other smoke tests
/// have a category of "Default" which is implemented by the base SmokeTests class. If this class were to derive from SmokeTests,
/// it would cause these tests to be executed along with all the other smoke tests, which is not what we want.
/// </remarks>
[Trait("Category", "Prereqs")]
public partial class PrereqsTests : IDisposable
{
    private static readonly Regex _dynamicVersionRegex = DynamicVersionRegex();
    private static readonly Regex _dotNetSdkMajorMinorVersionRegex = DotNetSdkMajorMinorVersionRegex();

    private DotNetHelper _dotNetHelper;
    private ITestOutputHelper _outputHelper;
    private string _tmpDir;

    public PrereqsTests(ITestOutputHelper outputHelper)
    {
        _dotNetHelper = new DotNetHelper(outputHelper);
        _outputHelper = outputHelper;
        _tmpDir = Path.Combine(Directory.GetCurrentDirectory(), "prereqs-artifacts");
        Directory.CreateDirectory(_tmpDir);
    }

    public void Dispose()
    {
        Directory.Delete(_tmpDir, recursive: true);
    }

    private static string GetSharedRuntimeVersion(string sharedRuntimeName)
    {
        string? version = null;
        string path = $"./shared/{sharedRuntimeName}/";
        SmokeTests.EnumerateTarball(Config.SdkTarballPath,
            tarEntry =>
            {
                if (tarEntry.Name.StartsWith(path) && tarEntry.Name.Length > path.Length)
                {
                    version = Path.GetFileName(tarEntry.Name.TrimEnd('/'));
                    return false;
                }

                return true;
            });

        Assert.NotNull(version);

        return version!;
    }

    /// <summary>
    /// Compares the restored packages from the smoke tests with the expected packages.
    /// </summary>
    [Fact]
    public void ComparePackages()
    {
        string aspnetVersion = GetSharedRuntimeVersion("Microsoft.AspNetCore.App");
        string runtimeVersion = GetSharedRuntimeVersion("Microsoft.NETCore.App");
        string fsharpVersion = Config.FSharpPackageVersion;

        string prereqsProjPath = Path.Combine(Directory.GetCurrentDirectory(), "assets", "prereqs.csproj");
        XDocument doc = XDocument.Load(prereqsProjPath);

        // Get all of the packages defined in prereqs project. There may be packages that have multiple versions
        // listed.
        IEnumerable<(string PackageName, IEnumerable<string> Versions)> expectedPackages = doc.Root!.Elements()
            .Where(element => element.Name == "Target" && element.Attribute("Name")!.Value == "DownloadPrereqs")
            .Descendants()
            .Where(element => element.Name.LocalName == "PackageDownload")
            .Select(element =>
                new
                {
                    PackageName = element.Attribute("Include")!.Value
                        .Replace("$(Arch)", RuntimeInformation.OSArchitecture.ToString())
                        .ToLowerInvariant(),
                    Version = element.Attribute("Version")!.Value
                        .ToLowerInvariant()
                        .TrimStart('[')
                        .TrimEnd(']')
                })
            .GroupBy(value => value.PackageName, value => value.Version)
            .Select(group => (group.Key, group.OrderBy(ver => ver).AsEnumerable()))
            .OrderBy(pkg => pkg.Key);

        _outputHelper.WriteLine($"Expected packages from '{prereqsProjPath}':");
        OutputPackageInfo(expectedPackages);

        // Get all the packages that were output by smoke tests. There may be packages that have multiple versions.
        IEnumerable<(string PackageName, IEnumerable<string> Versions)> actualPackages =
            new DirectoryInfo(DotNetHelper.PackagesDirectory).GetDirectories()
                .Select(dir =>
                    (
                        PackageName: dir.Name.ToLowerInvariant(),
                        Versions: dir.GetDirectories()
                            .Select(verDir => verDir.Name.ToLowerInvariant())
                            .OrderBy(ver => ver)
                            .AsEnumerable()
                    ))
                .OrderBy(pkg => pkg.PackageName);

        Assert.NotEmpty(actualPackages);

        _outputHelper.WriteLine($"Actual packages from '{DotNetHelper.PackagesDirectory}':");
        OutputPackageInfo(actualPackages);

        IEnumerable<string> expectedPackageNames = expectedPackages.Select(val => val.PackageName);
        IEnumerable<string> actualPackageNames = actualPackages.Select(val => val.PackageName);

        IEnumerable<string> packageNamesNotInOutputDir = expectedPackageNames.Except(actualPackageNames);
        _outputHelper.WriteLine($"Checking if there are any expected package names that did not show up in the output. If this fails, these packages should be cleaned up from the prereqs project at '{prereqsProjPath}'.");
        Assert.Empty(packageNamesNotInOutputDir);

        IEnumerable<string> packagesInOutputDirNotInExpected = actualPackageNames.Except(expectedPackageNames);
        _outputHelper.WriteLine($"Checking if there are any package names in the output dir that are not in the prereqs project. If this fails, those packages should be added to the prereqs project at '{prereqsProjPath}'.");
        Assert.Empty(packagesInOutputDirNotInExpected);

        // Get the major/minor version of .NET to help determine which packages that are outputted have dynamic versions
        Match sdkVersionMatch = _dotNetSdkMajorMinorVersionRegex.Match(Path.GetFileName(Config.SdkTarballPath)!);
        Assert.True(sdkVersionMatch.Success);

        Version sdkVersion = Version.Parse(sdkVersionMatch.Groups[1].Value);

        // Up to this point, we've only verified that package names are consistent between the prereqs project
        // and the smoke test package output. Now we need to compare package versions.

        foreach ((string PackageName, IEnumerable<string> Versions) expectedPackage in expectedPackages)
        {
            (string PackageName, IEnumerable<string> Versions) actualPackage =
                actualPackages.First(pkg => pkg.PackageName == expectedPackage.PackageName);

            IEnumerable<string> expectedVersionDuplicates = expectedPackage.Versions
                .GroupBy(ver => ver)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);

            _outputHelper.WriteLine($"Verifying the versions for package '{expectedPackage.PackageName}' do not have duplicates. If this fails, the prereqs project at '{prereqsProjPath}' needs to be updated to remove duplicate versions for this package.");
            Assert.Empty(expectedVersionDuplicates);

            // During the development cycle, there can be incoherency leading to package versions which aren't consistent across packages.
            // For that reason, we can only compare package versions whose values are statically defined in the prereqs project. Package
            // versions which use a variable are considered to be dynamic and ignored.

            IEnumerable<string> expectedDynamicVersions = expectedPackage.Versions
                .Where(ver => _dynamicVersionRegex.IsMatch(ver))
                .ToList();

            Assert.True(expectedDynamicVersions.Count() <= 1,
                $"Package '{expectedPackage.PackageName}' should not have more than 1 dynamic version in the prereqs project at '{prereqsProjPath}'. Dynamic versions defined: {string.Join(',', expectedDynamicVersions)}");

            // Due to the potential for build incoherency, there can be multiple dynamic package versions in the output for a given package
            // (the incoherency can lead to dependencies on different versions of the same package). But in the expected prereqs project,
            // that package would only be listed once. So it's not possible to do a comparison of the number of versions between the expected
            // and actual results when dynamic versions are present.
            if (!expectedDynamicVersions.Any())
            {
                _outputHelper.WriteLine($"Verifying the actual number of versions associated with package '{expectedPackage.PackageName}' are the same as the expected. If this fails, the prereqs project at '{prereqsProjPath}' needs to be updated to reflect the actual version numbers being outputted by the smoke tests.");
                Assert.Equal(expectedPackage.Versions.Count(), actualPackage.Versions.Count());
            }

            IEnumerable<string> expectedStaticVersions = expectedPackage.Versions
                .Except(expectedDynamicVersions)
                .OrderBy(ver => ver)
                .ToList();

            // Use the following heuristic for determining whether an outputted package's version represents a dynamic version:
            //  - Package name begins with "Microsoft" or "System"
            //      * Major/Minor version matches the version of the SDK
            //      * Contains a preview version label
            //  OR
            //  - Package name beings with "FSharp"
            //      * Contains a preview version label
            IEnumerable<string> actualDynamicVersions = Enumerable.Empty<string>();
            if (actualPackage.PackageName.StartsWith("microsoft.") || actualPackage.PackageName.StartsWith("system."))
            {
                actualDynamicVersions = actualPackage.Versions
                    .Where(ver =>
                        (ver.StartsWith($"{sdkVersion}.") && IsSdkPreviewVersion(ver, sdkVersion)) ||
                        ver == runtimeVersion ||
                        ver == aspnetVersion);
            }
            else if (actualPackage.PackageName.StartsWith("fsharp."))
            {
                actualDynamicVersions = actualPackage.Versions
                    .Where(ver => IsSdkPreviewVersion(ver, sdkVersion) || ver == fsharpVersion);
            }

            IEnumerable<string> actualStaticVersions = actualPackage.Versions
                .Except(actualDynamicVersions)
                .OrderBy(ver => ver)
                .ToList();

            _outputHelper.WriteLine($"Verifying the expected statically-defined versions for package '{expectedPackage.PackageName}' exist in the outputted packages of the smoke tests. If this fails, the prereqs project at '{prereqsProjPath}' needs to be updated to reflect the actual version numbers being outputted.");
            Assert.Equal(expectedStaticVersions, actualStaticVersions);
        }
    }

    private static bool IsSdkPreviewVersion(string version, Version sdkVersion) =>
        (version.Contains("-preview.") || version.Contains("-rc.") || version.Contains("-beta."));

    private void OutputPackageInfo(IEnumerable<(string PackageName, IEnumerable<string> Versions)> packages)
    {
        foreach ((string packageName, IEnumerable<string> versions) in packages)
        {
            _outputHelper.WriteLine(packageName);
            foreach (string version in versions)
            {
                _outputHelper.WriteLine($"- {version}");
            }
        }

        _outputHelper.WriteLine(string.Empty);
    }

    [GeneratedRegex(@"\$\(\S*Version\)", RegexOptions.IgnoreCase)]
    private static partial Regex DynamicVersionRegex();

    [GeneratedRegex(@"dotnet-sdk-(\d+\.\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex DotNetSdkMajorMinorVersionRegex();
}
