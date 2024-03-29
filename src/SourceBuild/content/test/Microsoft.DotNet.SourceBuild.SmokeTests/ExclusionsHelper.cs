// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileSystemGlobbing;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.SourceBuild.SmokeTests;

internal static class ExclusionsHelper
{
    private static Dictionary<string, Dictionary<string, HashSet<string>>> FileToSuffixToExclusionsMap = new();

    private static Dictionary<string, Dictionary<string, HashSet<string>>> FileToSuffixToUnusedExclusionsMap = new();
    
    private const string NullSuffix = "NULL_SUFFIX";

    // Use this to narrow down the scope of exclusions to a specific category.
    // For instance, setting this to "test-templates" will consider 
    // "src/test-templates/exclusions.txt" but not "src/arcade/exclusions.txt".
    public static Regex ExclusionRegex;

    static ExclusionsHelper()
    {
        AppDomain.CurrentDomain.ProcessExit += onProcessExit;
    }

    private static void onProcessExit(object? sender, EventArgs e)
    {
        RemoveUnusedExclusionsFromBaselines();
    }

    internal static bool IsFileExcluded(string filePath, string exclusionsFileName, string suffix = NullSuffix)
    {
        // If a specific suffix is provided, check that first. If it is not found, check the default suffix.
        return CheckAndRemoveIfExcluded(filePath, exclusionsFileName, suffix) ||
            (suffix != NullSuffix && CheckAndRemoveIfExcluded(filePath, exclusionsFileName, NullSuffix));
    }

    private static bool CheckAndRemoveIfExcluded(string filePath, string exclusionsFileName, string suffix = NullSuffix)
    {
        Dictionary<string, HashSet<string>> exclusions = GetExclusions(exclusionsFileName);

        if (exclusions.TryGetValue(suffix, out HashSet<string> suffixExclusionList))
        {
            foreach (string exclusion in suffixExclusionList)
            {
                Matcher matcher = new();
                matcher.AddInclude(exclusion);
                if (matcher.Match(filePath).HasMatches)
                {
                    RemoveUsedExclusion(exclusionsFileName, exclusion, suffix);
                    return true;
                }
            }
        }
        return false;
    }

    private static Dictionary<string, HashSet<string>> GetExclusions(string exclusionsFileName)
    {
        if (!FileToSuffixToExclusionsMap.TryGetValue(exclusionsFileName, out Dictionary<string, HashSet<string>> exclusions))
        {
            exclusions = ParseExclusionsFile(exclusionsFileName);
            FileToSuffixToExclusionsMap[exclusionsFileName] = exclusions;
            FileToSuffixToUnusedExclusionsMap[exclusionsFileName] = new Dictionary<string, HashSet<string>>(
                exclusions.ToDictionary(pair => pair.Key, pair => new HashSet<string>(pair.Value)));
        }

        return exclusions;
    }

    private static Dictionary<string, HashSet<string>> ParseExclusionsFile(string exclusionsFileName)
    {
        string exclusionsFilePath = Path.Combine(BaselineHelper.GetAssetsDirectory(), exclusionsFileName);
        return File.ReadAllLines(exclusionsFilePath)
            .Select(line =>
            {
                // Ignore comments
                var index = line.IndexOf('#');
                return index >= 0 ? line[..index].TrimEnd() : line;
            })
            .Where(line => !string.IsNullOrEmpty(line))
            .Select(line => line.Split('|'))
            .Where(parts =>
            {
                // Only include exclusions that match the exclusion regex
                return ExclusionRegex is null || ExclusionRegex.IsMatch(parts[0]);
            })
            .Select(parts => new
            {
                // Split the line into the exclusion and the suffixes
                Line = parts[0],
                Suffixes = parts.Length > 1
                    ? parts[1].Split(',').Select(suffix => suffix.Trim())
                    : new string[] { NullSuffix }
            })
            .SelectMany(parts =>
                // Create a new object for each suffix
                parts.Suffixes.Select(suffix => new
                {
                    parts.Line,
                    Suffix = suffix
                })
            )
            .GroupBy(
                parts => parts.Suffix,
                parts => parts.Line
            )
            .ToDictionary(
                group => group.Key,
                group => new HashSet<string>(group)
            );
    }

    private static void RemoveUsedExclusion(string exclusionsFileName, string exclusion, string suffix)
    {
        if (FileToSuffixToUnusedExclusionsMap.TryGetValue(exclusionsFileName, out Dictionary<string, HashSet<string>> suffixToExclusions)
            && suffixToExclusions.TryGetValue(suffix, out HashSet<string> exclusions))
        {
            exclusions.Remove(exclusion);
        }
    }

    private static void RemoveUnusedExclusionsFromBaselines()
    {

        foreach (KeyValuePair<string, Dictionary<string, HashSet<string>>> fileToUnusedExclusions in FileToSuffixToUnusedExclusionsMap)
        {
            string exclusionsFileName = fileToUnusedExclusions.Key;
            string exclusionsFilePath = Path.Combine(BaselineHelper.GetAssetsDirectory(), exclusionsFileName);
            var suffixesToUnusedExclusions = fileToUnusedExclusions.Value;

            string[] lines = File.ReadAllLines(exclusionsFilePath);

            var newLines = lines
                .Select(line =>
                {
                    if (suffixesToUnusedExclusions.Values.All(exclusions => exclusions.All(exclusion => !line.Contains(exclusion))))
                    {
                        // Line does not contain an unused exclusion, so we can keep it as is
                        return line;
                    }

                    string exclusion = line.Split('|')[0];
                    var unusedSuffixes = suffixesToUnusedExclusions.Where(pair => pair.Value.Contains(exclusion)).Select(pair => pair.Key).ToList();

                    if (unusedSuffixes.Count == 1 && unusedSuffixes[0] == NullSuffix)
                    {
                        // Line does not contain any suffixes, so we can remove it entirely
                        return null;
                    }

                    string suffixString = line.Split('|')[1].Split('#')[0];
                    var originalSuffixes = suffixString.Split(',').Select(suffix => suffix.Trim()).ToList();
                    var newSuffixes = originalSuffixes.Except(unusedSuffixes).ToList();

                    if (newSuffixes.Count == 0)
                    {
                        // All suffixes were unused, so we can remove the line entirely
                        return null;
                    }

                    return line.Replace(suffixString, string.Join(",", newSuffixes));
                })
                .Where(line => line is not null);

            string actualFilePath = Path.Combine(TestBase.LogsDirectory, $"Updated{exclusionsFileName}");
            File.WriteAllLines(actualFilePath, newLines);
        }
    }
}