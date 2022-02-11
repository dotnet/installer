// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.SourceBuild.SmokeTests;

public class DotNetFormatTests
{
    private const string TestFileName = "FormatTestUnformatted.cs";
    private const string SolutionFileName = "FormatTestSolution.cs";

    private ITestOutputHelper OutputHelper { get; }
    private DotNetHelper DotNetHelper { get; }
    private string _assetsDirectoryPath { get; }
    private string _formatTestsDirectoryPath { get; }

    public DotNetFormatTests(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
        DotNetHelper = new DotNetHelper(outputHelper);
        _assetsDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "assets");
        _formatTestsDirectoryPath = Path.Combine(_assetsDirectoryPath, nameof(DotNetFormatTests));
    }

    /// <Summary>
    /// Create a new project and verify that it doesn't need to be formatted.
    /// </Summary>
    [Fact]
    public void VerifyNoChanges()
    {
        string testProjectPath = Path.Join(_formatTestsDirectoryPath, nameof(VerifyNoChanges));

        DotNetHelper.NewProject("console", "C#", testProjectPath, OutputHelper);

        string csprojPath = Path.Combine(testProjectPath, nameof(VerifyNoChanges) + ".csproj");

        DotNetHelper.ExecuteDotNetCmd($"format {csprojPath} --verify-no-changes", OutputHelper);
    }

    /// <Summary>
    /// Format an unformatted project and verify that the output matches the pre-computed solution.
    /// </Summary>
    [Fact]
    public void FormatProject()
    {
        string testProjectPath = Path.Join(_formatTestsDirectoryPath, nameof(FormatProject));
        string csprojPath = Path.Join(testProjectPath, nameof(FormatProject) + ".csproj");

        string unformattedCsFilePath = Path.Combine(_assetsDirectoryPath, TestFileName);
        string solutionCsFilePath = Path.Combine(_assetsDirectoryPath, SolutionFileName);
        string testCsFilePath = Path.Combine(testProjectPath, TestFileName);

        DotNetHelper.NewProject("console", "C#", testProjectPath, OutputHelper);

        File.Copy(unformattedCsFilePath, testCsFilePath);

        DotNetHelper.ExecuteDotNetCmd($"format {csprojPath}", OutputHelper);

        Assert.True(BaselineHelper.FilesAreEqual(testCsFilePath, solutionCsFilePath));
    }
}
