// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.Build.Framework;

public partial class PackageDiff : PackageDiffBase
{
    [Required]
    public string BaselinePackage {get; set;} = "";

    [Required]
    public string TestPackage {get; set;} = "";

    public override async Task<bool> ExecuteAsync()
    {
        return await DiffPackages(BaselinePackage, TestPackage);
    }
}

public partial class PackagesDiff: PackageDiffBase
{
    [Required]
    public ITaskItem[] BaselinePackages {get; set;} = [];

    [Required]
    public ITaskItem[] TestPackages {get; set;} = [];

    public override async Task<bool> ExecuteAsync()
    {
        if (TestPackages.Length != BaselinePackages.Length)
        {
            Log.LogError("BaselinePackages and TestPackages must have the same length");
            return false;
        }

        for(int i = 0; i < BaselinePackages.Length; i++)
        {
            Log.LogMessage(MessageImportance.High, $"Comparing {BaselinePackages[i].ItemSpec} and {TestPackages[i].ItemSpec}");
            _ = await DiffPackages(BaselinePackages[i].ItemSpec, TestPackages[i].ItemSpec);
        }
        return true;
    }
}