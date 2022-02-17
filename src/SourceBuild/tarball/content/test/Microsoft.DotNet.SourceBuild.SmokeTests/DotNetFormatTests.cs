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
    private string AssetsDirectoryPath { get; }
    private string FormatTestsDirectoryPath { get; }

    public DotNetFormatTests(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
        DotNetHelper = new DotNetHelper(outputHelper);
        AssetsDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "assets");
        FormatTestsDirectoryPath = Path.Combine(AssetsDirectoryPath, nameof(DotNetFormatTests));
    }

    /// <Summary>
    /// Format an unformatted project and verify that the output matches the pre-computed solution.
    /// </Summary>
    [Fact]
    public void FormatProject()
    {
        string testProjectPath = Path.Join(FormatTestsDirectoryPath, nameof(FormatProject));
        string csprojPath = Path.Join(testProjectPath, nameof(FormatProject) + ".csproj");

        string unformattedCsFilePath = Path.Combine(AssetsDirectoryPath, TestFileName);
        string solutionCsFilePath = Path.Combine(AssetsDirectoryPath, SolutionFileName);
        string testCsFilePath = Path.Combine(testProjectPath, TestFileName);

        DotNetHelper.NewProject("console", "C#", testProjectPath, OutputHelper);

        File.Copy(unformattedCsFilePath, testCsFilePath);

        DotNetHelper.ExecuteDotNetCmd($"format {csprojPath}", OutputHelper);

        Assert.True(BaselineHelper.FilesAreEqual(testCsFilePath, solutionCsFilePath));
    }
}
