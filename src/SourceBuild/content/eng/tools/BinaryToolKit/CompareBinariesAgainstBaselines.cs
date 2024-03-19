// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.FileSystemGlobbing;

namespace BinaryToolKit;

public static class CompareBinariesAgainstBaselines
{
    public static List<string> Execute(
        IEnumerable<string> detectedBinaries,
        string? baselineFile,
        string outputReportDirectory,
        string targetDirectory,
        Modes mode)
    {
        Log.LogInformation($"Comparing detected binaries to baseline {Path.GetFileName(baselineFile)}.");

        var unmatchedBinaries = GetUnmatchedBinaries(
            detectedBinaries,
            baselineFile,
            outputReportDirectory,
            targetDirectory,
            mode).ToList();

        if (mode.HasFlag(Modes.Validate))
        {
            if (unmatchedBinaries.Any())
            {
                string newBinariesFile = Path.Combine(outputReportDirectory, "NewBinaries.txt");

                File.WriteAllLines(newBinariesFile, unmatchedBinaries);

                foreach (var unmatchedBinary in unmatchedBinaries)
                {
                    Log.LogDebug($"    {unmatchedBinary}");
                }

                Log.LogError($"    {unmatchedBinaries.Count()} new binaries detected. Check '{newBinariesFile}' for details.");

            }
        }

        Log.LogInformation("Finished comparing binaries.");

        return unmatchedBinaries;
    }

    private static IEnumerable<string> GetUnmatchedBinaries(
        IEnumerable<string> searchFiles,
        string? baselineFile,
        string outputReportDirectory,
        string targetDirectory,
        Modes mode)
    {
        HashSet<string> unmatchedFiles = new HashSet<string>(searchFiles);

        var filesToPatterns = new Dictionary<string, HashSet<string>>();
        ParseBaselineFile(baselineFile, ref filesToPatterns);

        foreach (var fileToPatterns in filesToPatterns)
        {
            var patterns = fileToPatterns.Value;
            HashSet<string> unusedPatterns = new HashSet<string>(patterns);

            foreach (string pattern in patterns)
            {
                Matcher matcher = new Matcher(StringComparison.Ordinal);
                matcher.AddInclude(pattern);
                
                var matches = matcher.Match(targetDirectory, searchFiles);
                if (matches.HasMatches)
                {
                    unusedPatterns.Remove(pattern);
                    unmatchedFiles.ExceptWith(matches.Files.Select(file => file.Path));
                }
            }

            UpdateBaselineFile(fileToPatterns.Key, outputReportDirectory, unusedPatterns);
        }

        return unmatchedFiles;
    }

    private static void ParseBaselineFile(string? file, ref Dictionary<string, HashSet<string>> result)
    {
        if (!File.Exists(file))
        {
            return;
        }

        if (!result.ContainsKey(file))
        {
            result[file] = new HashSet<string>();
        }

        foreach (var line in File.ReadLines(file))
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
            {
                continue;
            }

            if (trimmedLine.StartsWith("import:"))
            {
                var importFile = trimmedLine.Substring("import:".Length).Trim();
                if (!Path.IsPathFullyQualified(importFile))
                {
                    var currentDirectory = Path.GetDirectoryName(file)!;
                    importFile = Path.Combine(currentDirectory, importFile);
                }
                if (result.ContainsKey(importFile))
                {
                    Log.LogWarning($"    Duplicate import {importFile}. Skipping.");
                    continue;
                }

                ParseBaselineFile(importFile, ref result);
            }
            else
            {
                result[file].Add(trimmedLine.Split('#')[0].Trim());
            }
        }
    }

    private static void UpdateBaselineFile(string? file, string outputReportDirectory, HashSet<string> unusedPatterns)
    {
        if(File.Exists(file) && unusedPatterns.Any())
        {
            var lines = File.ReadAllLines(file);
            var newLines = lines.Where(line => !unusedPatterns.Contains(line)).ToList();

            string updatedFile = Path.Combine(outputReportDirectory, "Updated" + Path.GetFileName(file));

            File.WriteAllLines(updatedFile, newLines);

            Log.LogInformation($"    Updated baseline file '{file}' written to '{updatedFile}'");
        }
    }
}