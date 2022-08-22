// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.SourceBuild.SmokeTests;

public class DotNetWatchTests : SmokeTests
{
    public DotNetWatchTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    public async void WatchTests()
    {
        bool result = await DotNetWatchHasExpectedOutput();
        Assert.True(result);
    }

    private Task<bool> DotNetWatchHasExpectedOutput() {
        TaskCompletionSource<bool> tcs = new();

        string projectDirectory = DotNetHelper.ExecuteNew(DotNetTemplate.Console.GetName(), nameof(DotNetWatchTests));

        DotNetHelper.ExecuteCmd(
            "watch run",
            workingDirectory: projectDirectory,
            additionalProcessConfigCallback: processConfigCallback,
            expectedExitCode: null, // The exit code does not reflect whether or not dotnet watch is working properly
            millisecondTimeout: 30000);

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
                    OutputHelper.WriteLine("Successfully re-ran program after code change.");
                    process.Kill(true);
                    tcs.TrySetResult(true);
                }
            });
        }

        return tcs.Task;
    }
}
