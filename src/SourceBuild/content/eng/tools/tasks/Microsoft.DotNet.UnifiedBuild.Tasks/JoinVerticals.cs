// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.DotNet.UnifiedBuild.Tasks;

public class JoinVerticals : Microsoft.Build.Utilities.Task
{
    /// <summary>
    /// Paths to Verticals Manifests
    /// </summary>
    [Required]
    public required ITaskItem[] VerticalManifest { get; init; }

    /// <summary>
    /// Optional subset of the Verticals we want to join, separated by ;
    /// </summary>
    /// <example>Windows_x64;Windows_x86</example>
    public string? VerticalSubSet { get; set; }

    /// <summary>
    /// Name of the main vertical that we'll take all artifacts from
    /// </summary>
    [Required]
    public required string MainVertical { get; init; }

    /// <summary>
    /// Azure DevOps build id
    /// </summary>
    [Required]
    public required string BuildId { get; init; }

    /// <summary>
    /// Azure DevOps token, required scopes: "Build (read)"
    /// </summary>
    [Required]
    public required string AzureDevOpsToken { get; init; }

    /// <summary>
    /// Azure DevOps organization
    /// </summary>
    [Required]
    public required string AzureDevOpsOrg { get; init; }

    /// <summary>
    /// Azure DevOps project
    /// </summary>
    [Required]
    public required string AzureDevOpsProject { get; init; }

    /// <summary>
    /// Temporary folder where artifacts will be downloaded and extracted
    /// </summary>
    [Required]
    public required string TmpFolder { get; init; }

    /// <summary>
    /// Folder where packages and assets will be stored
    /// </summary>
    [Required]
    public required string OutputFolder { get; init; }

    /// <summary>
    /// If true, the task will copy all packages to the same output folder, they won'y be separated by shipping/nonshipping or repo.
    /// Set to true for the final join, false for all other joins.
    /// </summary>
    public required bool FlatCopy { get; init; }

    private const string _packageElementName = "Package";
    private const string _blobElementName = "Blob";
    private const string _idAttribute = "Id";
    private const string _verticalNameAttribute = "VerticalName";
    private const string _artifactNameSuffix = "_Artifacts";

    public override bool Execute()
    {
        ExecuteAsync().Wait();
        return !Log.HasLoggedErrors;
    }

    private async Task ExecuteAsync()
    {
        List<XDocument> verticalManifests = VerticalManifest.Select(xmlPath => XDocument.Load(xmlPath.ItemSpec)).ToList();

        if (!string.IsNullOrEmpty(VerticalSubSet))
        {
            var verticalSubSet = VerticalSubSet.Split(';');
            verticalManifests = verticalManifests.Where(manifest => verticalSubSet.Contains(GetRequiredRootAttribute(manifest, _verticalNameAttribute))).ToList();
        }

        // Find the main manifest, and put it on the beginning of the list
        XDocument mainVerticalManifest = verticalManifests.FirstOrDefault(manifest => GetRequiredRootAttribute(manifest, _verticalNameAttribute) == MainVertical)
            ?? throw new ArgumentException($"Couldn't find main vertical manifest {MainVertical} in vertical manifest list");
        verticalManifests = verticalManifests.Where(manifest => GetRequiredRootAttribute(manifest, _verticalNameAttribute) != MainVertical).ToList();
        verticalManifests.Insert(0, mainVerticalManifest);

        Dictionary<string, XElement> packageElements = new();
        Dictionary<string, XElement> blobElements = new();

        using AzureDevOpsClient azureDevOpsClient = new(AzureDevOpsToken, AzureDevOpsOrg, AzureDevOpsProject, Log);

        foreach(XDocument verticalManifest in verticalManifests)
        {
            string verticalName = GetRequiredRootAttribute(verticalManifest, _verticalNameAttribute);

            List<string> addedPackageIds = AddMissingElements(packageElements, verticalManifest, _packageElementName);
            List<string> addedBlobIds = AddMissingElements(blobElements, verticalManifest, _blobElementName);

            // Blob ids also contain paths, we don't need those, just the names
            // In the future, we might want to optimize this part in different ways
            // - we could run multiple downloads in parallel, while being mindful of the space
            // - if the number of packages or assets is small, we could download them one by one, instead of the whole artifact and cherry picking
            if (addedPackageIds.Count() > 0 && addedBlobIds.Count() > 0)
            {
                await azureDevOpsClient.DownloadArtifactFiles(
                    BuildId,
                    $"{verticalName}{_artifactNameSuffix}",
                    addedPackageIds,
                    addedBlobIds.Select(blob => Path.GetFileName(blob)).ToList(),
                    TmpFolder,
                    OutputFolder,
                    FlatCopy);
            }
        }

        XElement mainManifestRoot = verticalManifests.First().Root 
            ?? throw new ArgumentException("The root element of the vertical manifest is null.");
        mainManifestRoot.Attribute(_verticalNameAttribute)!.Remove();
        
        string manifestOutputPath = Path.Combine(OutputFolder, "MergedManifest.xml");
        XDocument mergedManifest = new(new XElement(
            mainManifestRoot.Name,
            mainManifestRoot.Attributes(),
            packageElements.Values.OrderBy(elem => elem.Attribute(_idAttribute)?.Value),
            blobElements.Values.OrderBy(elem => elem.Attribute(_idAttribute)?.Value)));
        File.WriteAllText(manifestOutputPath, mergedManifest.ToString());
    }

    private List<string> AddMissingElements(Dictionary<string, XElement> elements, XDocument document, string elementName) 
    {
        List<string> addedIds = new();
        
        string verticalName = document.Root!.Attribute(_verticalNameAttribute)!.Value;

        foreach (XElement element in document.Descendants(elementName)) 
        {
            string elementId = element.Attribute(_idAttribute)?.Value 
                ?? throw new ArgumentException($"Required attribute '{_idAttribute}' not found in {elementName} element.");
            if (!elements.ContainsKey(elementId))
            {
                elements.Add(elementId, element);
                addedIds.Add(elementId);
                Log.LogMessage(MessageImportance.High, $"Taking {elementName} '{elementId}' from '{verticalName}'");
            }
        }

        return addedIds;
    }

    private static string GetRequiredRootAttribute(XDocument document, string attributeName)
    {
        return document.Root?.Attribute(attributeName)?.Value 
            ?? throw new ArgumentException($"Required attribute '{attributeName}' not found in root element.");
    }
}