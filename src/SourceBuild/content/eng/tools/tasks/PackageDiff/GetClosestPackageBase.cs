// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;

public abstract class GetClosestPackageBase : NugetPackageTaskBase
{
    public async Task<string> GetClosestPackage(string packagePath)
    {
        if(await GetPackageFromFile(packagePath) is not {} pack)
            return "";

        // TODO: Find the correct closest package version from the official build feeds.
        var (name, version) = pack.GetPackageNameAndVersion();
        return "https://nuget.org/api/v2/package/" + name + "/" + version;
    }
}

