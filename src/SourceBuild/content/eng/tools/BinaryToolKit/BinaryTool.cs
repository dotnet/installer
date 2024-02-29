// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;

namespace BinaryToolKit;

public partial class BinaryTool
{

    // Target directory - where to look for binaries
    private readonly string TargetDirectory;

    // Allowed binaries file - known binaries in the target directory that should not be removed. Needed for validation and cleaning.
    private readonly string? AllowedBinariesFile;

    // Disallowed source build binaries file - known binaries in the target directory that are disallowed for source-build. Needed for validation.
    private readonly string? DisallowedSbBinariesFile;

    // Updated allowed binaries file - updated baseline of allowed binaries
    private readonly string? UpdatedAllowedBinariesFile;

    // Updated disallowed source build binaries file - updated baseline of disallowed source build binaries
    private readonly string? UpdatedDisallowedSbBinariesFile;

    // New binaries file - list of new binaries detected in the target directory
    private readonly string? NewBinariesFile;

    // Mode to run the binary detection in
    private Mode.ModeOptions ModeOption;

    // Logger
    private ILogger Log;

    public BinaryTool(string targetDirectory, string outputReportDirectory, string? allowedBinariesFile, string? disallowedSbBinariesFile, Mode.ModeOptions mode)
    {
        ModeOption = Mode.GetFullMode(mode);
        Log = ConfigureLogger();

        TargetDirectory = GetAndValidateFullPath(targetDirectory, true, false)!;

        if (ModeOption != Mode.ModeOptions.clean)
        {
            string outDir = GetAndValidateFullPath(outputReportDirectory, true, true)!;

            UpdatedAllowedBinariesFile = Path.Combine(outDir, "UpdatedAllowedBinariesFile.txt");
            UpdatedDisallowedSbBinariesFile = Path.Combine(outDir, "UpdatedDisallowedSbBinariesFile.txt");
            NewBinariesFile = Path.Combine(outDir, "NewBinaries.txt");
        }

        AllowedBinariesFile = GetAndValidateFullPath(allowedBinariesFile, false, false);
        DisallowedSbBinariesFile = GetAndValidateFullPath(disallowedSbBinariesFile, false, false);
    }

    public async Task ExecuteAsync()
    {
        DateTime startTime = DateTime.Now;

        Log.LogInformation($"Starting binary tool at {startTime} in {ModeOption} mode");

        var detectedBinaries = await DetectBinariesAsync();
        var comparedBinaries = CompareBinariesAgainstBaselines(detectedBinaries);

        if (ModeOption == Mode.ModeOptions.both || ModeOption == Mode.ModeOptions.clean)
        {
            RemoveBinaries(comparedBinaries);
        }

        Log.LogInformation("Finished all binary tasks. Took " + (DateTime.Now - startTime).TotalSeconds + " seconds.");
    }

    private ILogger ConfigureLogger()
    {
        var logLevel = Environment.GetEnvironmentVariable("LOG_LEVEL");
        LogLevel level = Enum.TryParse<LogLevel>(logLevel, out var parsedLevel) ? parsedLevel : LogLevel.Information;

        using ILoggerFactory loggerFactory =
            LoggerFactory.Create(builder =>
                builder.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                    options.UseUtcTimestamp = true;
                })
                .SetMinimumLevel(level));
        return loggerFactory.CreateLogger("BinaryTool");
    }

    private string? GetAndValidateFullPath(string? path, bool isDirectory, bool createIfNotExist)
    {
        if (!string.IsNullOrEmpty(path))
        {
            string fullPath = Path.GetFullPath(path);
            bool exists = isDirectory ? Directory.Exists(fullPath) : File.Exists(fullPath);

            if (!exists)
            {
                if (createIfNotExist && isDirectory)
                {
                    Log.LogInformation($"Creating directory {fullPath}");
                    Directory.CreateDirectory(fullPath);
                }
                else
                {
                    Log.LogError($"{(isDirectory ? "Directory" : "File")} {fullPath} does not exist.");
                    Environment.Exit(1);
                }
            }
            return fullPath;
        }
        return null;
    }
}
