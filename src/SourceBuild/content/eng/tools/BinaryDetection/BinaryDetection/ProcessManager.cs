// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Task = Microsoft.Build.Utilities.Task;

namespace BinaryDetection
{
    public static class ProcessManager
    {
        public static async Task<string> Execute(string executable, string arguments, string rootDir)
        {
            ProcessStartInfo psi = new ()
            {
                FileName = executable,
                Arguments = arguments,
                WorkingDirectory = rootDir,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var proc = Process.Start(psi);

            string output = await proc.StandardOutput.ReadToEndAsync();
            string error = await proc.StandardError.ReadToEndAsync();

            await proc.WaitForExitAsync();

            if (!string.IsNullOrEmpty(error))
            {
                Config.Instance.Log.LogError(error);
            }

            return output;
        }
    }
}
