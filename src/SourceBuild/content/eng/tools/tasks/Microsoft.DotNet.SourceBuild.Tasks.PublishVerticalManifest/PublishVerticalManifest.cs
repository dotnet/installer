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
        public ITaskItem[] AssetManifestPath { get; set; }
        [Required]
        public string VerticalAssetManifestOutputPath { get; set; }

        public override bool Execute()
        {
            var assetManifestXmls = AssetManifestPath.Select(xmlPath => XDocument.Load(xmlPath.ItemSpec)).ToList();

            VerifyAssetManifests(assetManifestXmls);

            var rootName = assetManifestXmls.First().Root.Name;
            var rootAttributes = assetManifestXmls.First().Root.Attributes();
            var packageElements = new List<XElement>();
            var blobElements = new List<XElement>();

            foreach (var assetManifestXml in assetManifestXmls)
            {
                packageElements.AddRange(assetManifestXml.Descendants("Package"));
                blobElements.AddRange(assetManifestXml.Descendants("Blob"));
            }
            
            packageElements = packageElements.OrderBy(packageElement => packageElement.Attribute("Id").Value).ToList();
            blobElements = blobElements.OrderBy(blobElement => blobElement.Attribute("Id").Value).ToList();

            var verticalManifest = new XDocument(new XElement(rootName, rootAttributes, packageElements, blobElements));

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

            if (assetManifestXmls.Skip(1).Any(assetManifestXml => 
                    !assetManifestXml.Root.Attributes().All(attribute => 
                        rootAttributes.Contains(attribute.ToString()))))
            {
                throw new ArgumentException("The asset manifests do not have the same root attributes.");
            }
        }
    }
}