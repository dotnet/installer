// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.DotNet.Tools.Test.Utilities;
using Xunit;

namespace EndToEnd.Tests
{

    public class WorkloadTests : TestBase
    {
        public static bool IsRunningOnWindowsX86 => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.OSArchitecture == Architecture.X86;
        private static string _tmpDirForDotNet = Path.Combine(Path.GetTempPath(), "WorkloadTests");

        [Fact]
        public void ItCannotPublishBlazorWasm_AOTWithoutWorkloadInstalled()
        {
            if (IsRunningOnWindowsX86)
            {
                // unsupported
                return;
            }

            DirectoryInfo directory = TestAssets.CreateTestDirectory();
            WorkloadTestEnvironment env = PrepareTestEnvironment(directory.FullName);

            //FIXME: poke the value in the project file

            // AOT builds are slow, so in case the test is failing (IOW, it is able to build)
            // then we want to stop it ASAP
            //
            // invalid AOTMode would cause the build to fail, but it shouldn't get there at all
            // because that logic is in wasm targets, which come from the workload packs
            string publishArgs = "publish /p:RunAOTCompilation=true /p:AOTMode=nonexistant /v:detailed";

            new DotnetCommand(env.dotnetUnderTest)
                 .WithEnvironmentVariable("PATH", env.pathValue)
                 .WithWorkingDirectory(Path.GetDirectoryName(env.projectFile))
                 .ExecuteWithCapturedOutput(publishArgs)
                 .Should().Fail()
                 .And.HaveStdOutMatching("error NETSDK1147:.*install microsoft-net-sdk-blazorwebassembly-aot");
        }

        [Fact]
        public void ItCanPublishBlazorWasmWithWorkloads_NoAOT() => TestBlazorWasmWithWithWorkload();

        [Fact]
        public void ItCanPublishBlazorWasmWithWorkloads_WithAOT() => TestBlazorWasmWithWithWorkload("/p:RunAOTCompilation=true");

        [Theory]
        [InlineData("")]
        [InlineData("/p:RunAOTCompilation=true")]
        public void ItCanPublishBlazorWasmWithWorkloads_Invariant(string args) => TestBlazorWasmWithWithWorkload($"/p:InvariantGlobalization=true {args}");

        private static void TestBlazorWasmWithWithWorkload(string extraBuildArgs="", [CallerMemberName] string callerName = "")
        {
            if (IsRunningOnWindowsX86)
            {
                // unsupported
                return;
            }

            DirectoryInfo testDirectory = TestAssets.CreateTestDirectory(callingMethod: callerName);
            WorkloadTestEnvironment env = PrepareTestEnvironment(testDirectory.FullName);

            new DotnetWorkloadCommand(env.dotnetUnderTest)
                .WithEnvironmentVariable("PATH", env.pathValue)
                .ExecuteWithCapturedOutput("install microsoft-net-sdk-blazorwebassembly-aot --skip-manifest-update")
                .Should().Pass();

            string publishArgs = $"publish /v:detailed {extraBuildArgs}";
            new DotnetCommand(env.dotnetUnderTest)
                 .WithEnvironmentVariable("PATH", env.pathValue)
                 .WithWorkingDirectory(Path.GetDirectoryName(env.projectFile))
                 .Execute(publishArgs)
                 .Should().Pass();
        }

        private static WorkloadTestEnvironment PrepareTestEnvironment(string testProjectName = "", [CallerMemberName] string callingMethod = "")
        {
            string origDotnetPath = Path.GetDirectoryName(RepoDirectoriesProvider.DotnetUnderTest);

            string packsDir = Path.Combine(origDotnetPath, "packs", "Microsoft.NET.Runtime.WebAssembly.Sdk");
            Directory.Exists(packsDir)
                .Should().BeFalse($"{packsDir} should not exist. Shared 'dotnet' installation in artifacts, should not have Microsoft.NET.Runtime.WebAssembly.Sdk pack installed");

            string baseDirectory = Path.Combine(_tmpDirForDotNet, callingMethod);
            if (Directory.Exists(baseDirectory))
                Directory.Delete(baseDirectory, recursive: true);

            string dotnetDirectory = Path.Combine(baseDirectory, "dotnet");
            DirectoryCopy(origDotnetPath, dotnetDirectory);

            string dotnetUnderTest = Path.Combine(dotnetDirectory, "dotnet");
            dotnetUnderTest += RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "";

            string projectDirectory = Path.Combine(baseDirectory, "project");
            string projectFile = TestTemplateCreate("blazorwasm", projectDirectory);

            // Use a controlled, clean PATH. This can help catch issues, for example, `emcc` should use
            // `python` from a workload pack, but it if isn't able to find it for some reason (bug!), then
            // it would fallback to using system python from `PATH`.
            //
            // Restricting PATH would catch that case.
            string pathValue = dotnetDirectory;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // On linux, we don't have a workload pack for Python. And it is expected
                //  that emcc will use system python.
                //  - But since we are using very restricted PATHs, we need to add python
                //    to that.
                var pythonDir = Environment.GetEnvironmentVariable("PATH")
                                    ?.Split(':', StringSplitOptions.RemoveEmptyEntries)
                                    .FirstOrDefault(dir => File.Exists(Path.Combine(dir, "python")));
                if (pythonDir != null)
                    pathValue += $":{pythonDir}";
            }

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // WasmApp.targets needs /bin/sh
                pathValue += ":/bin";
            }

            return new WorkloadTestEnvironment(dotnetUnderTest, projectFile, pathValue);
        }

        // code from https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true)
        {
            DirectoryInfo dir = new(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it.
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            foreach (FileInfo file in dir.EnumerateFiles())
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dir.GetDirectories())
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }

    internal record WorkloadTestEnvironment(string dotnetUnderTest, string projectFile, string pathValue);
}
