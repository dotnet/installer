﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.SourceBuild.SmokeTests
{
    internal class BaselineHelper
    {
        public static void CompareEntries(string baselineFileName, IOrderedEnumerable<string> actualEntries)
        {
            IEnumerable<string> baseline = File.ReadAllLines(GetBaselineFilePath(baselineFileName));
            string[] missingEntries = actualEntries.Except(baseline).ToArray();
            string[] extraEntries = baseline.Except(actualEntries).ToArray();

            string? message = null;
            if (missingEntries.Length > 0)
            {
                message = $"Missing entries in '{baselineFileName}' baseline: {Environment.NewLine}{string.Join(Environment.NewLine, missingEntries)}{Environment.NewLine}{Environment.NewLine}";
            }

            if (extraEntries.Length > 0)
            {
                message += $"Extra entries in '{baselineFileName}' baseline: {Environment.NewLine}{string.Join(Environment.NewLine, extraEntries)}{Environment.NewLine}{Environment.NewLine}";
            }

            Assert.Null(message);
        }

        public static void CompareContents(string baselineFileName, string actualContents, ITestOutputHelper outputHelper)
        {
            string actualFilePath = Path.Combine(Environment.CurrentDirectory, $"{baselineFileName}");
            File.WriteAllText(actualFilePath, actualContents);

            CompareFiles(baselineFileName, actualFilePath, outputHelper);
        }

        public static void CompareFiles(string baselineFileName, string? actualFilePath, ITestOutputHelper outputHelper)
        {
            if (!File.Exists(actualFilePath))
            {
                throw new InvalidOperationException($"Baseline comparison path '{actualFilePath}' does not exist.");
            }

            string baselineFilePath = GetBaselineFilePath(baselineFileName);
            string baselineFileText = File.ReadAllText(baselineFilePath);
            string actualFileText = File.ReadAllText(actualFilePath);

            string? message = null;

            if (baselineFileText != actualFileText)
            {
                // Retrieve a diff in order to provide a UX which calls out the diffs.
                string diff = DiffFiles(baselineFilePath, actualFilePath, outputHelper);
                message = $"{Environment.NewLine}Baseline '{baselineFilePath}' does not match actual '{actualFilePath}`.  {Environment.NewLine}"
                    + $"{diff}{Environment.NewLine}";
            }

            Assert.Null(message);
        }

        public static string DiffFiles(string file1Path, string file2Path, ITestOutputHelper outputHelper)
        {
            (Process Process, string StdOut, string StdErr) diffResult =
                ExecuteHelper.ExecuteProcess("git", $"diff --no-index {file1Path} {file2Path}", outputHelper);
            Assert.Equal(1, diffResult.Process.ExitCode);

            return diffResult.StdOut;
        }

        public static string GetAssetsDirectory() => Path.Combine(Directory.GetCurrentDirectory(), "assets");

        private static string GetBaselineFilePath(string baselineFileName) => Path.Combine(GetAssetsDirectory(), "baselines", baselineFileName);

        public static string RemoveRids(string diff) => diff.Replace(Config.TargetRid, "banana.rid");

        public static string RemoveVersions(string source)
        {
            // Remove semantic versions
            // Regex source: https://semver.org/#is-there-a-suggested-regular-expression-regex-to-check-a-semver-string
            Regex semanticVersionRegex = new(
                $"(0|[1-9]\\d*)\\.(0|[1-9]\\d*)\\.(0|[1-9]\\d*)"
                + $"(?:-((?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\\.(?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*))*))"
                + $"?(?:\\+([0-9a-zA-Z-]+(?:\\.[0-9a-zA-Z-]+)*))?");
            string result = semanticVersionRegex.Replace(source, $"x.y.z");

            // Remove net.x.y path segments
            string pathSeparator = Regex.Escape(Path.DirectorySeparatorChar.ToString());
            Regex netTfmRegex = new($"{pathSeparator}net[1-9]*.[0-9]{pathSeparator}");
            return netTfmRegex.Replace(result, $"{Path.DirectorySeparatorChar}netx.y{Path.DirectorySeparatorChar}");
        }
    }
}
