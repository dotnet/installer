// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;

namespace BinaryProcessor;
public static class RemoveBinaries
{
    public static void Execute()
    {
        Driver.Log.LogInformation($"Removing binaries from {Driver.TargetDirectory}...");

        IEnumerable<string> binariesToRemove = File.ReadLines(Driver.DetectedBinariesFile);
        
        foreach (var binary in binariesToRemove)
        {
            File.Delete(Path.Combine(Driver.TargetDirectory, binary));
        }

        Driver.Log.LogInformation("Finished binary removal.");
    }
}
