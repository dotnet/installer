// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;

namespace BinaryTool;

public partial class Driver
{

    // Target directory - where to look for binaries
    private string TargetDirectory { get; set; }

    // Allowed binaries keep file - binaries allowed in the target directory that should be kept
    private string AllowedBinariesKeepFile { get; set; } = string.Empty;

    // Allowed binaries remove file - binaries allowed in the target directory that should be removed
    private string AllowedBinariesRemoveFile { get; set; } = string.Empty;

    // Detected binaries file - list of detected binaries in the target directory
    private string DetectedBinariesFile { get; set; }

    // Updated allowed binaries keep file - updated baseline of allowed binaries to keep
    private string UpdatedAllowedBinariesKeepFile { get; set; }

    // Updated allowed binaries remove file - updated baseline of allowed binaries to remove
    private string UpdatedAllowedBinariesRemoveFile { get; set; }

    // New binaries file - list of new binaries detected in the target directory
    private string NewBinariesFile { get; set; }

    // Removed binaries file - list of removed binaries from the target directory
    private string RemovedBinariesFile { get; set; }

    // Logger
    private ILogger Log { get; set; }

    public Driver(string targetDirectory, string outputReportDirectory, string allowedBinariesKeepFile, string allowedBinariesRemoveFile)
    {
        Log = ConfigureLogger();

        TargetDirectory = Path.GetFullPath(targetDirectory);
        if(!Directory.Exists(TargetDirectory))
        {
            Log.LogError($"Target directory {TargetDirectory} does not exist.");
            Environment.Exit(1);
        }

        string outDir = Path.GetFullPath(outputReportDirectory);
        if(!Directory.Exists(outDir))
        {
            Log.LogInformation($"Creating output report directory {outDir}");
            Directory.CreateDirectory(outDir);
        }

        DetectedBinariesFile = Path.Combine(outDir, "DetectedBinaries.txt");
        UpdatedAllowedBinariesKeepFile = Path.Combine(outDir, "UpdatedAllowedBinariesKeepFile.txt");
        UpdatedAllowedBinariesRemoveFile = Path.Combine(outDir, "UpdatedAllowedBinariesRemoveFile.txt");
        NewBinariesFile = Path.Combine(outDir, "NewBinaries.txt");
        RemovedBinariesFile = Path.Combine(outDir, "RemovedBinaries.txt");

        if (!string.IsNullOrEmpty(allowedBinariesKeepFile))
        {
            AllowedBinariesKeepFile = Path.GetFullPath(allowedBinariesKeepFile);
            if (!File.Exists(AllowedBinariesKeepFile))
            {
                Log.LogError($"Allowed retainable binaries baseline file {AllowedBinariesKeepFile} does not exist.");
                Environment.Exit(1);
            }
        }

        if (!string.IsNullOrEmpty(allowedBinariesRemoveFile))
        {
            AllowedBinariesRemoveFile = Path.GetFullPath(allowedBinariesRemoveFile);
            if (!File.Exists(AllowedBinariesRemoveFile))
            {
                Log.LogError($"Allowed removeable binaries baseline file {AllowedBinariesRemoveFile} does not exist.");
                Environment.Exit(1);
            }
        }
    }

    public void Execute()
    {
        DateTime startTime = DateTime.Now;
        Log.LogInformation($"Starting binary detection, validation, and removal tool from {Environment.CurrentDirectory}...");

        DetectBinaries();
        CompareBinariesAgainstBaselines();
        RemoveBinaries();

        Log.LogInformation("Finished binary detection, validation, and removal tool. Took " + (DateTime.Now - startTime).TotalSeconds + " seconds.");
    }

    private ILogger ConfigureLogger()
    {
        using ILoggerFactory loggerFactory =
            LoggerFactory.Create(builder =>
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = false;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss";
                    options.UseUtcTimestamp = true;
                }));
        return loggerFactory.CreateLogger("BinaryTool");
    }
}
