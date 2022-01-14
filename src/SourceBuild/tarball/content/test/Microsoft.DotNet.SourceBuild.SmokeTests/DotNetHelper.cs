using System;
using System.Diagnostics;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.SourceBuild.SmokeTests;

public class DotNetHelper
{
    public string DotNetPath { get; }

    public DotNetHelper(ITestOutputHelper outputHelper)
    {
        DotNetPath = SetupDotNet(outputHelper);
    }

    private string SetupDotNet(ITestOutputHelper outputHelper)
    {
        if (!Directory.Exists(Config.DotNetDirectory))
        {
            if (!File.Exists(Config.DotNetTarballPath))
            {
                throw new InvalidOperationException($"Tarball path '{Config.DotNetTarballPath}' specified in {Config.DotNetTarballPathEnv} does not exist.");
            }

            Directory.CreateDirectory(Config.DotNetDirectory);
            ExecuteHelper.ExecuteProcess("tar", $"xzf {Config.DotNetTarballPath} -C {Config.DotNetDirectory}", outputHelper);
        }

        return Path.Combine(Directory.GetCurrentDirectory(), Config.DotNetDirectory, "dotnet");
    }

    public void ExecuteDotNetCmd(string args, ITestOutputHelper outputHelper)
    {
        (Process Process, string StdOut, string StdErr) executeResult = ExecuteHelper.ExecuteProcess(DotNetPath, args, outputHelper);

        Assert.Equal(0, executeResult.Process.ExitCode);
    }
}
