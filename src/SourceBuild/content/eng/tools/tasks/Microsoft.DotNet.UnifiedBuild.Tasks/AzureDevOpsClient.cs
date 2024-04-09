// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.DotNet.UnifiedBuild.Tasks;

public class AzureDevOpsClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly TaskLoggingHelper _logger;

    private readonly string _azureDevOpsBaseUri = "https://dev.azure.com";
    private readonly string _azureDevOpsApiVersion = "7.1-preview.5";
    // download in 100 MB chunks
    private readonly int _downloadBufferSize = 1024 * 1024 * 100;
    private readonly int _httpTimeoutSeconds = 300;
    private readonly string _assetsFolderName = "assets"
    private readonly string _packagesFolderName = "packages"

    public AzureDevOpsClient(
        string azureDevOpsToken,
        string azureDevOpsOrg,
        string azureDevOpsProject,
        TaskLoggingHelper logger)
    {

        _logger = logger;

        _httpClient = new(new HttpClientHandler { CheckCertificateRevocationList = true });

        _httpClient.BaseAddress = new Uri($"{_azureDevOpsBaseUri}/{azureDevOpsOrg}/{azureDevOpsProject}/_apis/");

        _httpClient.Timeout = TimeSpan.FromSeconds(_httpTimeoutSeconds);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($":{azureDevOpsToken}"))
        );
    }

    /// <summary>
    /// Downloads specified packages and symbols from a specific build artifact and stores them in an output folder, either flat or with the same relative path as in the artifact.
    /// </summary>
    public async Task DownloadArtifactFiles(
        string buildId,
        string artifactName,
        List<string> packageNames,
        List<string> symbolNames,
        string downloadFolder,
        string outputFolder,
        bool flatCopy)
    {
        string downloadPath = Path.Combine(downloadFolder, "artifact.zip");
        string extractPath = Path.Combine(downloadFolder, "extracted");
        string extractedAssetsPath = Path.Combine(extractPath, artifactName, _assetsFolderName);
        string extractedPackagesPath = Path.Combine(extractPath, artifactName, _packagesFolderName);

        try{
            await DownloadArtifactZip(buildId, artifactName, downloadPath);

            ZipFile.ExtractToDirectory(downloadPath, extractPath);

            // assets include all kinds of files, so just look at all files
            List<string> sourceSymbolPaths = Directory.GetFiles(extractedAssetsPath, "*", SearchOption.AllDirectories).ToList();
            List<string> sourcePackagePaths = Directory.GetFiles(extractedPackagesPath, "*.nupkg", SearchOption.AllDirectories).ToList();
        
            string packageOutputPath = Path.Combine(outputFolder, _packagesFolderName);
            string symbolOutputPath = Path.Combine(outputFolder, _assetsFolderName);

            if (!Directory.Exists(packageOutputPath))
            {
                Directory.CreateDirectory(packageOutputPath);
            }
            if (!Directory.Exists(symbolOutputPath))
            {
                Directory.CreateDirectory(symbolOutputPath);
            }

            CopyFiles(packageNames, sourcePackagePaths, extractedPackagesPath, packageOutputPath, flatCopy);
            CopyFiles(symbolNames, sourceSymbolPaths, extractedAssetsPath, symbolOutputPath, flatCopy);
        }
        finally
        {
            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }
            if (File.Exists(downloadPath))
            {
                File.Delete(downloadPath);
            }
        }
    }

    private async Task DownloadArtifactZip(string buildId, string artifactName, string downloadPath) 
    {
        string relativeUrl = $"build/builds/{buildId}/artifacts?artifactName={artifactName}&api-version={_azureDevOpsApiVersion}";
        _logger.LogMessage(MessageImportance.High, $"Downloading artifact information from {relativeUrl}");
        HttpResponseMessage response = await _httpClient.GetAsync(relativeUrl);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to download artifact information. Status code: {response.StatusCode} Reason: {response.ReasonPhrase}");
        }

        AzureDevOpsArtifactInformation azdoArtifactInformation = await response.Content.ReadFromJsonAsync<AzureDevOpsArtifactInformation>()
            ?? throw new ArgumentException($"Couldn't parse AzDo response {response.Content} to {nameof(AzureDevOpsArtifactInformation)}");

        _logger.LogMessage(MessageImportance.High, $"Downloading artifact zip from {azdoArtifactInformation.Resource.DownloadUrl}");
        response = await _httpClient.GetAsync(azdoArtifactInformation.Resource.DownloadUrl);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to download artifact zip. Status code: {response.StatusCode} Reason: {response.ReasonPhrase}");
        }

        using Stream stream = await response.Content.ReadAsStreamAsync();
        using FileStream fileStream = File.Create(downloadPath);

        byte[] buffer = new byte[_downloadBufferSize];
        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead);
        }
    }

    private void CopyFiles(List<string> fileNamesToCopy, List<string> sourceFiles, string sourceDirectory, string destinationFolder, bool flatCopy)
    {
        foreach (string file in fileNamesToCopy)
        {
            string sourceFilePath = sourceFiles.FirstOrDefault(f => f.Contains(file))
                ?? throw new ArgumentException($"File {file} not found in source files.");

            string destinationFilePath = flatCopy
                ? GetFlatDestinationPath(sourceFilePath, destinationFolder)
                : GetRelativeDestinationPath(sourceFilePath, sourceDirectory, destinationFolder);

            if (!Directory.Exists(Path.GetDirectoryName(destinationFilePath))) 
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath)!);
            }

            if (File.Exists(destinationFilePath))
            {
                _logger.LogWarning($"File {destinationFilePath} already exists. Overwriting.");
            }

            File.Copy(sourceFilePath, destinationFilePath, true);
        }
    }

    private string GetRelativeDestinationPath(string sourceFilePath, string sourceDirectory, string destinationFolder)
    {
        string relativeFilePath = string.Empty;

        // We need to keep the relative file path the same as in the artifacts
        // For example d:/extracted/artifacts/packages/Release/Shipping/emsdk/package.nupkg
        // should be copied to d:/output/packages/Release/Shipping/emsdk/package.nupkg
        string helpFilePath = sourceFilePath;
        while (helpFilePath != sourceDirectory)
        {
            relativeFilePath = Path.Combine(Path.GetFileName(helpFilePath)!, relativeFilePath);
            helpFilePath = Path.GetDirectoryName(helpFilePath)!;
        }

        return Path.Combine(destinationFolder, relativeFilePath);;
    }

    private string GetFlatDestinationPath(string sourceFilePath, string destinationFolder)
    {
        return Path.Combine(destinationFolder, Path.GetFileName(sourceFilePath));
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}