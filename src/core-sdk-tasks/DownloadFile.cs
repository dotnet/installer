// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.DotNet.Cli.Build
{
    public class DownloadFile : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string Uri { get; set; }

        /// <summary>
        /// If this field is set and the task fail to download the file from `Uri`, with a NotFound
        /// status, it will try to download the file from `PrivateUri`.
        /// </summary>
        public string PrivateUri { get; set; }

        public int MaxRetries { get; set; } = 5;

        [Required]
        public string DestinationPath { get; set; }

        public bool Overwrite { get; set; }

        public override bool Execute()
        {
            return ExecuteAsync().GetAwaiter().GetResult();
        }

        private async System.Threading.Tasks.Task<bool> ExecuteAsync()
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
                return true;
            }

            var downloadStatus = await DownloadWithRetriesAsync(Uri, DestinationPath);
            if (downloadStatus == false)
            {
                if (!string.IsNullOrEmpty(PrivateUri))
                {
                    downloadStatus = await DownloadWithRetriesAsync(PrivateUri, DestinationPath);
                }
            }

            if (downloadStatus != true)
            {
                throw new FileNotFoundException($"Error while attempting download file from the provided URLs.");
            }

            return true;
        }

        /// <summary>
        /// Attempt to download file from `source` with retries when response error is different of FileNotFound and Success.
        /// </summary>
        /// <param name="source">URL to the file to be downloaded.</param>
        /// <param name="target">Local path where to put the downloaded file.</param>
        /// <returns>true: Download Succeeded. false: Download failed with 404. null: Download failed but is retriable.</returns>
        private async Task<bool?> DownloadWithRetriesAsync(string source, string target)
        {
            Random rng = new Random();

            Log.LogMessage(MessageImportance.High, $"Attempting download '{source}' to '{target}'");

            using (var httpClient = new HttpClient())
            {
                for (int retryNumber = 0; retryNumber < MaxRetries; retryNumber++)
                {
                    try
                    {
                        var httpResponse = await httpClient.GetAsync(source);

                        // The Azure Storage REST API returns '400 - Bad Request' in some cases
                        // where the resource is not found on the storage.
                        // https://docs.microsoft.com/en-us/rest/api/storageservices/common-rest-api-error-codes
                        if (httpResponse.StatusCode == HttpStatusCode.NotFound ||
                            httpResponse.ReasonPhrase.IndexOf("The requested URI does not represent any resource on the server.", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Log.LogMessage($"File not found error while attempting download using the following URL(s): '{source}'.");
                            return false;
                        }

                        using (var outStream = File.Create(target))
                        {
                            await httpResponse.Content.CopyToAsync(outStream);
                        }

                        return true;
                    }
                    catch (Exception e)
                    {
                        Log.LogMessage($"Problems downloading file from '{source}'. {e.Message} {e.StackTrace}");
                        File.Delete(target);
                    }

                    await System.Threading.Tasks.Task.Delay(rng.Next(1000, 10000));
                }
            }

            Log.LogMessage($"Giving up downloading the file from '{source}' after {MaxRetries} retries.");
            return null;
        }
    }
}
