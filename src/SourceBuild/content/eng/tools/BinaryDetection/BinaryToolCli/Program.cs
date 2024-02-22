// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BinaryProcessor;

namespace BinaryToolCli;
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine($"Args: {string.Join(", ", args)}");
        if (args.Length < 2 || args.Length > 3)
        {
            Console.WriteLine("Invalid number of arguments. Usage: <rootDir> <outputReportDir> optional:<binaryBaselineFile>");
            Environment.Exit(1);
        }

        Driver.TargetDirectory = args[0];
        Driver.OutputReportDirectory = args[1];

        if (args.Length == 3)
        {
            Driver.BinaryBaselineFile = args[2];
        }

        Driver.Execute();
    }
}