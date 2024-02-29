// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;

namespace BinaryTool;
public partial class Driver
{
    private const string Utf16Marker = "UTF-16";

    private async Task<IEnumerable<string>> DetectBinariesAsync()
    {
        Log.LogInformation($"Detecting binaries in {TargetDirectory}...");

        var matcher = new Matcher();
        matcher.AddInclude("**/*");
        matcher.AddExcludePatterns(new[] { "**/.dotnet/**", "**/.git/**", "**/git-info/**", "**/artifacts/**", "**/prereqs/packages/**", "**/.packages/**" });

        IEnumerable<string> matchingFiles = matcher.GetResultsInFullPath(TargetDirectory);

        // Parse matching files with diff command to detect binary files
        // Need to check that the file is not UTF-16 encoded because diff can return false positives
        var tasks = matchingFiles
            .Select(async file =>
            {
                string output = await ExecuteProcessAsync("diff", $"/dev/null \"{file}\"");
                return output.StartsWith("Binary") && await IsNotUTF16Async(file) ? file : null;
            });

        var binaryFiles = (await Task.WhenAll(tasks)).OfType<string>().Select(file => file.Substring(TargetDirectory.Length + 1));
        
        Log.LogInformation($"Finished binary detection.");

        return binaryFiles;
    }

    private async Task<bool> IsNotUTF16Async(string file)
    {
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            string output = await ExecuteProcessAsync("file", file);

            if (output.Contains(Utf16Marker))
            {
                return false;
            }
        }
        return true;
    }

    private async Task<string> ExecuteProcessAsync(string executable, string arguments)
    {
        ProcessStartInfo psi = new ()
        {
            FileName = executable,
            Arguments = arguments,
            WorkingDirectory = TargetDirectory,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var proc = Process.Start(psi)!;

        string output = await proc.StandardOutput.ReadToEndAsync();
        string error = await proc.StandardError.ReadToEndAsync();

        await proc.WaitForExitAsync();

        if (!string.IsNullOrEmpty(error))
        {
            Log.LogError(error);
        }

        return output;
    }
}
