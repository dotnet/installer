// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Microsoft.DotNet.UnifiedBuild.Tasks
{
    public static class Utilities
    {
        public const string CR = "\r";
        public const string CRLF = "\r\n";
        public const string LF = "\n";

        public static string DetectNewLineChars(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.Contains(CRLF))
            {
                return CRLF;
            }
            else if (source.Contains(LF))
            {
                return LF;
            }
            else if (source.Contains(CR))
            {
                return CR;
            }
            else
            {
                throw new ArgumentException("Unsupported new line characters", nameof(source));
            }
        }

        public static string NormalizeNewLineChars(string source, string newLineChars)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            source = source.Replace(CRLF, LF).Replace(CR, LF);
            if (newLineChars == CRLF)
            {
                source = source.Replace(LF, CRLF);
            }
            else if (newLineChars == CR)
            {
                source = source.Replace(LF, CR);
            }
            else if (newLineChars != LF)
            {
                throw new ArgumentException("Unsupported new line characters", nameof(newLineChars));
            }

            // Ensure trailing new line on linux.
            if (newLineChars == LF && !source.EndsWith(newLineChars))
            {
                source += newLineChars;
            }

            return source;
        }

        // Checks if a file in Sdk layout requires an external Pdb.
        public static bool FileInSdkLayoutRequiresAPdb(string file, out string guid)
        {
            guid = string.Empty;

            // Files under packs/ are used for build only, no need for Pdbs
            return !file.Contains(Path.DirectorySeparatorChar + "packs" + Path.DirectorySeparatorChar) ?
                FileHasCompanionPdbInfo(file, out guid) :
                false;
        }

        // Checks if a file has debug data indicating an external companion Pdb.
        public static bool FileHasCompanionPdbInfo(string file, out string guid)
        {
            guid = string.Empty;

            if (file.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase) &&
                !file.EndsWith(".resources.dll", StringComparison.InvariantCultureIgnoreCase))
            {
                using var pdbStream = File.OpenRead(file);
                using var peReader = new PEReader(pdbStream);
                try
                {
                    // Check if pdb is embedded
                    if (peReader.ReadDebugDirectory().Any(entry => entry.Type == DebugDirectoryEntryType.EmbeddedPortablePdb))
                    {
                        return false;
                    }

                    var debugDirectory = peReader.ReadDebugDirectory().First(entry => entry.Type == DebugDirectoryEntryType.CodeView);
                    var codeViewData = peReader.ReadCodeViewDebugDirectoryData(debugDirectory);
                    guid = $"{codeViewData.Guid.ToString("N").Replace("-", string.Empty)}";
                }
                catch (Exception e) when (e is BadImageFormatException || e is InvalidOperationException)
                {
                    // Ignore binaries without debug info
                    return false;
                }
            }

            return guid != string.Empty;
        }
    }
}
