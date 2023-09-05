// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.SourceBuild.SmokeTests;

public class SourcelinkTests : SmokeTests
{
    private static string SourcelinkRoot { get; } = Path.Combine(Directory.GetCurrentDirectory(), "sourcelink");

    private string sourcelinkToolPath;
    private ConcurrentBag<string> failedFiles = new ConcurrentBag<string>();

    public SourcelinkTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    /// <summary>
    /// Verifies that all symbols have valid sourcelinks.
    /// </summary>
    [SkippableFact(Config.ExcludeSourcelinkEnv, skipOnTrueEnv: true)]
    public void VerifySourcelinks()
    {
        if (Directory.Exists(SourcelinkRoot))
        {
            Directory.Delete(SourcelinkRoot, true);
        }
        Directory.CreateDirectory(SourcelinkRoot);

        sourcelinkToolPath = GetSourcelinkToolPath();

        ValidateAllFiles(ExtractPackages(GetAllSymbolsPackages()));

        foreach (string file in failedFiles)
        {
            OutputHelper.WriteLine($"Failed sourcelink verification: {file}");
        }

        Assert.True(failedFiles.Count == 0);
    }

    /// <summary>
    /// Gets the path to sourcelink binary.
    /// Extracts the dotnet-sourcelink tool package from PSB arhive.
    /// </summary>
    /// <returns>Path to sourcelink tool binary.</returns>
    private string GetSourcelinkToolPath()
    {
        string sourcelinkToolPackageNamePattern = "dotnet-sourcelink*nupkg";
        string sourcelinkToolBinaryFilenamePattern = "dotnet-sourcelink.dll";

        string sourcelinkRoot = Directory.CreateDirectory(Path.Combine(SourcelinkRoot, "sourcelink-tool")).FullName;
        Utilities.ExtractTarball(Config.SourceBuiltArtifactsPath, sourcelinkRoot, sourcelinkToolPackageNamePattern);
        string[] files = Directory.GetFiles(sourcelinkRoot, sourcelinkToolPackageNamePattern, SearchOption.AllDirectories);
        Assert.True(files.Length > 0, "Did not find sourcelink tool package in PSB Artifacts archive");

        string extractedToolPath = Directory.CreateDirectory(Path.Combine(sourcelinkRoot, "extracted")).FullName;
        Utilities.ExtractNupkg(files[0], extractedToolPath);

        files = Directory.GetFiles(extractedToolPath, sourcelinkToolBinaryFilenamePattern, SearchOption.AllDirectories);
        Assert.True(files.Length > 0, $"Did not find sourcelink tool binary with expected filename pattern: {sourcelinkToolBinaryFilenamePattern}");

        return files[0];
    }

    private IEnumerable<string> GetAllSymbolsPackages()
    {
        /*
            At the moment we validate sourcelinks from runtime symbols package.
            The plan is to make symbols, from all repos, available in source-build artifacts.
            Once that's available, this code will be modified to validate all available symbols.
            Tracking issue: https://github.com/dotnet/source-build/issues/3612
        */

        // Runtime symbols package lives in the same directory as PSB artifacts.
        // i.e. <repo-root>/artifacts/x64/Release/runtime/dotnet-runtime-symbols-fedora.36-x64-8.0.0-preview.7.23355.7.tar.gz
        string runtimeSymbolsPackageNamePattern = "dotnet-runtime-symbols-*.tar.gz";
        string[] files = Directory.GetFiles(Path.GetDirectoryName(Config.SourceBuiltArtifactsPath), runtimeSymbolsPackageNamePattern, SearchOption.AllDirectories);
        Assert.True(files.Length > 0, "Did not find runtime symbols archive");

        yield return files[0];
    }

    /// <summary>
    /// Extracts packages to subdirectories of the common symbols root directory.
    /// </summary>
    /// <returns>Path to common symbols root directory.</returns>
    private string ExtractPackages(IEnumerable<string> packages)
    {
        string symbolsRoot = Directory.CreateDirectory(Path.Combine(SourcelinkRoot, "symbols")).FullName;

        foreach (string package in packages)
        {
            Assert.True(package.EndsWith(".tar.gz"), $"Package extension is not supported: {package}");
            DirectoryInfo targetDirInfo = Directory.CreateDirectory(Path.Combine(symbolsRoot, Path.GetFileNameWithoutExtension(package)));
            Utilities.ExtractTarball(package, targetDirInfo.FullName, OutputHelper);
        }

        return symbolsRoot;
    }

    private void ValidateAllFiles(string path)
    {
        Assert.True(Directory.Exists(path), $"Path, with symbol files to validate, does not exist: {path}");

        IEnumerable<string> allFiles = Directory.GetFiles(path, "*.pdb", SearchOption.AllDirectories);
        Parallel.ForEach(allFiles, file =>
        {
            (Process Process, string StdOut, string StdErr) executeResult = ExecuteHelper.ExecuteProcess(
                DotNetHelper.DotNetPath,
                $"{sourcelinkToolPath} test --offline {file}",
                OutputHelper,
                logOutput: false,
                logInfo: false, // Do not log process cmd line, as there can be 1,000+
                millisecondTimeout: 5000,
                configureCallback: (process) => DotNetHelper.ConfigureProcess(process, null));

            if (executeResult.Process.ExitCode != 0)
            {
                failedFiles.Add(file);
            }
        });

        Assert.True(allFiles.Count() > 0, $"Did not find any symbols for sourcelink verification in {path}");
    }
}
