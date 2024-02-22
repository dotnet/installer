// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileSystemGlobbing;

namespace BinaryProcessor;
public static class CompareBinariesAgainstBaseline
{
    private static readonly string NewlyDetectedBinariesMarker = "# The following binaries have been newly detected:";
    private static readonly string BinariesToRemoveMarker = "# Consider removing the following unused patterns from the baseline file:";

    public static void Execute()
    {
        Driver.Log.LogInformation($"Validating binaries in {Driver.TargetDirectory} against baseline {Driver.BinaryBaselineFile}...");

        if (!File.Exists(Driver.BinaryBaselineFile))
        {
            Driver.Log.LogInformation("No baseline file found. Copying all detected binaries to new baseline...");

            File.Copy(Driver.DetectedBinariesFile, Driver.NewBinariesFile, true);
            File.Copy(Driver.DetectedBinariesFile, Driver.UpdatedBinaryBaselineFile, true);
        }
        else
        {
            IEnumerable<string> detectedBinaries = File.ReadLines(Driver.DetectedBinariesFile);
            IEnumerable<string> baselinePatterns = ParseBaselineFile();
            IEnumerable<string> newBinaries = GetBinariesNotInBaseline(detectedBinaries, baselinePatterns);

            // Write new binaries to file
            File.WriteAllLines(Driver.NewBinariesFile, newBinaries);

            // Write updated baseline file
            File.Copy(Driver.BinaryBaselineFile, Driver.UpdatedBinaryBaselineFile, true);
            File.AppendAllText(Driver.UpdatedBinaryBaselineFile, $"{Environment.NewLine}{NewlyDetectedBinariesMarker}{Environment.NewLine}");
            File.AppendAllLines(Driver.UpdatedBinaryBaselineFile, newBinaries);
            File.AppendAllText(Driver.UpdatedBinaryBaselineFile, $"{Environment.NewLine}{BinariesToRemoveMarker}{Environment.NewLine}");
            File.AppendAllLines(Driver.UpdatedBinaryBaselineFile, GetUnusedBaselinePatterns(detectedBinaries, baselinePatterns));

            Driver.Log.LogInformation($"Updated baseline file written to {Driver.UpdatedBinaryBaselineFile}");
        }

        Driver.Log.LogInformation("Finished binary validation.");
    }

    private static IEnumerable<string> ParseBaselineFile() => 
        File.ReadLines(Driver.BinaryBaselineFile)
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
            .Select(line => line.Split('#')[0].Trim());

    private static IEnumerable<string> GetBinariesNotInBaseline(IEnumerable<string> detectedBinaries, IEnumerable<string> baselinePatterns)
    {
        Matcher matcher = new Matcher();
        matcher.AddInclude("**/*");
        matcher.AddExcludePatterns(baselinePatterns);

        var newBinaries = matcher.Match(Driver.TargetDirectory, detectedBinaries).Files;

        if (newBinaries.Any())
        {
            Driver.Log.LogWarning($"New binaries detected. See {Driver.NewBinariesFile} for details.");
        }

        return newBinaries.Select(file => file.Path);
    }

    private static IEnumerable<string> GetUnusedBaselinePatterns(IEnumerable<string> detectedBinaries, IEnumerable<string> baselinePatterns) =>
        baselinePatterns.Where(pattern => {
            Matcher matcher = new Matcher();
            matcher.AddInclude(pattern);

            return !matcher.Match(Driver.TargetDirectory, detectedBinaries).HasMatches;
        });
}
