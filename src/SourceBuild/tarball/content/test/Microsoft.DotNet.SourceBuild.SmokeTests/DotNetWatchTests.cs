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
        string projectDirectory = DotNetHelper.ExecuteNew(DotNetTemplate.Console.GetName(), "WatchTest");

        // We expect an exit code of 143 (128 + 15, i.e. SIGTERM) because we are killing the process manually, so we
        // don't need to validate the exit code.
        DotNetHelper.ExecuteCmd(
            "watch run",
            workingDirectory: projectDirectory,
            customConfigureProcess: customConfigureProcess,
            expectedExitCode: 143);

        void customConfigureProcess(Process process)
        {
            const string waitingString = "Waiting for a file to change before restarting dotnet...";
            const string expectedString = "Hello from dotnet watch!";

            bool fileChanged = false;

            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (e.Data?.Contains(waitingString) ?? false)
                {
                    if (!fileChanged) {
                        fileChanged = true;
                        File.WriteAllText(
                            Path.Combine(projectDirectory, "Program.cs"),
                            File.ReadAllText(Path.Combine(projectDirectory, "Program.cs")).Replace("Hello, World!", expectedString));
                    }
                }

                if (e.Data?.Contains(expectedString) ?? false)
                {
                    OutputHelper.WriteLine("Success!");
                    ExecuteHelper.ExecuteProcessValidateExitCode("kill", $"-s TERM {process.Id}", OutputHelper);
                }
            });
        }
    }
}
