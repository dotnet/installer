// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using System;
using System.Linq;

namespace Microsoft.DotNet.SourceBuild.SmokeTests;

public class DotNetFormatTests
{
    const string VerifyNoChangesTestDirectoryName = "VerifyNoChanges";
    const string FormatTestDirectoryName = "FormatTest";
    const string UnformattedDirectoryName = "unformatted";
    const string SolutionDirectoryName = "solution";
    const string TestFileName = "Test.cs";

    private ITestOutputHelper _outputHelper { get; }
    private DotNetHelper _dotNetHelper { get; }
    private string _formatTestRootDirectory { get; }

    public DotNetFormatTests(ITestOutputHelper outputHelper)
    {
        this._outputHelper = outputHelper;
        this._dotNetHelper = new DotNetHelper(outputHelper);
        this._formatTestRootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "format");
    }

    /// <Summary>
    /// Create a new project and verify that it doesn't need to be formatted.
    /// </Summary>
    [Fact]
    public void DotNetFormatVerifyNoChanges()
    {
        string verifyNoChangesTestPath = Path.Join(this._formatTestRootDirectory, VerifyNoChangesTestDirectoryName);

        CreateNewProject(verifyNoChangesTestPath);

        string verifyNoChangesCsprojPath = Path.Combine(verifyNoChangesTestPath, VerifyNoChangesTestDirectoryName + ".csproj");

        string[] dotnetFormatArgs = {
            "format",
            verifyNoChangesCsprojPath,
            "--verify-no-changes"
        };

        _dotNetHelper.ExecuteDotNetCmd(String.Join(' ', dotnetFormatArgs), _outputHelper);
    }

    /// <Summary>
    /// Format an unformatted project and verify that the output matches the
    /// pre-computed solution.
    /// </Summary>
    [Fact]
    public void DotNetFormatProject()
    {
        string formatTestPath = Path.Join(this._formatTestRootDirectory, FormatTestDirectoryName);
        string formatTestCsprojPath = Path.Join(formatTestPath, FormatTestDirectoryName + ".csproj");

        string unformattedCsFilePath = Path.Combine(this._formatTestRootDirectory, UnformattedDirectoryName, TestFileName);
        string solutionCsFilePath = Path.Combine(this._formatTestRootDirectory, SolutionDirectoryName, TestFileName);
        string testCsFilePath = Path.Combine(formatTestPath, TestFileName);

        CreateNewProject(formatTestPath);

        File.Copy(unformattedCsFilePath, testCsFilePath);

        string[] dotnetFormatArgs = {
            "format",
            formatTestCsprojPath,
        };

        _dotNetHelper.ExecuteDotNetCmd(String.Join(' ', dotnetFormatArgs), _outputHelper);

        Assert.True(File.ReadAllLines(testCsFilePath).SequenceEqual(File.ReadAllLines(solutionCsFilePath)));
    }

    /// <Summary>
    /// Run `dotnet new console` in projectDirectory. If projectDirectory
    /// already exists, then empty it out first.
    /// </Summary>
    private void CreateNewProject(string projectDirectory)
    {
        if (Directory.Exists(projectDirectory))
        {
            Directory.Delete(projectDirectory, true);
        }

        Directory.CreateDirectory(projectDirectory);

        string[] dotnetNewArgs = {
            "new",
            "console",
            "--output",
            projectDirectory
        };

        _dotNetHelper.ExecuteDotNetCmd(String.Join(' ', dotnetNewArgs), _outputHelper);
    }
}
