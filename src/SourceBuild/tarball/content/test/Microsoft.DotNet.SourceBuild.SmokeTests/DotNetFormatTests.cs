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
    private const string TestFileName = "Test.cs";

    private ITestOutputHelper OutputHelper { get; }
    private DotNetHelper DotNetHelper { get; }
    private string _formatTestRootDirectory { get; }

    public DotNetFormatTests(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
        DotNetHelper = new DotNetHelper(outputHelper);
        _formatTestRootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "format");
    }

    /// <Summary>
    /// Create a new project and verify that it doesn't need to be formatted.
    /// </Summary>
    [Fact]
    public void VerifyNoChanges()
    {
        string testProjectPath = Path.Join(this._formatTestRootDirectory, nameof(VerifyNoChanges));

        CreateNewProject(testProjectPath);

        string csprojPath = Path.Combine(testProjectPath, nameof(VerifyNoChanges) + ".csproj");

        DotNetHelper.ExecuteDotNetCmd($"format {csprojPath} --verify-no-changes", OutputHelper);
    }

    /// <Summary>
    /// Format an unformatted project and verify that the output matches the
    /// pre-computed solution.
    /// </Summary>
    [Fact]
    public void FormatProject()
    {
        string testProjectPath = Path.Join(this._formatTestRootDirectory, nameof(FormatProject));
        string csprojPath = Path.Join(testProjectPath, nameof(FormatProject) + ".csproj");

        string unformattedCsFilePath = Path.Combine(this._formatTestRootDirectory, "unformatted", TestFileName);
        string solutionCsFilePath = Path.Combine(this._formatTestRootDirectory, "solution", TestFileName);
        string testCsFilePath = Path.Combine(testProjectPath, TestFileName);

        CreateNewProject(testProjectPath);

        File.Copy(unformattedCsFilePath, testCsFilePath);

        DotNetHelper.ExecuteDotNetCmd($"format {csprojPath}", OutputHelper);

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

        DotNetHelper.ExecuteDotNetCmd($"new console --output {projectDirectory}", OutputHelper);
    }
}
