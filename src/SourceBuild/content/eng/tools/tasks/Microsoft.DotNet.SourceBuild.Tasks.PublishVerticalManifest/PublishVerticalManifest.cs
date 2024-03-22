// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.DotNet.SourceBuild.Tasks.PublishVerticalManifest
{
    public class PublishVerticalManifest : Task
    {
        [Required]
        public ITaskItem[] AssetManifest { get; set; }
        [Required]
        public string VerticalAssetManifestOutputPath { get; set; }
        [Required]
        public string VmrBuildNumber { get; set; }

        private static readonly string _buildIdAttribute = "BuildId";
        private static readonly string _azureDevOpsBuildNumberAttribute = "AzureDevOpsBuildNumber";
        private static readonly string[] _ignoredAttributes = new string[] { _buildIdAttribute, _azureDevOpsBuildNumberAttribute, "IsReleaseOnlyPackageVersion" };

        public override bool Execute()
        {
            var assetManifestXmls = AssetManifest.Select(xmlPath => XDocument.Load(xmlPath.ItemSpec)).ToList();

            VerifyAssetManifests(assetManifestXmls);

            var verticalManifestRoot = assetManifestXmls.First().Root;

            // Set the BuildId and AzureDevOpsBuildNumber attributes to the value of VmrBuildNumber
            verticalManifestRoot.SetAttributeValue(_buildIdAttribute, VmrBuildNumber);
            verticalManifestRoot.SetAttributeValue(_azureDevOpsBuildNumberAttribute, VmrBuildNumber);

            var packageElements = new List<XElement>();
            var blobElements = new List<XElement>();

            foreach (var assetManifestXml in assetManifestXmls)
            {
                packageElements.AddRange(assetManifestXml.Descendants("Package"));
                blobElements.AddRange(assetManifestXml.Descendants("Blob"));
            }
            
            packageElements = packageElements.OrderBy(packageElement => packageElement.Attribute("Id").Value).ToList();
            blobElements = blobElements.OrderBy(blobElement => blobElement.Attribute("Id").Value).ToList();

            var verticalManifest = new XDocument(new XElement(verticalManifestRoot.Name, verticalManifestRoot.Attributes(), packageElements, blobElements));

            File.WriteAllText(VerticalAssetManifestOutputPath, verticalManifest.ToString());

            return true;
        }

        private static void VerifyAssetManifests(List<XDocument> assetManifestXmls)
        {
            if (assetManifestXmls.Count == 0)
            {
                throw new ArgumentException("No asset manifests were provided.");
            }

            var rootAttributes = assetManifestXmls
                .First()
                .Root
                .Attributes()
                .Select(attribute => attribute.ToString())
                .ToHashSet();

            if (assetManifestXmls.Skip(1).Any(manifest => manifest.Root.Attributes().Count() != rootAttributes.Count))
            {
                throw new ArgumentException("The asset manifests do not have the same number of root attributes.");
            }

            if (assetManifestXmls.Skip(1).Any(assetManifestXml => 
                    !assetManifestXml.Root.Attributes().Select(attribute => attribute.ToString()).All(attribute =>
                        // Ignore BuildId and AzureDevOpsBuildNumber attributes, they're different for different repos, 
                        // TODO this should be fixed with https://github.com/dotnet/source-build/issues/3934
                        _ignoredAttributes.Any(ignoredAttribute => attribute.StartsWith(ignoredAttribute)) || rootAttributes.Contains(attribute))))
            {
                throw new ArgumentException("The asset manifests do not have the same root attributes.");
            }
        }
    }
}