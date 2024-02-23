// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileSystemGlobbing;

namespace BinaryTool;
public static class DetectBinaries
{
    private static readonly string Utf16Marker = "UTF-16";

    public static bool Execute() => ExecuteAsync().GetAwaiter().GetResult();

    public static async Task<bool> ExecuteAsync()
    {
        Driver.Log.LogInformation($"Detecting binaries in {Driver.TargetDirectory}...");

        var matcher = new Matcher();
        matcher.AddInclude("**/*");
        matcher.AddExcludePatterns(new[] { "**/.dotnet/**", "**/.git/**", "**/git-info/**", "**/artifacts/**", "**/prereqs/packages/**, **/.packages/**" });

        IEnumerable<string> matchingFiles = matcher.GetResultsInFullPath(Driver.TargetDirectory);

        // Parse matching files with diff command to detect binary files
        // Need to check that the file is not UTF-16 encoded because diff can return false positives
        var tasks = matchingFiles
            .Select(async file =>
            {
                string output = await ProcessManager.Execute("diff", $"/dev/null \"{file}\"");
                return output.StartsWith("Binary") && await IsNotUTF16(file) ? file : null;
            });

        var binaryFiles = (await Task.WhenAll(tasks)).Where(file => file != null).Select(file => file!).Select(file => file.Substring(Driver.TargetDirectory.Length + 1));

        File.WriteAllLines(Driver.DetectedBinariesFile, binaryFiles);
        
        Driver.Log.LogInformation($"Finished binary detection. Wrote all detected binaries to {Driver.DetectedBinariesFile}.");

        return true;
    }

    private static async Task<bool> IsNotUTF16(string file)
    {
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            string output = await ProcessManager.Execute("file", file);

            if (output.Contains(Utf16Marker))
            {
                return false;
            }
        }
        return true;
    }
}
