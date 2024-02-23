// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BinaryTool;

namespace BinaryToolCli;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2 || args.Length > 6)
        {
            Console.WriteLine("Usage: BinaryToolCli <target-directory> <output-report-directory> [--allowed-binaries-keep-file <file> | --keep <file>] [--allowed-binaries-remove-file <file> | --remove <file>]");
            return;
        }

        var targetDirectory = args[0];
        var outputReportDirectory = args[1];
        var allowedBinariesKeepFile = "";
        var allowedBinariesRemoveFile = "";

        for (int i = 2; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--allowed-binaries-keep-file":
                case "--keep":
                    if (++i < args.Length)
                    {
                        allowedBinariesKeepFile = args[i];
                    }
                    break;
                case "--allowed-binaries-remove-file":
                case "--remove":
                    if (++i < args.Length)
                    {
                        allowedBinariesRemoveFile = args[i];
                    }
                    break;
            }
        }

        Driver.TargetDirectory = targetDirectory;
        Driver.OutputReportDirectory = outputReportDirectory;
        Driver.AllowedBinariesKeepFile = allowedBinariesKeepFile;
        Driver.AllowedBinariesRemoveFile = allowedBinariesRemoveFile;

        Driver.Execute();
    }
}