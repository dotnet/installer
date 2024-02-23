// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileSystemGlobbing;

namespace BinaryTool;
public static class CompareBinariesAgainstBaseline
{
    private static readonly string PatternsToRemoveMarker = "# Consider removing the following unused patterns from the baseline file:";

    public static void Execute()
    {
        string againstLogString = 
            !string.IsNullOrEmpty(Driver.AllowedBinariesKeepFile) && !string.IsNullOrEmpty(Driver.AllowedBinariesRemoveFile)
            ? $"against baselines {Driver.AllowedBinariesKeepFile} and {Driver.AllowedBinariesRemoveFile}"
            : string.IsNullOrEmpty(Driver.AllowedBinariesKeepFile)
                ? $"against baseline {Driver.AllowedBinariesRemoveFile}"
            : string.IsNullOrEmpty(Driver.AllowedBinariesRemoveFile)
                ? $"against baseline {Driver.AllowedBinariesKeepFile}"
            : "without a baseline";
        Driver.Log.LogInformation($"Validating binaries in {Driver.TargetDirectory} {againstLogString}...");

        IEnumerable<string> detectedBinaries = File.ReadLines(Driver.DetectedBinariesFile);

        var binariesToKeep = CompareBinaries(detectedBinaries, Driver.AllowedBinariesKeepFile, Driver.UpdatedAllowedBinariesKeepFile);
        var binariesToRemove = CompareBinaries(detectedBinaries, Driver.AllowedBinariesRemoveFile, Driver.UpdatedAllowedBinariesRemoveFile);
        var newBinaries = detectedBinaries.Except(binariesToKeep).Except(binariesToRemove);

        if (newBinaries.Any())
        {
            Driver.Log.LogWarning($"    New binaries detected. Review {Driver.NewBinariesFile} and update the baseline file(s).");
        }

        // Write new binaries to file
        File.WriteAllLines(Driver.NewBinariesFile, newBinaries);

        // Write binaries to remove to file
        File.WriteAllLines(Driver.RemovedBinariesFile, newBinaries);
        File.AppendAllLines(Driver.RemovedBinariesFile, binariesToRemove);

        Driver.Log.LogInformation("Finished binary validation.");
    }

    private static HashSet<string> CompareBinaries(IEnumerable<string> detectedBinaries, string baselineFile, string updatedBaselineFile)
    {
        IEnumerable<string> baselinePatterns = ParseBaselineFile(baselineFile);
        HashSet<string> unusedPatterns = new HashSet<string>(baselinePatterns);
        HashSet<string> matchedBinaries = new HashSet<string>();

        // Compare detected binaries against the baseline patterns
        foreach (string pattern in baselinePatterns)
        {
            Matcher matcher = new Matcher();
            matcher.AddInclude(pattern);
            
            var matches = matcher.Match(Driver.TargetDirectory, detectedBinaries);
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

        Driver.Log.LogInformation($"    Updated baseline file written to {updatedBaselineFile}");

        return matchedBinaries;
    }

    private static IEnumerable<string> ParseBaselineFile(string file) {
        if (!File.Exists(file))
        {
            return Enumerable.Empty<string>();
        }

        // Read the baseline file and parse the patterns, ignoring comments and empty lines
        return File.ReadLines(file)
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
            .Select(line => line.Split('#')[0].Trim());
    }
}
