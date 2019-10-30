// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Threading;

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
            else
            {
                Log.LogMessage(MessageImportance.High, $"Downloading '{Uri}' to '{DestinationPath}'");

                for (int retryNumber = 0; retryNumber < MaxRetries; retryNumber++)
                {
                    using (var httpClient = new HttpClient())
                    {
                        try
                        {
                            var httpResponse = await httpClient.GetAsync(Uri);

                            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                            {
                                if (!string.IsNullOrEmpty(PrivateUri))
                                {
                                    httpResponse = await httpClient.GetAsync(PrivateUri);
                                }

                                // The Azure Storage REST API returns '400 - Bad Request' in some cases
                                // where the resource is not found on the storage.
                                // https://docs.microsoft.com/en-us/rest/api/storageservices/common-rest-api-error-codes
                                if (httpResponse.StatusCode == HttpStatusCode.NotFound ||
                                    httpResponse.ReasonPhrase.IndexOf("The requested URI does not represent any resource on the server.", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    return false;
                                }
                            }

                            using (var outStream = File.Create(DestinationPath))
                            {
                                await httpResponse.Content.CopyToAsync(outStream);
                            }

                            return true;
                        }
                        catch (Exception e)
                        {
                            var secondaryUri = !string.IsNullOrWhiteSpace(PrivateUri) ? $"/'{PrivateUri}'" : string.Empty;
                            Log.LogMessage($"Problems downloading file from '{Uri}'{secondaryUri}. {e.Message} {e.StackTrace}");
                            File.Delete(DestinationPath);
                        }
                    }

                    Thread.Sleep(new Random().Next(3000, 10000));
                }

                return false;
            }
        }
    }
}
