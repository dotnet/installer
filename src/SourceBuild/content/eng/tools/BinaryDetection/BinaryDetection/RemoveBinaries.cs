// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace BinaryDetection
{
    public static class RemoveBinaries
    {
        public static void Execute()
        {
            Config.Instance.Log.LogInformation($"Removing binaries from {Config.Instance.RootDir}...");

            IEnumerable<string> binariesToRemove = File.ReadLines(Config.Instance.DetectedBinariesFile);
            
            foreach (var binary in binariesToRemove)
            {
                File.Delete(Path.Combine(Config.Instance.RootDir, binary));
            }

            Config.Instance.Log.LogInformation("Finished binary removal.");
        }
    }
}
