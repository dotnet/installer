// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;

namespace BinaryToolKit;

public partial class BinaryTool
{
    private const string PatternsToRemoveMarker = "# Consider removing the following unused patterns from the baseline file:";

    private List<string> CompareBinariesAgainstBaselines(IEnumerable<string> detectedBinaries)
    {
        Log.LogInformation("Comparing detected binaries to baseline(s).");

        var binariesToRemove = GetUnmatchedBinaries(detectedBinaries, AllowedBinariesFile, UpdatedAllowedBinariesFile).ToList();

        if (ModeOption != Mode.ModeOptions.clean)
        {
            var nonSbBinariesToRemove = GetUnmatchedBinaries(detectedBinaries, DisallowedSbBinariesFile, UpdatedDisallowedSbBinariesFile).ToList();
            var newBinaries = binariesToRemove.Intersect(nonSbBinariesToRemove);

            if (newBinaries.Any())
            {
                Log.LogWarning($"    {newBinaries.Count()} new binaries detected. Check {NewBinariesFile}");
                File.WriteAllLines(NewBinariesFile!, newBinaries);
            }
        }

        Log.LogInformation("Finished comparing binaries.");

        return binariesToRemove;
    }

    private IEnumerable<string> GetUnmatchedBinaries(IEnumerable<string> searchFiles, string? baselineFile, string? updatedBaselineFile)
    {
        var patterns = ParseBaselineFile(baselineFile);

        if (ModeOption == Mode.ModeOptions.clean)
        {
            Matcher matcher = new Matcher(StringComparison.Ordinal);
            matcher.AddInclude("**/*");
            matcher.AddExcludePatterns(patterns);

            return matcher.Match(TargetDirectory, searchFiles).Files.Select(file => file.Path);
        }
        else
        {
            HashSet<string> unusedPatterns = new HashSet<string>(patterns);
            HashSet<string> unmatchedFiles = new HashSet<string>(searchFiles);

            foreach (string pattern in patterns)
            {
                Matcher matcher = new Matcher(StringComparison.Ordinal);
                matcher.AddInclude(pattern);
                
                var matches = matcher.Match(TargetDirectory, searchFiles);
                if (matches.HasMatches)
                {
                    unusedPatterns.Remove(pattern);
                    unmatchedFiles.ExceptWith(matches.Files.Select(file => file.Path));
                }
            }

            UpdateBaselineFile(baselineFile!, updatedBaselineFile!, unusedPatterns);

            return unmatchedFiles;
        }
    }

    private IEnumerable<string> ParseBaselineFile(string? file) {
        if (!File.Exists(file))
        {
            return Enumerable.Empty<string>();
        }

        // Read the baseline file and parse the patterns, ignoring comments and empty lines
        return File.ReadLines(file)
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
            .Select(line => line.Split('#')[0].Trim());
    }

    private void UpdateBaselineFile(string? file, string updatedFile, HashSet<string> unusedPatterns)
    {
        if(File.Exists(file))
        {
            File.Copy(file, updatedFile, true);
            File.AppendAllText(updatedFile, $"{Environment.NewLine}{PatternsToRemoveMarker}{Environment.NewLine}");
            File.AppendAllLines(updatedFile, unusedPatterns);

            Log.LogInformation($"    Updated baseline file {file} written to {updatedFile}");
        }
    }
}
