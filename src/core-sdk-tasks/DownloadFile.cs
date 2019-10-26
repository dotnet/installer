// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Net.Http;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.DotNet.Cli.Build
{
    public class DownloadFile : Task
    {
        [Required]
        public string Uri { get; set; }

        /// <summary>
        /// If this field is set and the task fail to download the file from `Uri` it will try
        /// to download the file from `PrivateUri`.
        /// </summary>
        public string PrivateUri { get; set; }

        [Required]
        public string DestinationPath { get; set; }

        public bool Overwrite { get; set; }

        public override bool Execute()
        {
            string destinationDir = Path.GetDirectoryName(DestinationPath);
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            if (File.Exists(DestinationPath) && !Overwrite)
            {
                return true;
            }

            const string FileUriProtocol = "file://";

            if (Uri.StartsWith(FileUriProtocol, StringComparison.Ordinal))
            {
                var filePath = Uri.Substring(FileUriProtocol.Length);
                Log.LogMessage($"Copying '{filePath}' to '{DestinationPath}'");
                File.Copy(filePath, DestinationPath);
            }
            else
            {
                Log.LogMessage(MessageImportance.High, $"Downloading '{Uri}' to '{DestinationPath}'");

                if (!DownloadFromUri(Uri, DestinationPath))
                {
                    if (!string.IsNullOrWhiteSpace(PrivateUri))
                    {
                        Log.LogMessage(MessageImportance.High, $"Couldn't download file '{Uri}' to '{DestinationPath}'. Trying to download file from '{PrivateUri}' instead.");

                        if (!DownloadFromUri(PrivateUri, DestinationPath))
                        {
                            Log.LogError($"Couldn't download file '{PrivateUri}' to '{DestinationPath}' either.");
                            return false;
                        }
                    }
                    else
                    {
                        Log.LogError($"Couldn't download file '{Uri}' to '{DestinationPath}'");
                        return false;
                    }
                }
            }

            return true;
        }
        
        private bool DownloadFromUri(string source, string target)
        {
            try
            {
                using (var httpClient = new HttpClient())
                using (var outStream = File.Create(target))
                {
                    var getTask = httpClient.GetStreamAsync(source);
                    getTask.Result.CopyTo(outStream);
                }

                return true;
            }
            catch (Exception)
            {
                File.Delete(target);
                return false;
            }
        }
    }
}
