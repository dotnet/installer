// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using BinaryTool;

namespace BinaryToolCli;
class Program
{
    static async Task Main(string[] args)
    {
        var rootCommand = new RootCommand("Binary Detection, Validation, and Removal Tool");

        var targetDirectory = new Argument<string>(
            "target-directory",
            "Directory to scan for binaries");
        
        var outputReportDirectory = new Argument<string>(
            "output-report-directory",
            "Directory to output reports");
        
        var retainableBinariesFile = new Option<string>(
            "--allowed-binaries-keep-file",
            "File for the baseline of binaries that, if found, should be kept in the VMR and not errored for.");
        retainableBinariesFile.AddAlias("--keep");
        
        var removableBinariesFile = new Option<string>(
            "--allowed-binaries-remove-file",
            "File for the baseline of binaries that, if found, should be removed from the VMR but not errored for.");
        removableBinariesFile.AddAlias("--remove");

        rootCommand.AddArgument(targetDirectory);
        rootCommand.AddArgument(outputReportDirectory);
        rootCommand.AddOption(retainableBinariesFile);
        rootCommand.AddOption(removableBinariesFile);

        rootCommand.SetHandler((targetDirectory, outputReportDirectory, allowedBinariesKeepFile, allowedBinariesRemoveFile) =>
        {
            Driver.TargetDirectory = targetDirectory;
            Driver.OutputReportDirectory = outputReportDirectory;
            Driver.AllowedBinariesKeepFile = allowedBinariesKeepFile;
            Driver.AllowedBinariesRemoveFile = allowedBinariesRemoveFile;

            Driver.Execute();
        }, targetDirectory, outputReportDirectory, retainableBinariesFile, removableBinariesFile);

        await rootCommand.InvokeAsync(args);
    }
}