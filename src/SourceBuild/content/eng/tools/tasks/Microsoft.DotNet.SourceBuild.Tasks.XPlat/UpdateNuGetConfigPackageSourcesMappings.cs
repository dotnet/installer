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
     * This task updates the package source mappings in the NuGet.Config.
     * If package source mappings are used, source-build packages sources will be added with the cumulative package patterns
     * for all of the existing package sources. When building offline, the existing package source mappings will be removed;
     * otherwise they will be preserved after the source-build sources.
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

        public string VmrRoot { get; set; }

        private string SBRPCacheSourceName = "source-build-reference-package-cache";

        public override bool Execute()
        {
            string xml = File.ReadAllText(NuGetConfigFile);
            string newLineChars = FileUtilities.DetectNewLineChars(xml);
            XDocument document = XDocument.Parse(xml);
            XElement pkgSrcMappingElement = document.Root.Descendants().FirstOrDefault(e => e.Name == "packageSourceMapping");
            XElement pkgSourcesElement = document.Root.Descendants().FirstOrDefault(e => e.Name == "packageSources");
            if (pkgSourcesElement == null)
            {
                return true;
            }

            if (pkgSrcMappingElement == null)
            {
                pkgSrcMappingElement = new XElement("packageSourceMapping");
                document.Root.Add(pkgSrcMappingElement);
            }

            Hashtable allSourcesPackages = new Hashtable();
            Hashtable currentPackages = new Hashtable();
            Hashtable referencePackages = new Hashtable();
            Hashtable previouslyBuiltPackages = new Hashtable();
            Hashtable oldSourceMappingPatterns = new Hashtable();

            DiscoverPackagesFromAllSourceBuildSources(pkgSourcesElement, allSourcesPackages, currentPackages, referencePackages, previouslyBuiltPackages);

            // Discover all SBRP packages if source-build-reference-package-cache source is present in NuGet.config
            XElement sbrpCacheSourceElement = pkgSourcesElement.Descendants().FirstOrDefault(e => e.Name == "add" && e.Attribute("key").Value == SBRPCacheSourceName);
            if (sbrpCacheSourceElement != null)
            {
                DiscoverPackagesFromSBRPCacheSource(pkgSourcesElement, allSourcesPackages, referencePackages);
            }

            // If building online, enumerate any existing package source mappings and filter
            // to remove packages that are present in any local source-build source
            if (BuildWithOnlineFeeds && pkgSrcMappingElement != null)
            {
                foreach (XElement packageSource in pkgSrcMappingElement.Descendants().Where(e => e.Name == "packageSource"))
                {
                    List<string> filteredPatterns = new List<string>();
                    foreach (XElement package in packageSource.Descendants().Where(e => e.Name == "package"))
                    {
                        string pattern = package.Attribute("pattern").Value.ToLower();
                        if (!currentPackages.Contains(pattern) &&
                            !referencePackages.Contains(pattern) &&
                            !previouslyBuiltPackages.Contains(pattern))
                        {
                            filteredPatterns.Add(pattern);
                        }
                    }

                    oldSourceMappingPatterns.Add(packageSource.Attribute("key").Value, filteredPatterns);
                }
            }

            // Remove all packageSourceMappings
            pkgSrcMappingElement.ReplaceNodes(new XElement("clear"));

            XElement pkgSrcMappingClearElement = pkgSrcMappingElement.Descendants().FirstOrDefault(e => e.Name == "clear");

            // When building online add the filtered mappings from original online sources.
            if (BuildWithOnlineFeeds)
            {
                foreach (DictionaryEntry entry in oldSourceMappingPatterns)
                {
                    // Skip sources with zero package patterns
                    if (entry.Value == null)
                    {
                        continue;
                    }

                    XElement pkgSrc = new XElement("packageSource", new XAttribute("key", entry.Key));
                    foreach (string pattern in (List<string>)entry.Value)
                    {
                        pkgSrc.Add(new XElement("package", new XAttribute("pattern", pattern)));
                    }

                    pkgSrcMappingClearElement.AddAfterSelf(pkgSrc);
                }
            }

            // Add package source mappings for local package sources
            foreach (string packageSource in allSourcesPackages.Keys)
            {
                // Skip sources with zero package patterns
                if (allSourcesPackages[packageSource] == null)
                {
                    continue;
                }

                bool isCurrentSourceBuiltSource =
                    packageSource.StartsWith("source-built-") ||
                    packageSource.Equals(SBRPCacheSourceName) ||
                    packageSource.Equals("reference-packages");

                XElement pkgSrc = new XElement("packageSource", new XAttribute("key", packageSource));
                foreach (string packagePattern in (List<string>)allSourcesPackages[packageSource])
                {
                    // Add all packages from current source-built sources.
                    // For previously source-built and prebuilt sources add only packages
                    // where version does not exist in current source-built sources.
                    if (isCurrentSourceBuiltSource || !currentPackages.Contains(packagePattern))
                    {
                        pkgSrc.Add(new XElement("package", new XAttribute("pattern", packagePattern)));
                    }
                    else
                    {
                        foreach (string version in (List<string>)previouslyBuiltPackages[packagePattern])
                        {
                            if (!((List<string>)currentPackages[packagePattern]).Contains(version))
                            {
                                pkgSrc.Add(new XElement("package", new XAttribute("pattern", packagePattern)));
                                break;
                            }
                        }
                    }
                }

                pkgSrcMappingClearElement.AddAfterSelf(pkgSrc);
            }

            using (var writer = XmlWriter.Create(NuGetConfigFile, new XmlWriterSettings { NewLineChars = newLineChars, Indent = true }))
            {
                document.Save(writer);
            }

            return true;
        }

        private void DiscoverPackagesFromAllSourceBuildSources(XElement pkgSourcesElement, Hashtable allSourcesPackages, Hashtable currentPackages, Hashtable referencePackages, Hashtable previouslyBuiltPackages)
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
                        AddToHashtable(currentPackages, id, version);
                    }
                    else if (packageSource.Equals("reference-packages"))
                    {
                        AddToHashtable(referencePackages, id, version);
                    }
                    else // previously built packages
                    {
                        AddToHashtable(previouslyBuiltPackages, id, version);
                    }

                    AddToHashtable(allSourcesPackages, packageSource, id);
                }
            }
        }

        private void DiscoverPackagesFromSBRPCacheSource(XElement pkgSourcesElement, Hashtable allSourcesPackages, Hashtable currentPackages)
        {
            // source-build-reference-package-cache is a dynamic source, populated by SBRP build,
            // Discover all SBRP packages from checked in nuspec files.

            if (string.IsNullOrEmpty(VmrRoot))
            {
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, "VmrRoot is not set - cannot determine SBRP packages."));
            }

            string sbrpRepoRoot = Path.Combine(VmrRoot, "src", "source-build-reference-packages");
            if (!Directory.Exists(sbrpRepoRoot))
            {
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, "SBRP repo root does not exist in expected path: {0}", sbrpRepoRoot));
            }

            string[] nuspecFiles = Directory.GetFiles(sbrpRepoRoot, "*.nuspec", SearchOption.AllDirectories);
            foreach (string nuspecFile in nuspecFiles)
            {
                try
                {
                    using Stream stream = File.OpenRead(nuspecFile);
                    NupkgInfo info = GetNupkgInfo(stream);
                    string id = info.Id.ToLower();
                    string version = info.Version.ToLower();

                    AddToHashtable(currentPackages, id, version);
                    AddToHashtable(allSourcesPackages, SBRPCacheSourceName, id);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, "Invalid nuspec file", nuspecFile), ex);
                }
            }
        }

        private void AddToHashtable(Hashtable hashtable, string key, string value)
        {
            if (hashtable.ContainsKey(key))
            {
                List<string> values = (List<string>)hashtable[key];
                if (!values.Contains(value))
                {
                    values.Add(value);
                }
            }
            else
            {
                hashtable.Add(key, new List<string> { value });
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

                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, "Did not extract nuspec file from package: {0}", path));
            }
            catch (Exception ex)
            {
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, "Invalid package", path), ex);
            }
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
    }

    public class NupkgInfo
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
