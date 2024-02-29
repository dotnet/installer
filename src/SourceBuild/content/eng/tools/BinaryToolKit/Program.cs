// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.CommandLine;

namespace BinaryToolKit;

class Program
{
    static async Task<int> Main(string[] args)
    {
        CliArgument<string> TargetDirectory = new("target-directory") { Description = "The directory to run the binary tooling on" };
        CliArgument<string> OutputReportDirectory = new("output-report-directory") { Description = "The directory to output the report to" };
        CliOption<string> AllowedBinariesKeepFile = new("--allowed-binaries-keep-file", "-k") { Description = "The file containing the allowed binaries to keep" };
        CliOption<string> AllowedBinariesRemoveFile = new("--allowed-binaries-remove-file", "-r") { Description = "The file containing the allowed binaries to remove" };
        CliOption<Mode.ModeOptions> ModeOption = new("--mode", "-m") { Description = "The mode to run the tool in. Defaults to 'both' ('b').", Arity = ArgumentArity.ZeroOrOne};

        var rootCommand = new CliRootCommand("Tool for detecting, validating, and cleaning binaries in the target directory.")
        {
            TargetDirectory,
            OutputReportDirectory,
            AllowedBinariesKeepFile,
            AllowedBinariesRemoveFile,
            ModeOption
        };

        rootCommand.SetAction(async (result, CancellationToken) =>
        {
            var binaryTool = new BinaryTool(
                result.GetValue(TargetDirectory)!,
                result.GetValue(OutputReportDirectory)!,
                result.GetValue(AllowedBinariesKeepFile),
                result.GetValue(AllowedBinariesRemoveFile),
                result.GetValue(ModeOption));

            await binaryTool.ExecuteAsync();
        });

        // return new CliConfiguration(rootCommand).Invoke(args);
        return await rootCommand.Parse(args).InvokeAsync();
    }
}