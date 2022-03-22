// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.SourceBuild.SmokeTests;

public class DotNetWatchTests : SmokeTests
{
    public DotNetWatchTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public void WatchTests()
    {
        string projectDirectory = DotNetHelper.ExecuteNew(DotNetTemplate.Console.GetName(), nameof(DotNetWatchTests));
        bool outputChanged = false;

        // We expect an exit code of 143 (128 + 15, i.e. SIGTERM) because we are killing the process manually
        DotNetHelper.ExecuteCmd(
            "watch run",
            workingDirectory: projectDirectory,
            additionalProcessConfigCallback: processConfigCallback,
            expectedExitCode: 143,
            millisecondTimeout: 30000);

        Assert.True(outputChanged);

        void processConfigCallback(Process process)
        {
            const string waitingString = "Waiting for a file to change before restarting dotnet...";
            const string expectedString = "Hello from dotnet watch!";

            bool fileChanged = false;

            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (e.Data?.Contains(waitingString) ?? false)
                {
                    if (!fileChanged) {
                        OutputHelper.WriteLine("Program started, changing file on disk to trigger restart...");
                        File.WriteAllText(
                            Path.Combine(projectDirectory, "Program.cs"),
                            File.ReadAllText(Path.Combine(projectDirectory, "Program.cs")).Replace("Hello, World!", expectedString));
                        fileChanged = true;
                    }
                }
                else if (e.Data?.Contains(expectedString) ?? false)
                {
                    outputChanged = true;
                    OutputHelper.WriteLine("Successfully re-ran program after code change.");
                    ExecuteHelper.ExecuteProcessValidateExitCode("kill", $"-s TERM {process.Id}", OutputHelper);
                }
            });
        }
    }
}
