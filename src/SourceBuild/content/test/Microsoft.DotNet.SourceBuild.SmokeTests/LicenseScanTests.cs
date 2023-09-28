// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.SourceBuild.SmokeTests;

[Trait("Category", "LicenseScan")]
public class LicenseScanTests : TestBase
{
    private static readonly string[] s_allowedLicenseExpressions = new string[]
    {
        "apache-1.1", // https://opensource.org/license/apache-1-1/
        "apache-2.0", // https://opensource.org/license/apache-2-0/
        "apache-2.0 WITH llvm-exception", // https://foundation.llvm.org/relicensing/LICENSE.txt
        "apsl-2.0", // https://opensource.org/license/apsl-2-0-php/
        "bsd-new", // https://opensource.org/license/BSD-3-clause/
        "bsd-original", // https://github.com/nexB/scancode-toolkit/blob/develop/src/licensedcode/data/licenses/bsd-original.LICENSE
        "bsd-original-uc", // https://github.com/nexB/scancode-toolkit/blob/develop/src/licensedcode/data/licenses/bsd-original-uc.LICENSE
        "bsd-simplified", // https://opensource.org/license/bsd-2-clause/
        "bytemark", // https://github.com/nexB/scancode-toolkit/blob/develop/src/licensedcode/data/licenses/bytemark.LICENSE
        "bzip2-libbzip-2010", // https://github.com/nexB/scancode-toolkit/blob/develop/src/licensedcode/data/licenses/bzip2-libbzip-2010.LICENSE
        "cc0-1.0", // https://creativecommons.org/publicdomain/zero/1.0/legalcode
        "cc-by-sa-3.0", // https://creativecommons.org/licenses/by-sa/3.0/legalcode
        "cc-by-sa-4.0", // https://creativecommons.org/licenses/by-sa/4.0/legalcode
        "cc-pd", // https://creativecommons.org/publicdomain/mark/1.0/
        "cecill-c", // https://cecill.info/licences/Licence_CeCILL-C_V1-en.txt
        "epl-1.0", // https://opensource.org/license/epl-1-0/
        "generic-cla", // https://github.com/nexB/scancode-toolkit/blob/develop/src/licensedcode/data/licenses/generic-cla.LICENSE
        "gpl-1.0-plus", // https://opensource.org/license/gpl-1-0/
        "gpl-2.0", // https://opensource.org/license/gpl-2-0/
        "gpl-2.0-plus WITH autoconf-simple-exception-2.0", // https://github.com/nexB/scancode-toolkit/blob/develop/src/licensedcode/data/rules/gpl-2.0-plus_with_autoconf-simple-exception-2.0_8.RULE
        "gpl-2.0 WITH gcc-linking-exception-2.0", // https://github.com/nexB/scancode-toolkit/blob/develop/src/licensedcode/data/rules/gpl-2.0_with_gcc-linking-exception-2.0_6.RULE
        "isc", // https://opensource.org/license/isc-license-txt/
        "json", // https://www.json.org/license.html
        "lgpl-2.0-plus", // https://opensource.org/license/lgpl-2-0/
        "lgpl-2.1", // https://opensource.org/license/lgpl-2-1/
        "lgpl-2.1-plus", // https://opensource.org/license/lgpl-2-1/
        "mit", // https://opensource.org/license/mit/
        "mit-addition", // https://github.com/nexB/scancode-toolkit/blob/develop/src/licensedcode/data/licenses/mit-addition.LICENSE
        "ms-pl", // https://opensource.org/license/ms-pl-html/
        "ms-rl", // https://opensource.org/license/ms-rl-html/
        "newton-king-cla", // https://github.com/nexB/scancode-toolkit/blob/develop/src/licensedcode/data/licenses/newton-king-cla.LICENSE
        "ngpl", // https://opensource.org/license/nethack-php/
        "ofl-1.1", // https://opensource.org/license/ofl-1-1/
        "osf-1990", // https://fedoraproject.org/wiki/Licensing:MIT?rd=Licensing/MIT#HP_Variant
        "public-domain", // https://github.com/nexB/scancode-toolkit/blob/develop/src/licensedcode/data/licenses/public-domain.LICENSE
        "public-domain-disclaimer", // https://github.com/nexB/scancode-toolkit/blob/develop/src/licensedcode/data/licenses/public-domain-disclaimer.LICENSE
        "rpl-1.5", // https://opensource.org/license/rpl-1-5/
        "unicode", // https://opensource.org/license/unicode-inc-license-agreement-data-files-and-software/
        "uoi-ncsa", // https://opensource.org/license/uoi-ncsa-php/
        "warranty-disclaimer", // https://github.com/nexB/scancode-toolkit/blob/develop/src/licensedcode/data/licenses/warranty-disclaimer.LICENSE
        "zlib" // https://opensource.org/license/zlib/
    };

    private static readonly string[] s_ignoredFilePatterns = new string[]
    {
        "*.bin",
        "*.bmp",
        "*.bson",
        "*.db",
        "*.dic",
        "*.docx",
        "*.eot",
        "*.gif",
        "*.ico",
        "*.jpg",
        "*.il",
        "*.ildump",
        "*.lss",
        "*.nlp",
        "*.otf",
        "*.pdf",
        "*.pfx",
        "*.png",
        "*.rtf",
        "*.snk",
        "*.ttf",
        "*.vsd",
        "*.vsdx",
        "*.winmd",
        "*.woff",
        "*.woff2",
        "*.xlsx",
    };

    private readonly string _targetRepo;

    public LicenseScanTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Assert.NotNull(Config.LicenseScanPath);
        _targetRepo = new DirectoryInfo(Config.LicenseScanPath).Name;
    }

    [SkippableFact(Config.LicenseScanPathEnv, skipOnNullOrWhiteSpaceEnv: true)]
    public void ScanForLicenses()
    {
        Assert.NotNull(Config.LicenseScanPath);

        string OriginalScancodeResultsPath = Path.Combine(LogsDirectory, "scancode-results-original.json");
        string FilteredScancodeResultsPath = Path.Combine(LogsDirectory, "scancode-results-filtered.json");

        // Scancode Doc: https://scancode-toolkit.readthedocs.io/en/latest/index.html
        string ignoreOptions = string.Join(" ", s_ignoredFilePatterns.Select(pattern => $"--ignore {pattern}"));
        ExecuteHelper.ExecuteProcessValidateExitCode(
            "scancode",
            $"--license --strip-root --only-findings {ignoreOptions} --json-pp {OriginalScancodeResultsPath} {Config.LicenseScanPath}",
            OutputHelper);

        JsonDocument doc = JsonDocument.Parse(File.ReadAllText(OriginalScancodeResultsPath));
        ScancodeResults? scancodeResults = doc.Deserialize<ScancodeResults>();
        Assert.NotNull(scancodeResults);

        FilterFiles(scancodeResults);

        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };
        string json = JsonSerializer.Serialize(scancodeResults, options);
        File.WriteAllText(FilteredScancodeResultsPath, json);

        string baselineName = $"Licenses.{_targetRepo}.json";

        string baselinePath = BaselineHelper.GetBaselineFilePath(baselineName);
        if (!File.Exists(baselinePath))
        {
            Assert.Fail($"No license baseline file exists for repo '{_targetRepo}'. Expected file: {baselinePath}");
        }

        BaselineHelper.CompareBaselineContents(baselineName, json, OutputHelper, Config.WarnOnLicenseScanDiffs);
    }

    private LicenseExclusion ParseLicenseExclusion(string rawExclusion)
    {
        string[] parts = rawExclusion.Split('|', StringSplitOptions.RemoveEmptyEntries);

        Match repoNameMatch = Regex.Match(parts[0], @"(?<=src/)[^/]+");

        Assert.True(repoNameMatch.Success);

        // The path in the exclusion file is rooted from the VMR. But the path in the scancode results is rooted from the
        // target repo within the VMR. So we need to strip off the beginning part of the path.
        Match restOfPathMatch = Regex.Match(parts[0], @"(?<=src/[^/]+/).*");
        string path = restOfPathMatch.Value;

        if (parts.Length == 0 || parts.Length > 2)
        {
            throw new Exception($"Invalid license exclusion: '{rawExclusion}'");
        }

        if (parts.Length > 1)
        {
            string[] licenseExpressions = parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries);
            return new LicenseExclusion(repoNameMatch.Value, path, licenseExpressions);
        }
        else
        {
            return new LicenseExclusion(repoNameMatch.Value, path, Enumerable.Empty<string>());
        }
    }

    private void FilterFiles(ScancodeResults scancodeResults)
    {
        IEnumerable<string> rawExclusions = Utilities.ParseExclusionsFile("LicenseExclusions.txt");
        IEnumerable<LicenseExclusion> exclusions = rawExclusions
            .Select(exclusion => ParseLicenseExclusion(exclusion))
            .Where(exclusion => exclusion.Repo == _targetRepo)
            .ToList();

        // This will filter out files that we don't want to include in the baseline.
        // Filtering can happen in two ways:
        //   1. There are a set of allowed license expressions that apply to all files. If a file has a match on one of those licenses,
        //      that license will not be considered.
        //   2. The LicenseExclusions.txt file contains a list of files and the licenses that should be excluded from those files.
        // Once the license expression filtering has been applied, if a file has any licenses left, it will be included in the baseline.
        // In that case, the baseline will list all of the licenses for that file, even if some were originally excluded during this processing.
        // In other words, the baseline will be fully representative of the licenses that apply to the files that are listed there.

        for (int i = scancodeResults.Files.Count - 1; i >= 0; i--)
        {
            ScancodeFileResult file = scancodeResults.Files[i];

            // A license expression can be a logical expression, e.g. "(MIT OR Apache-2.0)"
            // For our purposes, we just care about the license involved, not the semantics of the expression.
            // Parse out all the expression syntax to just get the license names.
            string[] licenses = file.LicenseExpression?
                .Replace("(", string.Empty)
                .Replace(")", string.Empty)
                .Replace(" AND ", ",")
                .Replace(" OR ", ",")
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(license => license.Trim())
                .ToArray()
                ?? Array.Empty<string>();

            // First check whether the file's licenses can all be matched with allowed expressions
            IEnumerable<string> disallowedLicenses = licenses
                .Where(license => !s_allowedLicenseExpressions.Contains(license, StringComparer.OrdinalIgnoreCase));

            if (!disallowedLicenses.Any())
            {
                scancodeResults.Files.Remove(file);
            }
            else
            {
                // There are some licenses that are not allowed. Now check whether the file is excluded.

                IEnumerable<LicenseExclusion> matchingExclusions =
                    Utilities.GetMatchingFileExclusions(file.Path, exclusions, exclusion => exclusion.Path);

                if (matchingExclusions.Any())
                {
                    IEnumerable<string> excludedLicenses = matchingExclusions.SelectMany(exclusion => exclusion.LicenseExpressions);
                    // If no licenses are explicitly specified, it means they're all excluded.
                    if (!excludedLicenses.Any())
                    {
                        scancodeResults.Files.Remove(file);
                    }
                    else
                    {
                        IEnumerable<string> remainingLicenses = disallowedLicenses.Except(excludedLicenses);

                        if (!remainingLicenses.Any())
                        {
                            scancodeResults.Files.Remove(file);
                        }
                    }
                }
            }
        }
    }

    private record LicenseExclusion(string Repo, string Path, IEnumerable<string> LicenseExpressions);
}
