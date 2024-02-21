// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Build.Framework;

public class BinaryDetection: Microsoft.Build.Utilities.ToolTask
{
    [Required]
    public string AcceptedBinaries {get; set;} = "";

    protected override string ToolName { get; } = $"BinaryDetection" + (System.Environment.OSVersion.Platform == PlatformID.Unix ? "" : ".exe");

    protected override MessageImportance StandardOutputLoggingImportance => MessageImportance.High;
    protected override bool HandleTaskExecutionErrors() => true;

    protected override string GenerateFullPathToTool()
    {
        return Path.Combine(Path.GetDirectoryName(typeof(BinaryDetection).Assembly.Location)!, "..", "..", "tools", ToolName);
    }

    protected override string GenerateCommandLineCommands()
    {
        return $"\"{AcceptedBinaries}\"";
    }
}