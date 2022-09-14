﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using FluentAssertions;
using Microsoft.DotNet.TestFramework;
using Microsoft.DotNet.Tools.Test.Utilities;
using Xunit;

namespace EndToEnd.Tests
{
    public class ProjectBuildTests : TestBase
    {
        private static readonly string currentTfm = "net7.0";

        [Fact]
        public void ItCanNewRestoreBuildRunCleanMSBuildProject()
        {
            var directory = TestAssets.CreateTestDirectory();
            string projectDirectory = directory.FullName;

            string newArgs = "console --debug:ephemeral-hive --no-restore";
            new NewCommandShim()
                .WithWorkingDirectory(projectDirectory)
                .Execute(newArgs)
                .Should().Pass();

            string projectPath = Path.Combine(projectDirectory, directory.Name + ".csproj");
            var project = XDocument.Load(projectPath);
            var ns = project.Root.Name.Namespace;
            project.Root.Element(ns + "PropertyGroup")
                .Element(ns + "TargetFramework").Value = currentTfm;
            project.Save(projectPath);

            new RestoreCommand()
                .WithWorkingDirectory(projectDirectory)
                .Execute()
                .Should().Pass();

            new BuildCommand()
                .WithWorkingDirectory(projectDirectory)
                .Execute()
                .Should().Pass();

            var runCommand = new RunCommand()
                .WithWorkingDirectory(projectDirectory)
                .ExecuteWithCapturedOutput()
                .Should().Pass().And.HaveStdOutContaining("Hello, World!");

            var binDirectory = new DirectoryInfo(projectDirectory).Sub("bin");
            binDirectory.Should().HaveFilesMatching("*.dll", SearchOption.AllDirectories);

            new CleanCommand()
                .WithWorkingDirectory(projectDirectory)
                .Execute()
                .Should().Pass();

            binDirectory.Should().NotHaveFilesMatching("*.dll", SearchOption.AllDirectories);
        }

        [Fact]
        public void ItCanRunAnAppUsingTheWebSdk()
        {
            var directory = TestAssets.CreateTestDirectory();
            string projectDirectory = directory.FullName;

            string newArgs = "console --debug:ephemeral-hive --no-restore";
            new NewCommandShim()
                .WithWorkingDirectory(projectDirectory)
                .Execute(newArgs)
                .Should().Pass();

            string projectPath = Path.Combine(projectDirectory, directory.Name + ".csproj");

            var project = XDocument.Load(projectPath);
            var ns = project.Root.Name.Namespace;

            project.Root.Attribute("Sdk").Value = "Microsoft.NET.Sdk.Web";
            project.Root.Element(ns + "PropertyGroup")
                .Element(ns + "TargetFramework").Value = currentTfm;

            project.Save(projectPath);

            new BuildCommand()
                .WithWorkingDirectory(projectDirectory)
                .Execute()
                .Should().Pass();

            var runCommand = new RunCommand()
                .WithWorkingDirectory(projectDirectory)
                .ExecuteWithCapturedOutput()
                .Should().Pass().And.HaveStdOutContaining("Hello, World!");
        }

        [WindowsOnlyTheory]
        [InlineData("net5.0")]
        [InlineData("current")]
        public void ItCanPublishArm64Winforms(string TargetFramework)
        {
            DirectoryInfo directory = TestAssets.CreateTestDirectory();
            string projectDirectory = directory.FullName;
            string TargetFrameworkParameter = "";

            if (TargetFramework != "current")
            {
                TargetFrameworkParameter = $"-f {TargetFramework}";
            }
            string newArgs = $"winforms {TargetFrameworkParameter} --no-restore";
            new NewCommandShim()
                .WithWorkingDirectory(projectDirectory)
                .Execute(newArgs)
                .Should().Pass();

            string publishArgs="-r win-arm64";
            new PublishCommand()
                .WithWorkingDirectory(projectDirectory)
                .Execute(publishArgs)
                .Should().Pass();

            var selfContainedPublishDir = new DirectoryInfo(projectDirectory)
                .Sub("bin").Sub("Debug").GetDirectories().FirstOrDefault()
                .Sub("win-arm64").Sub("publish");

            selfContainedPublishDir.Should().HaveFilesMatching("System.Windows.Forms.dll", SearchOption.TopDirectoryOnly);
            selfContainedPublishDir.Should().HaveFilesMatching($"{directory.Name}.dll", SearchOption.TopDirectoryOnly);
        }

        [WindowsOnlyTheory]
        [InlineData("net5.0")]
        [InlineData("current")]
        public void ItCanPublishArm64Wpf(string TargetFramework)
        {
            DirectoryInfo directory = TestAssets.CreateTestDirectory();
            string projectDirectory = directory.FullName;
            string TargetFrameworkParameter = "";

            if (TargetFramework != "current")
            {
                TargetFrameworkParameter = $"-f {TargetFramework}";
            }

            string newArgs = $"wpf {TargetFrameworkParameter} --no-restore";
            new NewCommandShim()
                .WithWorkingDirectory(projectDirectory)
                .Execute(newArgs)
                .Should().Pass();

            string publishArgs="-r win-arm64";
            new PublishCommand()
                .WithWorkingDirectory(projectDirectory)
                .Execute(publishArgs)
                .Should().Pass();

            var selfContainedPublishDir = new DirectoryInfo(projectDirectory)
                .Sub("bin").Sub("Debug").GetDirectories().FirstOrDefault()
                .Sub("win-arm64").Sub("publish");

            selfContainedPublishDir.Should().HaveFilesMatching("PresentationCore.dll", SearchOption.TopDirectoryOnly);
            selfContainedPublishDir.Should().HaveFilesMatching("PresentationNative_*.dll", SearchOption.TopDirectoryOnly);
            selfContainedPublishDir.Should().HaveFilesMatching($"{directory.Name}.dll", SearchOption.TopDirectoryOnly);
        }

        [Theory]
        // microsoft.dotnet.common.projectemplates templates
        [InlineData("console")]
        [InlineData("console", "C#")]
        [InlineData("console", "VB")]
        [InlineData("console", "F#")]
        [InlineData("classlib")]
        [InlineData("classlib", "C#")]
        [InlineData("classlib", "VB")]
        [InlineData("classlib", "F#")]

        [InlineData("mstest")]
        [InlineData("nunit")]
        [InlineData("web")]
        [InlineData("mvc")]
        public void ItCanBuildTemplates(string templateName, string language = "")
        {
            TestTemplateCreateAndBuild(templateName, language: language);
        }

        /// <summary>
        /// The test checks if dotnet new shows curated list correctly after the SDK installation and template insertion.
        /// </summary>
        [Fact]
        public void DotnetNewShowsCuratedListCorrectly()
        {
            string locale = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            if (!string.IsNullOrWhiteSpace(locale)
                && !locale.StartsWith("en", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[{nameof(DotnetNewShowsCuratedListCorrectly)}] CurrentUICulture: {locale}");
                Console.WriteLine($"[{nameof(DotnetNewShowsCuratedListCorrectly)}] Test is skipped as it supports only 'en' or invariant culture.");
                return;
            }

            string expectedOutput =
@"[\-\s]+
[\w \.]+webapp,razor\s+\[C#\][\w\ \/]+
[\w \.]+blazorserver\s+\[C#\][\w\ \/]+
[\w \.]+classlib\s+\[C#\],F#,VB[\w\ \/]+
[\w \.]+console\s+\[C#\],F#,VB[\w\ \/]+
";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                expectedOutput +=
@"[\w \.]+winforms\s+\[C#\],VB[\w\ \/]+
[\w \.]+\wpf\s+\[C#\],VB[\w\ \/]+
";
            }
            //list should end with new line
            expectedOutput += Environment.NewLine;

            new NewCommandShim()
             .Execute()
             .Should().Pass()
             .And.HaveStdOutMatching(expectedOutput);
        }

        [Theory]
        // microsoft.dotnet.common.itemtemplates templates
        [InlineData("globaljson")]
        [InlineData("nugetconfig")]
        [InlineData("webconfig")]
        [InlineData("gitignore")]
        [InlineData("tool-manifest")]
        [InlineData("sln")]
        public void ItCanCreateItemTemplate(string templateName)
        {
            DirectoryInfo directory = TestAssets.CreateTestDirectory(identifier: templateName);
            string projectDirectory = directory.FullName;

            string newArgs = $"{templateName} --debug:ephemeral-hive";

            new NewCommandShim()
                .WithWorkingDirectory(projectDirectory)
                .Execute(newArgs)
                .Should().Pass();

            //check if the template created files
            Assert.True(directory.Exists);
            Assert.True(directory.EnumerateFileSystemInfos().Any());
        }

        [WindowsOnlyTheory]
        [InlineData("wpf", Skip = "https://github.com/dotnet/wpf/issues/2363")]
        [InlineData("winforms", Skip = "https://github.com/dotnet/wpf/issues/2363")]
        public void ItCanBuildDesktopTemplates(string templateName)
        {
            TestTemplateCreateAndBuild(templateName);
        }

        [WindowsOnlyTheory]
        [InlineData("wpf", Skip = "https://github.com/dotnet/wpf/issues/2363")]
        public void ItCanBuildDesktopTemplatesSelfContained(string templateName)
        {
            TestTemplateCreateAndBuild(templateName);
        }

        [Theory]
        [InlineData("web")]
        [InlineData("console")]
        public void ItCanBuildTemplatesSelfContained(string templateName)
        {
            TestTemplateCreateAndBuild(templateName, selfContained: true);
        }

        /// <summary>
        /// The test checks if the template creates the template for correct framework by default.
        /// For .NET 6 the templates should create the projects targeting net6.0
        /// </summary>
        [Theory]
        [InlineData("console")]
        [InlineData("console", "C#")]
        [InlineData("console", "VB")]
        [InlineData("console", "F#")]
        [InlineData("classlib")]
        [InlineData("classlib", "C#")]
        [InlineData("classlib", "VB")]
        [InlineData("classlib", "F#")]
        [InlineData("worker")]
        [InlineData("worker", "C#")]
        [InlineData("worker", "F#")]
        [InlineData("mstest")]
        [InlineData("mstest", "C#")]
        [InlineData("mstest", "VB")]
        [InlineData("mstest", "F#")]
        [InlineData("nunit")]
        [InlineData("nunit", "C#")]
        [InlineData("nunit", "VB")]
        [InlineData("nunit", "F#")]
        [InlineData("xunit")]
        [InlineData("xunit", "C#")]
        [InlineData("xunit", "VB")]
        [InlineData("xunit", "F#")]
        [InlineData("blazorserver")]
        [InlineData("blazorwasm")]
        [InlineData("web")]
        [InlineData("web", "C#")]
        [InlineData("web", "F#")]
        [InlineData("mvc")]
        [InlineData("mvc", "C#")]
        [InlineData("mvc", "F#")]
        [InlineData("webapi")]
        [InlineData("webapi", "C#")]
        [InlineData("webapi", "F#")]
        [InlineData("webapp")]
        [InlineData("razorclasslib")]
        public void ItCanCreateAndBuildTemplatesWithDefaultFramework(string templateName, string language = "")
        {
            string framework = DetectExpectedDefaultFramework(templateName);
            TestTemplateCreateAndBuild(templateName, selfContained: true, language: language, framework: framework);
        }

        /// <summary>
        /// The test checks if the template creates the template for correct framework by default.
        /// For .NET 6 the templates should create the projects targeting net6.0.
        /// These templates require node.js to be built, so we just check if TargetFramework is present in csproj files
        /// </summary>
        [Theory]
        [InlineData("angular")]
        [InlineData("react")]
        public void ItCanCreateTemplateWithDefaultFramework(string templateName)
        {
            string framework = DetectExpectedDefaultFramework();
            TestTemplateCreateAndBuild(templateName, build: false, framework: framework);
        }

        /// <summary>
        /// [Windows only tests]
        /// The test checks if the template creates the template for correct framework by default.
        /// For .NET 6 the templates should create the projects targeting net6.0.
        /// </summary>
        [WindowsOnlyTheory]
        [InlineData("wpf")]
        [InlineData("wpf", "C#")]
        [InlineData("wpf", "VB")]
        [InlineData("wpflib")]
        [InlineData("wpflib", "C#")]
        [InlineData("wpflib", "VB")]
        [InlineData("wpfcustomcontrollib")]
        [InlineData("wpfcustomcontrollib", "C#")]
        [InlineData("wpfcustomcontrollib", "VB")]
        [InlineData("wpfusercontrollib")]
        [InlineData("wpfusercontrollib", "C#")]
        [InlineData("wpfusercontrollib", "VB")]
        [InlineData("winforms")]
        [InlineData("winforms", "C#")]
        [InlineData("winforms", "VB")]
        [InlineData("winformslib")]
        [InlineData("winformslib", "C#")]
        [InlineData("winformslib", "VB")]
        [InlineData("winformscontrollib")]
        [InlineData("winformscontrollib", "C#")]
        [InlineData("winformscontrollib", "VB")]
        public void ItCanCreateAndBuildTemplatesWithDefaultFramework_Windows(string templateName, string language = "")
        {
            string framework = DetectExpectedDefaultFramework(templateName);
            TestTemplateCreateAndBuild(templateName, selfContained: true, language: language, framework: $"{framework}-windows");
        }

        /// <summary>
        /// [project is not built on linux-musl]
        /// The test checks if the template creates the template for correct framework by default.
        /// For .NET 6 the templates should create the projects targeting net6.0.
        /// </summary>
        [Theory]
        [InlineData("grpc")]
        public void ItCanCreateAndBuildTemplatesWithDefaultFramework_DisableBuildOnLinuxMusl(string templateName)
        {
            string framework = DetectExpectedDefaultFramework();

            if (RuntimeInformation.RuntimeIdentifier.StartsWith("alpine")) //linux musl
            {
                TestTemplateCreateAndBuild(templateName, build: false, framework: framework);
            }
            else
            {
                TestTemplateCreateAndBuild(templateName, selfContained: true, framework: framework);
            }
        }

        private static string DetectExpectedDefaultFramework(string template = "")
        {
            string dotnetFolder = Path.GetDirectoryName(RepoDirectoriesProvider.DotnetUnderTest);
            string[] runtimeFolders = Directory.GetDirectories(Path.Combine(dotnetFolder, "shared", "Microsoft.NETCore.App"));

            int latestMajorVersion = runtimeFolders.Select(folder => int.Parse(Path.GetFileName(folder).Split('.').First())).Max();
            if (latestMajorVersion == 7)
            {
                return $"net{latestMajorVersion}.0";
            }

            throw new Exception("Unsupported version of SDK");
        }

        private static void TestTemplateCreateAndBuild(string templateName, bool build = true, bool selfContained = false, string language = "", string framework = "")
        {
            DirectoryInfo directory = TestAssets.CreateTestDirectory(identifier: string.IsNullOrWhiteSpace(language) ? templateName : $"{templateName}[{language}]");
            string projectDirectory = directory.FullName;

            string newArgs = $"{templateName} --debug:ephemeral-hive --no-restore";
            if (!string.IsNullOrWhiteSpace(language))
            {
                newArgs += $" --language {language}";
            }

            new NewCommandShim()
                .WithWorkingDirectory(projectDirectory)
                .Execute(newArgs)
                .Should().Pass();

            if (!string.IsNullOrWhiteSpace(framework))
            {
                //check if MSBuild TargetFramework property for *proj is set to expected framework
                string expectedExtension = language switch
                {
                    "C#" => "*.csproj",
                    "F#" => "*.fsproj",
                    "VB" => "*.vbproj",
                    _ => "*.csproj"
                };
                string projectFile = Directory.GetFiles(projectDirectory, expectedExtension).Single();
                XDocument projectXml = XDocument.Load(projectFile);
                XNamespace ns = projectXml.Root.Name.Namespace;
                Assert.Equal(framework, projectXml.Root.Element(ns + "PropertyGroup").Element(ns + "TargetFramework").Value);
            }

            if (build)
            {
                string buildArgs = selfContained ? "" : $"-r {RuntimeInformation.RuntimeIdentifier}";
                if (!string.IsNullOrWhiteSpace(framework))
                {
                    buildArgs += $" --framework {framework}";
                }
                
                // Remove this (or formalize it) after https://github.com/dotnet/installer/issues/12479 is resolved.
                if (language == "F#")
                {
                    buildArgs += $" /p:_NETCoreSdkIsPreview=true";
                }
            
                string dotnetRoot = Path.GetDirectoryName(RepoDirectoriesProvider.DotnetUnderTest);
                new BuildCommand()
                     .WithEnvironmentVariable("PATH", dotnetRoot) // override PATH since razor rely on PATH to find dotnet
                     .WithWorkingDirectory(projectDirectory)
                     .Execute(buildArgs)
                     .Should().Pass();
            }
        }
    }
}
