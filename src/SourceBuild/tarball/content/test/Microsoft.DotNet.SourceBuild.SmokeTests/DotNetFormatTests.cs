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

    public DotNetFormatTests(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
        DotNetHelper = new DotNetHelper(outputHelper);
    }

    /// <Summary>
    /// Format an unformatted project and verify that the output matches the pre-computed solution.
    /// </Summary>
    [Fact]
    public void FormatProject()
    {
        string assetsDirectory = BaselineHelper.GetAssetsDirectory();

        string unformattedCsFilePath = Path.Combine(assetsDirectory, TestFileName);
        string solutionCsFilePath = Path.Combine(assetsDirectory, SolutionFileName);

        string projectDirectory = DotNetHelper.ExecuteNew("console", "C#", nameof(FormatProject), OutputHelper);

        string projectFilePath = Path.Combine(projectDirectory, nameof(FormatProject) + ".csproj");
        string testCsFilePath = Path.Combine(projectDirectory, TestFileName);

        File.Copy(unformattedCsFilePath, testCsFilePath);

        DotNetHelper.ExecuteCmd($"format {projectFilePath}", OutputHelper);

        BaselineHelper.CompareFiles(solutionCsFilePath, testCsFilePath, OutputHelper);
    }
}
