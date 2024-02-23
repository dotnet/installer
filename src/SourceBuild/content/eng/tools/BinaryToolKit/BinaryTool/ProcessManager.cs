// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BinaryTool;
public static class ProcessManager
{
    public static async Task<string> Execute(string executable, string arguments)
    {
        ProcessStartInfo psi = new ()
        {
            FileName = executable,
            Arguments = arguments,
            WorkingDirectory = Driver.TargetDirectory,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var proc = Process.Start(psi)!;

        string output = await proc.StandardOutput.ReadToEndAsync();
        string error = await proc.StandardError.ReadToEndAsync();

        await proc.WaitForExitAsync();

        if (!string.IsNullOrEmpty(error))
        {
            Driver.Log.LogError(error);
        }

        return output;
    }
}
