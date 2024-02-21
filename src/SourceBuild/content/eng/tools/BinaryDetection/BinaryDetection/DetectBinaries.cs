// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace BinaryDetection
{
    public static class DetectBinaries
    {
        private static readonly string Utf16Marker = "UTF-16";

        public static bool Execute() => ExecuteAsync().GetAwaiter().GetResult();

        public static async Task<bool> ExecuteAsync()
        {
            Config.Instance.Log.LogInformation($"Detecting binaries in {Config.Instance.RootDir}...");

            var matcher = new Matcher();
            matcher.AddInclude("**/*");
            matcher.AddExcludePatterns(new[] { "**/.dotnet/**", "**/.git/**", "**/git-info/**", "**/artifacts/**", "**/prereqs/packages/**" });

            IEnumerable<string> matchingFiles = matcher.GetResultsInFullPath(Config.Instance.RootDir);

            var tasks = matchingFiles
                .Select(async file =>
                {
                    string output = await ProcessManager.Execute("diff", $"/dev/null \"{file}\"", Config.Instance.RootDir);
                    return output.StartsWith("Binary") && await IsNotUTF16(file) ? file : null;
                });

            var binaryFiles = (await Task.WhenAll(tasks)).Where(file => file != null).Select(file => file!).Select(file => file.Substring(Config.Instance.RootDir.Length + 1));

            File.WriteAllLines(Config.Instance.DetectedBinariesFile, binaryFiles);
            Config.Instance.Log.LogInformation($"Finished binary detection. Wrote all detected binaries to {Config.Instance.DetectedBinariesFile}.");

            return true;
        }

        private static async Task<bool> IsNotUTF16(string file)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                string output = await ProcessManager.Execute("file", file, Config.Instance.RootDir);

                if (output.Contains(Utf16Marker))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
