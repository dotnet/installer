// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.SourceBuild.SmokeTests;

/// <summary>
/// Run the test suite at https://github.com/redhat-developer/dotnet-regular-tests/
/// </summary>
public class RedHatTests : SmokeTests
{
    private string TestRunnerDirectory { get; }
    private string TestRunnerPath { get; }

    public RedHatTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        TestRunnerDirectory = Path.Combine(Directory.GetCurrentDirectory(), "turkey");
        TestRunnerPath = Path.Combine(TestRunnerDirectory, "Turkey.dll");
    }

    [Fact]
    public void VerifyScenario()
    {
        DownloadTestRunner();
        CloneTestRepository();
        RunTests();
    }

    private async Task DownloadTestRunner()
    {
        Uri testRunnerUrl = new("https://github.com/redhat-developer/dotnet-bunny/releases/latest/download/turkey.tar.gz");
        string testRunnerFile = "turkey.tar.gz";
        using HttpClient client = new();
        await client.DownloadFileAsync(testRunnerUrl, testRunnerFile);
        ExecuteHelper.ExecuteProcessValidateExitCode("tar", $"xf {testRunnerFile}", OutputHelper);
    }

    private void CloneTestRepository()
    {
        if (Directory.Exists("dotnet-regular-tests"))
        {
            Directory.Delete("dotnet-regular-tests", recursive: true);
        }

        Uri testRepoUrl = new("https://github.com/redhat-developer/dotnet-regular-tests");
        ExecuteHelper.ExecuteProcessValidateExitCode("git", $"clone {testRepoUrl}", OutputHelper);

        // Remove tests that are known to fail
        string[] testsToDelete = new[]
        {
            // Tests that check for things not in the SDK release tarball
            "bash-completion",
            "man-pages",

            // Tests that need distribution packages to work
            "distribution-packages",
            "release-version-sane",

            // Tests that check distro-modifications
            "file-permissions",
            "liblttng-ust_sys-sdt.h",
            "system-libunwind",
            "use-current-runtime",  // Fixed in 6.0.2xx: https://github.com/dotnet/sdk/pull/22373

            // Tests that need to be debugged
            "cgroup-limit",  // Needs access to systemd/dbus environment variables?
            "workload",   // this test hardcodes macos workload, which no longer seems to be available in 6.0.104?
        };

        foreach (var test in testsToDelete)
        {
            Directory.Delete($"dotnet-regular-tests/{test}", recursive: true);
        }
    }

    private void RunTests()
    {
        ExecuteHelper.ExecuteProcessValidateExitCode("find", null, OutputHelper);

        DotNetHelper.ExecuteCmd($"{TestRunnerPath} --version", setPath: true);

        DotNetHelper.ExecuteCmd($"{TestRunnerPath} dotnet-regular-tests", setPath: true);
    }

}
