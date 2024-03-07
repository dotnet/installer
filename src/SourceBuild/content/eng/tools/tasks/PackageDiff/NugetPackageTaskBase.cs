// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Framework;

public abstract class NugetPackageTaskBase : Microsoft.Build.Utilities.Task, ICancelableTask
{
    private CancellationTokenSource _cts = new CancellationTokenSource();
    protected CancellationToken _cancellationToken => _cts.Token;
    public abstract Task<bool> ExecuteAsync();

    public override bool Execute()
    {
        return Task.Run(ExecuteAsync).Result;
    }

    public async Task<(ZipArchive, ZipArchive)?> GetPackages(string packagePath1, string packagePath2)
    {
        _cancellationToken.ThrowIfCancellationRequested();
        var package1 = await GetPackageFromPathOrUri(packagePath1);
        var package2 = await GetPackageFromPathOrUri(packagePath2);
        if (package1 is null || package2 is null)
            return null;
        return (package1, package2);
    }

    public async Task<ZipArchive?> GetPackageFromPathOrUri(string pathOrUrl)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        if (File.Exists(pathOrUrl))
        {
            return await GetPackageFromFile(pathOrUrl);
        }
        else if (Uri.TryCreate(pathOrUrl, UriKind.RelativeOrAbsolute, out _))
        {
            return await GetPackageFromUrlAsync(pathOrUrl);
        }
        else
        {
            return null;
        }
    }

    public async Task<ZipArchive?> GetPackageFromUrlAsync(string url)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
        {
            try
            {
                var webClient = new HttpClient();
                var packageStream = await webClient.GetStreamAsync(uri, _cancellationToken);
                return new ZipArchive(packageStream);
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Log.LogWarning($"Package not found (404): {url}");
                    return null;
                }
                else
                {
                    Log.LogWarning($"Failed to download package from {url}: {ex.Message}");
                    return null;
                }
            }
        }
        else
        {
            Log.LogWarning($"Invalid URL: {url}");
            return null;
        }
    }

    public async Task<ZipArchive?> GetPackageFromFile(string pathToPackage)
    {
        _cancellationToken.ThrowIfCancellationRequested();
        if (File.Exists(pathToPackage))
        {
            var packageStream = await File.ReadAllBytesAsync(pathToPackage);
            return new ZipArchive(new MemoryStream(packageStream));
        }

        Log.LogWarning($"Package not found: {pathToPackage}");
        return null;
    }

    public void Cancel()
    {
        _cts.Cancel();
    }
}
