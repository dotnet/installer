// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;
using BinaryProcessor;

namespace BinaryToolTask;

public class BinaryProcessorToolTask : Task
{
    // Directory to scan for binaries
    [Required]
    public static string TargetDirectory { get; set; } = string.Empty;

    // Directory to output reports
    [Required]
    public static string OutputReportDirectory { get; set; } = string.Empty;

    // Full path to the binary baseline file
    public static string BinaryBaselineFile { get; set; } = string.Empty;

    public override bool Execute()
    {
        Log.LogMessage(MessageImportance.High, "Invoking BinaryProcessor tool...");

        Driver.TargetDirectory = TargetDirectory;
        Driver.OutputReportDirectory = OutputReportDirectory;
        Driver.BinaryBaselineFile = BinaryBaselineFile;

        Driver.Execute();

        return true;
    }
}
