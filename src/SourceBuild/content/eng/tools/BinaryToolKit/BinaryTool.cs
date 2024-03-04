// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;

namespace BinaryToolKit;

public partial class BinaryTool
{
    private ILogger Log;

    public BinaryTool() =>
        Log = ConfigureLogger();

    public async Task ExecuteAsync(
        string targetDirectory,
        string outputReportDirectory,
        string? allowedBinariesFile,
        string? disallowedSbBinariesFile,
        Modes mode)
    {
        DateTime startTime = DateTime.Now;

        Log.LogInformation($"Starting binary tool at {startTime} in {mode} mode");

        // Parse args
        targetDirectory = GetAndValidateFullPath(targetDirectory, isDirectory: true, createIfNotExist: false)!;
        outputReportDirectory = GetAndValidateFullPath(outputReportDirectory, isDirectory: true, createIfNotExist: true)!;
        allowedBinariesFile = GetAndValidateFullPath(allowedBinariesFile, isDirectory: false, createIfNotExist: false);
        disallowedSbBinariesFile = GetAndValidateFullPath(disallowedSbBinariesFile, isDirectory: false, createIfNotExist: false);

        // Run the tooling
        var detectedBinaries = await DetectBinariesAsync(targetDirectory);

        var comparedBinaries = CompareBinariesAgainstBaselines(
            detectedBinaries,
            allowedBinariesFile,
            disallowedSbBinariesFile,
            outputReportDirectory,
            targetDirectory,
            mode);

        if (mode.HasFlag(Modes.Clean))
        {
            RemoveBinaries(comparedBinaries, targetDirectory);
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
