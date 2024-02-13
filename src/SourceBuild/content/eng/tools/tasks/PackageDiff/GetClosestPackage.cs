// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.Build.Framework;

public class GetClosestPackage: GetClosestPackageBase
{
    [Required]
    public string PackagePath { get; set; } = "";

    [Output]
    public string ClosestPackagePath { get; set; } = "";

    public override async Task<bool> ExecuteAsync()
    {
        ClosestPackagePath = await GetClosestPackage(PackagePath);
        return true;
    }
}

public class GetClosestPackages: GetClosestPackageBase
{
    [Required]
    public ITaskItem[] Packages { get; set; } = [];

    [Output]
    public ITaskItem[] ClosestPackagePaths { get; set; } = [];

    public override async Task<bool> ExecuteAsync()
    {
        ClosestPackagePaths = new ITaskItem[Packages.Length];
        for (int i = 0; i < Packages.Length; i++)
        {
            var packagePath = Packages[i].ItemSpec;
            ClosestPackagePaths[i] = new Microsoft.Build.Utilities.TaskItem(await GetClosestPackage(packagePath));
        }

        return true;
    }
}

