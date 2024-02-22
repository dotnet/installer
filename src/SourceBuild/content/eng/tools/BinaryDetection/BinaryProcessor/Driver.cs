// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;

namespace BinaryProcessor;

public static class Driver
{
    // Directory to scan for binaries
    public static string TargetDirectory { get; set; } = string.Empty;

    // Directory to output reports
    public static string OutputReportDirectory { get; set; } = string.Empty;

    // Full path to the binary baseline file
    public static string BinaryBaselineFile { get; set; } = string.Empty;

    public static ILogger Log { get; private set; } = ConfigureLogger();
    public static string DetectedBinariesFile { get; private set; } = string.Empty;
    public static string UpdatedBinaryBaselineFile { get; private set; } = string.Empty;
    public static string NewBinariesFile { get; private set; } = string.Empty;

    public static void Execute()
    {
        DateTime startTime = DateTime.Now;
        Log.LogInformation($"Starting binary detection, validation, and removal tool from {Environment.CurrentDirectory}...");

        ProcessParameters();
        DetectBinaries.Execute();
        CompareBinariesAgainstBaseline.Execute();
        RemoveBinaries.Execute();

        Log.LogInformation("Finished binary detection, validation, and removal tool. Took " + (DateTime.Now - startTime).TotalSeconds + " seconds.");
    }

    private static ILogger ConfigureLogger()
    {
        // Setup logger with console output
        using ILoggerFactory loggerFactory =
            LoggerFactory.Create(builder =>
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = false;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                    options.UseUtcTimestamp = true;
                }));
        return loggerFactory.CreateLogger("BinaryProcessor");
    }

    private static void ProcessParameters()
    {
        TargetDirectory = Path.GetFullPath(TargetDirectory);
        if(!Directory.Exists(TargetDirectory))
        {
            Log.LogError($"Target directory {TargetDirectory} does not exist.");
            Environment.Exit(1);
        }

        OutputReportDirectory = Path.GetFullPath(OutputReportDirectory);
        if(!Directory.Exists(OutputReportDirectory))
        {
            Log.LogInformation($"Creating output report directory {OutputReportDirectory}");
            Directory.CreateDirectory(OutputReportDirectory);
        }

        if (!string.IsNullOrEmpty(BinaryBaselineFile) && !File.Exists(BinaryBaselineFile))
        {
            Log.LogError($"Binary baseline file {BinaryBaselineFile} does not exist.");
            Environment.Exit(1);
        }

        // Setup configuration from command line arguments
        DetectedBinariesFile = Path.Combine(OutputReportDirectory, "DetectedBinaries.txt");
        UpdatedBinaryBaselineFile = Path.Combine(OutputReportDirectory, "UpdatedBinaryBaselineFile.txt");
        NewBinariesFile = Path.Combine(OutputReportDirectory, "NewBinaries.txt");
    }
}
