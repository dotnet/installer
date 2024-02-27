// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;

namespace BinaryTool;
public partial class Driver
{
    private const string PatternsToRemoveMarker = "# Consider removing the following unused patterns from the baseline file:";

    // Requires: DetectedBinariesFile exists
    // Modifies: UpdatedAllowedBinariesKeepFile, UpdatedAllowedBinariesRemoveFile, NewBinariesFile, RemovedBinariesFile
    // Effects:  Compares the detected binaries against the baseline patterns and writes the results to the output files.
    private void CompareBinariesAgainstBaselines()
    {
        Log.LogInformation($"Validating binaries in {TargetDirectory} {GetBaselineLogString()}...");

        IEnumerable<string> detectedBinaries = File.ReadLines(DetectedBinariesFile);

        var binariesToKeep = MatchBinariesAndUpdateBaseline(detectedBinaries, AllowedBinariesKeepFile, UpdatedAllowedBinariesKeepFile);
        var binariesToRemove = MatchBinariesAndUpdateBaseline(detectedBinaries, AllowedBinariesRemoveFile, UpdatedAllowedBinariesRemoveFile);
        var newBinaries = detectedBinaries.Except(binariesToKeep).Except(binariesToRemove);

        if (newBinaries.Any())
        {
            Log.LogWarning($"    New binaries detected. Review {NewBinariesFile} and update the baseline file(s).");
        }

        // Write new binaries to file
        File.WriteAllLines(NewBinariesFile, newBinaries);

        // Write binaries to remove to file
        File.WriteAllLines(RemovedBinariesFile, newBinaries);
        File.AppendAllLines(RemovedBinariesFile, binariesToRemove);

        Log.LogInformation("Finished binary validation.");
    }

    private HashSet<string> MatchBinariesAndUpdateBaseline(IEnumerable<string> detectedBinaries, string? baselineFile, string updatedBaselineFile)
    {
        IEnumerable<string> baselinePatterns = ParseBaselineFile(baselineFile);
        HashSet<string> unusedPatterns = new HashSet<string>(baselinePatterns);
        HashSet<string> matchedBinaries = new HashSet<string>();

        // Compare detected binaries against the baseline patterns
        foreach (string pattern in baselinePatterns)
        {
            Matcher matcher = new Matcher();
            matcher.AddInclude(pattern);
            
            var matches = matcher.Match(TargetDirectory, detectedBinaries);
            if (matches.HasMatches)
            {
                unusedPatterns.Remove(pattern);
                matchedBinaries.UnionWith(matches.Files.Select(file => file.Path));
            }
        }

        // Write the updated baseline file
        if(File.Exists(baselineFile))
        {
            File.Copy(baselineFile, updatedBaselineFile, true);
        }
        File.AppendAllText(updatedBaselineFile, $"{Environment.NewLine}{PatternsToRemoveMarker}{Environment.NewLine}");
        File.AppendAllLines(updatedBaselineFile, unusedPatterns);

        Log.LogInformation($"    Updated baseline file written to {updatedBaselineFile}");

        return matchedBinaries;
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

    private string GetBaselineLogString()
    {
        if (!string.IsNullOrEmpty(AllowedBinariesKeepFile) && !string.IsNullOrEmpty(AllowedBinariesRemoveFile))
        {
            return $"against baselines {AllowedBinariesKeepFile} and {AllowedBinariesRemoveFile}";
        }
        else if (string.IsNullOrEmpty(AllowedBinariesKeepFile))
        {
            return $"against baseline {AllowedBinariesRemoveFile}";
        }
        else if (string.IsNullOrEmpty(AllowedBinariesRemoveFile))
        {
            return $"against baseline {AllowedBinariesKeepFile}";
        }

        return "without a baseline";
    }
}
