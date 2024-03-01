// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;

namespace BinaryToolKit;

public partial class BinaryTool
{
    private void RemoveBinaries(IEnumerable<string> binariesToRemove)
    {
        Log.LogInformation($"Removing binaries from {TargetDirectory}...");
        
        foreach (var binary in binariesToRemove)
        {
            File.Delete(Path.Combine(TargetDirectory, binary));
            Log.LogDebug($"    Removed {binary}");
        }

        Log.LogInformation($"Finished binary removal. Removed {binariesToRemove.Count()} binaries.");
    }
}
