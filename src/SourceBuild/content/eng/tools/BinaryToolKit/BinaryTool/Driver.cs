// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;

namespace BinaryTool;

public static class Driver
{
    // Directory to scan for binaries: required
    public static string TargetDirectory { get; set; } = string.Empty;

    // Directory to output reports: required
    public static string OutputReportDirectory { get; set; } = string.Empty;

    // File for the baseline of allowed retainable binaries: optional
    public static string AllowedBinariesKeepFile { get; set; } = string.Empty;

    // File for the baseline of allowed removeable binaries: optional
    public static string AllowedBinariesRemoveFile { get; set; } = string.Empty;

    // Output files
    public static string DetectedBinariesFile { get; private set; } = string.Empty;
    public static string UpdatedAllowedBinariesKeepFile { get; private set; } = string.Empty;
    public static string UpdatedAllowedBinariesRemoveFile { get; private set; } = string.Empty;
    public static string NewBinariesFile { get; private set; } = string.Empty;
    public static string RemovedBinariesFile { get; private set; } = string.Empty;

    // Logger
    public static ILogger Log { get; private set; } = ConfigureLogger();

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
        using ILoggerFactory loggerFactory =
            LoggerFactory.Create(builder =>
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = false;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                    options.UseUtcTimestamp = true;
                }));
        return loggerFactory.CreateLogger("BinaryTool");
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

        if (!string.IsNullOrEmpty(AllowedBinariesKeepFile))
        {
            AllowedBinariesKeepFile = Path.GetFullPath(AllowedBinariesKeepFile);
            if (!File.Exists(AllowedBinariesKeepFile))
            {
                Log.LogError($"Allowed retainable binaries baseline file {AllowedBinariesKeepFile} does not exist.");
                Environment.Exit(1);
            }
        }

        if (!string.IsNullOrEmpty(AllowedBinariesRemoveFile))
        {
            AllowedBinariesRemoveFile = Path.GetFullPath(AllowedBinariesRemoveFile);
            if (!File.Exists(AllowedBinariesRemoveFile))
            {
                Log.LogError($"Allowed removeable binaries baseline file {AllowedBinariesRemoveFile} does not exist.");
                Environment.Exit(1);
            }
        }

        DetectedBinariesFile = Path.Combine(OutputReportDirectory, "DetectedBinaries.txt");
        UpdatedAllowedBinariesKeepFile = Path.Combine(OutputReportDirectory, "UpdatedAllowedBinariesKeepFile.txt");
        UpdatedAllowedBinariesRemoveFile = Path.Combine(OutputReportDirectory, "UpdatedAllowedBinariesRemoveFile.txt");
        NewBinariesFile = Path.Combine(OutputReportDirectory, "NewBinaries.txt");
        RemovedBinariesFile = Path.Combine(OutputReportDirectory, "RemovedBinaries.txt");
    }
}
