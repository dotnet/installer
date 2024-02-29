// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;

namespace BinaryTool;

public partial class Driver
{

    // Target directory - where to look for binaries
    private readonly string TargetDirectory;

    // Allowed binaries keep file - binaries allowed in the target directory that should be kept. Needed for validation and cleaning.
    private readonly string? AllowedBinariesKeepFile;

    // Allowed binaries remove file - binaries allowed in the target directory that should be removed. Needed for validation.
    private readonly string? AllowedBinariesRemoveFile;

    // Updated allowed binaries keep file - updated baseline of allowed binaries to keep
    private readonly string? UpdatedAllowedBinariesKeepFile;

    // Updated allowed binaries remove file - updated baseline of allowed binaries to remove
    private readonly string? UpdatedAllowedBinariesRemoveFile;

    // New binaries file - list of new binaries detected in the target directory
    private readonly string? NewBinariesFile;

    // Mode to run the binary detection in
    private Mode.ModeOptions ModeOption;

    // Logger
    private ILogger Log;

    public Driver(string targetDirectory, string outputReportDirectory, string? allowedBinariesKeepFile, string? allowedBinariesRemoveFile, Mode.ModeOptions mode)
    {
        ModeOption = Mode.GetFullMode(mode);
        Log = ConfigureLogger();

        TargetDirectory = GetAndValidateFullPath(targetDirectory, true, false)!;

        if (ModeOption != Mode.ModeOptions.clean)
        {
            string outDir = GetAndValidateFullPath(outputReportDirectory, true, true)!;

            UpdatedAllowedBinariesKeepFile = Path.Combine(outDir, "UpdatedAllowedBinariesKeepFile.txt");
            UpdatedAllowedBinariesRemoveFile = Path.Combine(outDir, "UpdatedAllowedBinariesRemoveFile.txt");
            NewBinariesFile = Path.Combine(outDir, "NewBinaries.txt");
        }

        AllowedBinariesKeepFile = GetAndValidateFullPath(allowedBinariesKeepFile, false, false);
        AllowedBinariesRemoveFile = GetAndValidateFullPath(allowedBinariesRemoveFile, false, false);
    }

    public async Task ExecuteAsync()
    {
        DateTime startTime = DateTime.Now;

        Log.LogInformation($"Starting binary tool at {startTime}");

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
