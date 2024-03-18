// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.DotNet.SourceBuild.Tasks.PublishVerticalManifest
{
    public class PublishVerticalManifest : Task
    {
        [Required]
        public string AssetManifestsLocalStorageDir { get; set; }
        [Required]
        public string VerticalAssetManifestLocalPath { get; set; }

        public override bool Execute()
        {
            var assetManifestXmls = Directory.GetFiles(AssetManifestsLocalStorageDir, "*.xml", SearchOption.AllDirectories)
                .Select(xmlPath => XDocument.Load(xmlPath)).ToList();

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

            File.WriteAllText(VerticalAssetManifestLocalPath, verticalManifest.ToString());

            return true;
        }
    }
}