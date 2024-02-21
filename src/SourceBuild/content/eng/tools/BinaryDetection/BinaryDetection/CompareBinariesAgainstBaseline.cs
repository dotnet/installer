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

namespace BinaryDetection
{
    public static class CompareBinariesAgainstBaseline
    {
        private static readonly string NewlyDetectedBinariesMarker = "# The following binaries have been newly detected:";
        private static readonly string BinariesToRemoveMarker = "# Consider removing the following unused patterns from the baseline file:";

        public static void Execute()
        {
            Config.Instance.Log.LogInformation($"Validating binaries in {Config.Instance.RootDir} against baseline {Config.Instance.BinaryBaselineFile}...");

            if (!File.Exists(Config.Instance.BinaryBaselineFile))
            {
                Config.Instance.Log.LogInformation("No baseline file found. Copying all detected binaries to new baseline...");

                File.Copy(Config.Instance.DetectedBinariesFile, Config.Instance.NewBinariesFile, true);
                File.Copy(Config.Instance.DetectedBinariesFile, Config.Instance.UpdatedBinaryBaselineFile, true);
            }
            else
            {
                IEnumerable<string> detectedBinaries = File.ReadLines(Config.Instance.DetectedBinariesFile);
                IEnumerable<string> baselinePatterns = ParseBaselineFile();
                IEnumerable<string> newBinaries = GetBinariesNotInBaseline(detectedBinaries, baselinePatterns);

                // Write new binaries to file
                File.WriteAllLines(Config.Instance.NewBinariesFile, newBinaries);

                // Write updated baseline file
                File.Copy(Config.Instance.BinaryBaselineFile, Config.Instance.UpdatedBinaryBaselineFile, true);
                File.AppendAllText(Config.Instance.UpdatedBinaryBaselineFile, $"{Environment.NewLine}{NewlyDetectedBinariesMarker}{Environment.NewLine}");
                File.AppendAllLines(Config.Instance.UpdatedBinaryBaselineFile, newBinaries);
                File.AppendAllText(Config.Instance.UpdatedBinaryBaselineFile, $"{Environment.NewLine}{BinariesToRemoveMarker}{Environment.NewLine}");
                File.AppendAllLines(Config.Instance.UpdatedBinaryBaselineFile, GetUnusedBaselinePatterns(detectedBinaries, baselinePatterns));

                Config.Instance.Log.LogInformation($"Updated baseline file written to {Config.Instance.UpdatedBinaryBaselineFile}");
            }

            Config.Instance.Log.LogInformation("Finished binary validation.");
        }

        private static IEnumerable<string> ParseBaselineFile() => 
            File.ReadLines(Config.Instance.BinaryBaselineFile)
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                .Select(line => line.Split('#')[0].Trim());

        private static IEnumerable<string> GetBinariesNotInBaseline(IEnumerable<string> detectedBinaries, IEnumerable<string> baselinePatterns)
        {
            Matcher matcher = new Matcher();
            matcher.AddInclude("**/*");
            matcher.AddExcludePatterns(baselinePatterns);

            var newBinaries = matcher.Match(Config.Instance.RootDir, detectedBinaries).Files;

            if (newBinaries.Any())
            {
                Config.Instance.Log.LogWarning($"New binaries detected. See {Config.Instance.NewBinariesFile} for details.");
            }

            return newBinaries.Select(file => file.Path);
        }

        private static IEnumerable<string> GetUnusedBaselinePatterns(IEnumerable<string> detectedBinaries, IEnumerable<string> baselinePatterns) =>
            baselinePatterns.Where(pattern => {
                Matcher matcher = new Matcher();
                matcher.AddInclude(pattern);

                return !matcher.Match(Config.Instance.RootDir, detectedBinaries).HasMatches;
            });
    }
}
