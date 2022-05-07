// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using Xunit.Abstractions;

namespace Microsoft.DotNet.SourceBuild.SmokeTests;

internal class DotNetHelper
{
    private static readonly object s_lockObj = new();

    public static string DotNetPath { get; } = Path.Combine(Config.DotNetDirectory, "dotnet");
    public static string LogsDirectory { get; } = Path.Combine(Directory.GetCurrentDirectory(), "logs");
    public static string PackagesDirectory { get; } = Path.Combine(Directory.GetCurrentDirectory(), "packages");
    public static string ProjectsDirectory { get; } = Path.Combine(Directory.GetCurrentDirectory(), $"projects-{DateTime.Now:yyyyMMddHHmmssffff}");

    private ITestOutputHelper OutputHelper { get; }

    public DotNetHelper(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;

        lock (s_lockObj)
        {
            if (!Directory.Exists(Config.DotNetDirectory))
            {
                if (!File.Exists(Config.SdkTarballPath))
                {
                    throw new InvalidOperationException($"Tarball path '{Config.SdkTarballPath}' specified in {Config.SdkTarballPath} does not exist.");
                }

                Directory.CreateDirectory(Config.DotNetDirectory);
                ExecuteHelper.ExecuteProcessValidateExitCode("tar", $"xzf {Config.SdkTarballPath} -C {Config.DotNetDirectory}", outputHelper);
            }

            if (!Directory.Exists(ProjectsDirectory))
            {
                Directory.CreateDirectory(ProjectsDirectory);
                InitNugetConfig();
            }

            if (!Directory.Exists(PackagesDirectory))
            {
                Directory.CreateDirectory(PackagesDirectory);
            }

            if (!Directory.Exists(LogsDirectory))
            {
                Directory.CreateDirectory(LogsDirectory);
            }
        }
    }

    private static void InitNugetConfig()
    {
        bool useLocalPackages = !string.IsNullOrEmpty(Config.PrereqsPath);
        string nugetConfigPrefix = useLocalPackages ? "local" : "online";
        string nugetConfigPath = Path.Combine(ProjectsDirectory, "NuGet.Config");
        File.Copy(
            Path.Combine(BaselineHelper.GetAssetsDirectory(), $"{nugetConfigPrefix}.NuGet.Config"),
            nugetConfigPath);

        if (useLocalPackages)
        {
            if (!Directory.Exists(Config.PrereqsPath))
            {
                throw new InvalidOperationException(
                    $"Prereqs path '{Config.PrereqsPath}' specified in {Config.PrereqsPathEnv} does not exist.");
            }

            string nugetConfig = File.ReadAllText(nugetConfigPath);
            nugetConfig = nugetConfig.Replace("SMOKE_TEST_PACKAGE_FEED", Config.PrereqsPath);
            File.WriteAllText(nugetConfigPath, nugetConfig);
        }
    }

    public void ExecuteCmd(string args, string? workingDirectory = null, Action<Process>? additionalProcessConfigCallback = null, int expectedExitCode = 0, int millisecondTimeout = -1)
    {
        (Process Process, string StdOut, string StdErr) executeResult = ExecuteHelper.ExecuteProcess(
            DotNetPath,
            args,
            OutputHelper,
            configure: (process) => configureProcess(process, workingDirectory),
            millisecondTimeout: millisecondTimeout);
        
        ExecuteHelper.ValidateExitCode(executeResult, expectedExitCode);

        void configureProcess(Process process, string? workingDirectory)
        {
            ConfigureProcess(process, workingDirectory);

            additionalProcessConfigCallback?.Invoke(process);
        }
    }

    public static void ConfigureProcess(Process process, string? workingDirectory, bool setPath = false)
    {
        if (workingDirectory != null)
        {
            process.StartInfo.WorkingDirectory = workingDirectory;
        }

        process.StartInfo.EnvironmentVariables["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        process.StartInfo.EnvironmentVariables["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        process.StartInfo.EnvironmentVariables["DOTNET_ROOT"] = Config.DotNetDirectory;
        process.StartInfo.EnvironmentVariables["NUGET_PACKAGES"] = PackagesDirectory;

        if (setPath)
        {
            process.StartInfo.EnvironmentVariables["PATH"] = $"{Config.DotNetDirectory}:{Environment.GetEnvironmentVariable("PATH")}";
        }
    }

    public void ExecuteBuild(string projectName) =>
        ExecuteCmd($"build {GetBinLogOption(projectName, "build")}", GetProjectDirectory(projectName));

    /// <summary>
    /// Create a new .NET project and return the path to the created project folder.
    /// </summary>
    public string ExecuteNew(string projectType, string name, string? language = null, string? customArgs = null)
    {
        string projectDirectory = GetProjectDirectory(name);
        string options = $"--name {name} --output {projectDirectory}";
        if (language != null)
        {
            options += $" --language \"{language}\"";
        }
        if (string.IsNullOrEmpty(customArgs))
        {
            options += $" {customArgs}";
        }

        ExecuteCmd($"new {projectType} {options}");

        return projectDirectory;
    }

    public void ExecutePublish(string projectName, bool? selfContained = null, string? rid = null, bool trimmed = false, bool readyToRun = false)
    {
        string options = string.Empty;
        string binlogDifferentiator = string.Empty;

        if (selfContained.HasValue)
        {
            options += $"--self-contained {selfContained.Value.ToString().ToLowerInvariant()}";
            if (selfContained.Value)
            {
                binlogDifferentiator += "self-contained";
                if (!string.IsNullOrEmpty(rid))
                {
                    options += $" -r {rid}";
                    binlogDifferentiator += $"-{rid}";
                }
                if (trimmed)
                {
                    options += " /p:PublishTrimmed=true";
                    binlogDifferentiator += "-trimmed";
                }
                if (readyToRun)
                {
                    options += " /p:PublishReadyToRun=true";
                    binlogDifferentiator += "-R2R";
                }
            }
        }

        ExecuteCmd(
            $"publish {options} {GetBinLogOption(projectName, "publish", binlogDifferentiator)}",
            GetProjectDirectory(projectName));
    }

    public void ExecuteRun(string projectName) =>
        ExecuteCmd($"run {GetBinLogOption(projectName, "run")}", GetProjectDirectory(projectName));

    public void ExecuteRunWeb(string projectName)
    {
        ExecuteCmd(
            $"run {GetBinLogOption(projectName, "run")}",
            GetProjectDirectory(projectName),
            additionalProcessConfigCallback: processConfigCallback,
            millisecondTimeout: 30000);

        void processConfigCallback(Process process)
        {
            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (e.Data?.Contains("Application started. Press Ctrl+C to shut down.") ?? false)
                {
                    ExecuteHelper.ExecuteProcessValidateExitCode("kill", $"-s TERM {process.Id}", OutputHelper);
                }
            });
        }
    }

    public void ExecuteTest(string projectName) =>
        ExecuteCmd($"test {GetBinLogOption(projectName, "test")}", GetProjectDirectory(projectName));

    private static string GetBinLogOption(string projectName, string command, string? differentiator = null)
    {
        string fileName = $"{projectName}-{command}";
        if (!string.IsNullOrEmpty(differentiator))
        {
            fileName += $"-{differentiator}";
        }

        return $"/bl:{Path.Combine(LogsDirectory, $"{fileName}.binlog")}";
    }

    private static string GetProjectDirectory(string projectName) => Path.Combine(ProjectsDirectory, projectName);
}
