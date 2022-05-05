// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.DotNet.SourceBuild.Tasks;
using NuGet.Packaging;
using NuGet.Packaging.Core;


namespace Microsoft.DotNet.Build.Tasks
{
    /*
     * This task adds a source to a well-formed NuGet.Config file. If a source with `SourceName` is already present, then
     * the path of the source is changed. Otherwise, the source is added as the first source in the list, after any clear
     * elements (if present).
     */
    public class AddSourceToNuGetConfig : Task
    {
        [Required]
        public string NuGetConfigFile { get; set; }

        [Required]
        public string SourceName { get; set; }

        [Required]
        public string SourcePath { get; set; }

        public override bool Execute()
        {
            string xml = File.ReadAllText(NuGetConfigFile);
            string newLineChars = FileUtilities.DetectNewLineChars(xml);
            XDocument d = XDocument.Parse(xml);
            XElement packageSourcesElement = d.Root.Descendants().First(e => e.Name == "packageSources");
            XElement toAdd = new XElement("add", new XAttribute("key", SourceName), new XAttribute("value", SourcePath));
            XElement clearTag = new XElement("clear");

            XElement exisitingSourceBuildElement = packageSourcesElement.Descendants().FirstOrDefault(e => e.Name == "add" && e.Attribute(XName.Get("key")).Value == SourceName);
            XElement lastClearElement = packageSourcesElement.Descendants().LastOrDefault(e => e.Name == "clear");
            XElement packageSourceMappingElement = d.Root.Descendants().FirstOrDefault(e => e.Name == "packageSourceMapping");

            if (exisitingSourceBuildElement != null)
            {
                exisitingSourceBuildElement.ReplaceWith(toAdd);
            }
            else if (lastClearElement != null)
            {
                lastClearElement.AddAfterSelf(toAdd);
            }
            else
            {
                packageSourcesElement.AddFirst(toAdd);
                packageSourcesElement.AddFirst(clearTag);
            }

            if (packageSourceMappingElement != null)
            {
                // Package sources look like:
                // <packageSource key="contoso.com">
                //      <package pattern="Contoso.*" />
                //      <package pattern="NuGet.Common" />
                // </packageSource>
                // https://docs.microsoft.com/en-us/nuget/consume-packages/package-source-mapping
                XElement pkgSrc = new XElement("packageSource", new XAttribute("key", SourceName));
                try
                {
                    // We use the complete name for each available package so our sources are considered
                    // the most specific and override any other sources that were not removed if the package
                    // is available on multiple feeds.
                    foreach (var p in Directory.EnumerateFiles(SourcePath, "*.nupkg"))
                    {
                        PackageIdentity pkgId = ReadNuGetPackageInfos.ReadIdentity(p);
                        pkgSrc.Add(new XElement("package", new XAttribute("pattern", pkgId.Id)));
                    }
                    packageSourceMappingElement.Add(pkgSrc);
                }
                catch (Exception e)
                {
                    Log.LogWarning($"Couldn't add package source mapping: {e.ToString()}");
                }
            }

            using (var w = XmlWriter.Create(NuGetConfigFile, new XmlWriterSettings { NewLineChars = newLineChars, Indent = true }))
            {
                d.Save(w);
            }

            return true;
        }
    }
}
