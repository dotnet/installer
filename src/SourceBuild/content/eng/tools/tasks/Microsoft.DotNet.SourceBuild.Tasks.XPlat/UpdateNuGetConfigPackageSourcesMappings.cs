// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.DotNet.Build.Tasks
{
    /*
     * This task updates the package source mappings in the NuGet.Config using the following logic:
     * Add all packages from current source-build sources, i.e. source-built-*, reference-packages.
     * For previously source-built sources (PSB), add only the packages that do not exist in any of the current source-built sources.
     * Also add PSB packages if that package version does not exist in current package sources.
     * In offline build, remove all existing package source mappings for online sources.
     * In online build, filter existing package source mappings to remove anything that exists in any source-build source.
     * In online build, if NuGet.config didn't have any mappings, add default "*" pattern for all online sources.
     */
    public class UpdateNuGetConfigPackageSourcesMappings : Task
    {
        [Required]
        public string NuGetConfigFile { get; set; }

        /// <summary>
        /// Whether to work in offline mode (remove all internet sources) or online mode (remove only authenticated sources)
        /// </summary>
        public bool BuildWithOnlineFeeds { get; set; }

        /// <summary>
        /// A list of all source-build specific NuGet sources.
        /// </summary>
        public string[] SourceBuildSources { get; set; }

        [Required]
        public string SbrpRepoSrcPath { get; set; }

        private const string SbrpCacheSourceName = "source-build-reference-package-cache";

        public override bool Execute()
        {
            string xml = File.ReadAllText(NuGetConfigFile);
            string newLineChars = FileUtilities.DetectNewLineChars(xml);
            XDocument document = XDocument.Parse(xml);
            XElement pkgSourcesElement = document.Root.Descendants().FirstOrDefault(e => e.Name == "packageSources");
            if (pkgSourcesElement == null)
            {
                Log.LogMessage(MessageImportance.Low, "Package sources are missing.");

                return true;
            }

            XElement pkgSrcMappingElement = document.Root.Descendants().FirstOrDefault(e => e.Name == "packageSourceMapping");
            if (pkgSrcMappingElement == null)
            {
                pkgSrcMappingElement = new XElement("packageSourceMapping");
                document.Root.Add(pkgSrcMappingElement);
            }

            var allSourcesPackages = new Dictionary<string, List<string>>();
            var currentPackages = new Dictionary<string, List<string>>();
            var referencePackages = new Dictionary<string, List<string>>();
            var previouslyBuiltPackages = new Dictionary<string, List<string>>();
            var oldSourceMappingPatterns = new Dictionary<string, List<string>>();

            DiscoverPackagesFromAllSourceBuildSources(pkgSourcesElement, allSourcesPackages, currentPackages, referencePackages, previouslyBuiltPackages);

            // Discover all SBRP packages if source-build-reference-package-cache source is present in NuGet.config
            XElement sbrpCacheSourceElement = pkgSourcesElement.Descendants().FirstOrDefault(e => e.Name == "add" && e.Attribute("key").Value == SbrpCacheSourceName);
            if (sbrpCacheSourceElement != null)
            {
                DiscoverPackagesFromSbrpCacheSource(pkgSourcesElement, allSourcesPackages, referencePackages);
            }

            // If building online, enumerate any existing package source mappings and filter
            // to remove packages that are present in any local source-build source
            if (BuildWithOnlineFeeds && pkgSrcMappingElement != null)
            {
                GetExistingFilteredSourceMappings(pkgSrcMappingElement, currentPackages, referencePackages, previouslyBuiltPackages, oldSourceMappingPatterns);
            }

            // Remove all packageSourceMappings
            pkgSrcMappingElement.ReplaceNodes(new XElement("clear"));

            XElement pkgSrcMappingClearElement = pkgSrcMappingElement.Descendants().FirstOrDefault(e => e.Name == "clear");

            // When building online add the filtered mappings from original online sources.
            // If there are none, add default mappings for all online sources.
            if (BuildWithOnlineFeeds)
            {
                if (oldSourceMappingPatterns.Count > 0)
                {
                    foreach (var entry in oldSourceMappingPatterns)
                    {
                        // Skip sources with zero package patterns
                        if (entry.Value != null)
                        {
                            pkgSrcMappingClearElement.AddAfterSelf(GetPackageMappingsElementForSource(entry.Key, entry.Value));
                        }
                    }
                }
                else
                {
                    foreach (XElement sourceElement in pkgSourcesElement.Descendants().Where(e => e.Name == "add" && !allSourcesPackages.Keys.Contains(e.Attribute("key").Value)))
                    {
                        pkgSrcMappingClearElement.AddAfterSelf(new XElement("packageSource", new XAttribute("key", sourceElement.Attribute("key").Value), new XElement("package", new XAttribute("pattern", "*"))));
                    }
                }
            }

            // Add package source mappings for local package sources
            foreach (string packageSource in allSourcesPackages.Keys)
            {
                // Skip sources with zero package patterns
                if (allSourcesPackages[packageSource] != null)
                {
                    pkgSrcMappingClearElement.AddAfterSelf(GetPackageMappingsElementForSource(packageSource, allSourcesPackages, currentPackages, previouslyBuiltPackages));
                }
            }

            using (var writer = XmlWriter.Create(NuGetConfigFile, new XmlWriterSettings { NewLineChars = newLineChars, Indent = true }))
            {
                document.Save(writer);
            }

            return true;
        }

        private void AddDefaultMappingsForOnlineSources(XElement pkgSrcMappingClearElement, XElement pkgSourcesElement)
        {
            throw new NotImplementedException();
        }

        private XElement GetPackageMappingsElementForSource(string key, List<string> value)
        {
            XElement pkgSrc = new XElement("packageSource", new XAttribute("key", key));
            foreach (string pattern in value)
            {
                pkgSrc.Add(new XElement("package", new XAttribute("pattern", pattern)));
            }

            return pkgSrc;
        }

        private XElement GetPackageMappingsElementForSource(string packageSource, Dictionary<string, List<string>> allSourcesPackages, Dictionary<string, List<string>> currentPackages, Dictionary<string, List<string>> previouslyBuiltPackages)
        {
            bool isCurrentSourceBuiltSource =
                packageSource.StartsWith("source-built-") ||
                packageSource.Equals(SbrpCacheSourceName) ||
                packageSource.Equals("reference-packages");

            XElement pkgSrc = new XElement("packageSource", new XAttribute("key", packageSource));
            foreach (string packagePattern in allSourcesPackages[packageSource])
            {
                // Add all packages from current source-built sources.
                // For previously source-built and prebuilt sources add only packages
                // where version does not exist in current source-built sources.
                if (isCurrentSourceBuiltSource || !currentPackages.ContainsKey(packagePattern))
                {
                    pkgSrc.Add(new XElement("package", new XAttribute("pattern", packagePattern)));
                }
                else
                {
                    foreach (string version in previouslyBuiltPackages[packagePattern])
                    {
                        if (!currentPackages[packagePattern].Contains(version))
                        {
                            pkgSrc.Add(new XElement("package", new XAttribute("pattern", packagePattern)));
                            break;
                        }
                    }
                }
            }

            return pkgSrc;
        }

        private void DiscoverPackagesFromAllSourceBuildSources(XElement pkgSourcesElement, Dictionary<string, List<string>> allSourcesPackages, Dictionary<string, List<string>> currentPackages, Dictionary<string, List<string>> referencePackages, Dictionary<string, List<string>> previouslyBuiltPackages)
        {
            foreach (string packageSource in SourceBuildSources)
            {
                XElement sourceElement = pkgSourcesElement.Descendants().FirstOrDefault(e => e.Name == "add" && e.Attribute("key").Value == packageSource);
                if (sourceElement == null)
                {
                    continue;
                }

                string path = sourceElement.Attribute("value").Value;
                if (!Directory.Exists(path))
                {
                    continue;
                }

                string[] packages = Directory.GetFiles(path, "*.nupkg", SearchOption.AllDirectories);
                foreach (string package in packages)
                {
                    NupkgInfo info = GetNupkgInfo(package);
                    string id = info.Id.ToLower();
                    string version = info.Version.ToLower();

                    // Add package with version to appropriate hashtable
                    if (packageSource.StartsWith("source-built-"))
                    {
                        AddToDictionary(currentPackages, id, version);
                    }
                    else if (packageSource.Equals("reference-packages"))
                    {
                        AddToDictionary(referencePackages, id, version);
                    }
                    else // previously built packages
                    {
                        AddToDictionary(previouslyBuiltPackages, id, version);
                    }

                    AddToDictionary(allSourcesPackages, packageSource, id);
                }
            }
        }

        private void DiscoverPackagesFromSbrpCacheSource(XElement pkgSourcesElement, Dictionary<string, List<string>> allSourcesPackages, Dictionary<string, List<string>> currentPackages)
        {
            // 'source-build-reference-package-cache' is a dynamic source, populated by SBRP build.
            // Discover all SBRP packages from checked in nuspec files.

            if (!Directory.Exists(SbrpRepoSrcPath))
            {
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, "SBRP repo root does not exist in expected path: {0}", SbrpRepoSrcPath));
            }

            string[] nuspecFiles = Directory.GetFiles(SbrpRepoSrcPath, "*.nuspec", SearchOption.AllDirectories);
            foreach (string nuspecFile in nuspecFiles)
            {
                try
                {
                    using Stream stream = File.OpenRead(nuspecFile);
                    NupkgInfo info = GetNupkgInfo(stream);
                    string id = info.Id.ToLower();
                    string version = info.Version.ToLower();

                    AddToDictionary(currentPackages, id, version);
                    AddToDictionary(allSourcesPackages, SbrpCacheSourceName, id);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, "Invalid nuspec file", nuspecFile), ex);
                }
            }
        }

        private void GetExistingFilteredSourceMappings(XElement pkgSrcMappingElement, Dictionary<string, List<string>> currentPackages, Dictionary<string, List<string>> referencePackages, Dictionary<string, List<string>> previouslyBuiltPackages, Dictionary<string, List<string>> oldSourceMappingPatterns)
        {
            foreach (XElement packageSource in pkgSrcMappingElement.Descendants().Where(e => e.Name == "packageSource"))
            {
                List<string> filteredPatterns = new List<string>();
                foreach (XElement package in packageSource.Descendants().Where(e => e.Name == "package"))
                {
                    string pattern = package.Attribute("pattern").Value.ToLower();
                    if (!currentPackages.ContainsKey(pattern) &&
                        !referencePackages.ContainsKey(pattern) &&
                        !previouslyBuiltPackages.ContainsKey(pattern))
                    {
                        filteredPatterns.Add(pattern);
                    }
                }

                oldSourceMappingPatterns.Add(packageSource.Attribute("key").Value, filteredPatterns);
            }
        }

        private void AddToDictionary(Dictionary<string, List<string>> dictionary, string key, string value)
        {
            if (dictionary.ContainsKey(key))
            {
                List<string> values = dictionary[key];
                if (!values.Contains(value))
                {
                    values.Add(value);
                }
            }
            else
            {
                dictionary.Add(key, [value]);
            }
        }

        /// <summary>
        /// Get nupkg info, id and version, from nupkg file.
        /// </summary>
        private NupkgInfo GetNupkgInfo(string path)
        {
            try
            {
                using Stream stream = File.OpenRead(path);
                ZipArchive zipArchive = new(stream, ZipArchiveMode.Read);
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    if (entry.Name.EndsWith(".nuspec"))
                    {
                        using Stream nuspecFileStream = entry.Open();
                        return GetNupkgInfo(nuspecFileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, "Invalid package", path), ex);
            }

            throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, "Did not extract nuspec file from package: {0}", path));
        }

        /// <summary>
        /// Get nupkg info, id and version, from nuspec stream.
        /// </summary>
        private NupkgInfo GetNupkgInfo(Stream nuspecFileStream)
        {
            XDocument doc = XDocument.Load(nuspecFileStream, LoadOptions.PreserveWhitespace);
            XElement metadataElement = doc.Descendants().First(c => c.Name.LocalName.ToString() == "metadata");
            return new NupkgInfo(
                    metadataElement.Descendants().First(c => c.Name.LocalName.ToString() == "id").Value,
                    metadataElement.Descendants().First(c => c.Name.LocalName.ToString() == "version").Value);
        }

        private class NupkgInfo
        {
            public NupkgInfo(string id, string version)
            {
                Id = id;
                Version = version;
            }

            public string Id { get; }
            public string Version { get; }
        }
    }
}
