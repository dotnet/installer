// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Microsoft.DotNet.SourceBuild.SmokeTests;

public static class Utilities
{
    public static void EnumerateTarball(string tarballPath, Func<TarEntry, bool> continueEnumeration)
    {
        using FileStream fileStream = File.OpenRead(tarballPath);
        using GZipStream decompressorStream = new(fileStream, CompressionMode.Decompress);
        using TarReader reader = new(decompressorStream);

        TarEntry? entry = null;
        while ((entry = reader.GetNextEntry()) is not null && continueEnumeration(entry))
        {
            // Do nothing
        }
    }

    public static void ExtractTarball(string tarballPath, string outputDir)
    {
        using FileStream fileStream = File.OpenRead(tarballPath);
        using GZipStream decompressorStream = new(fileStream, CompressionMode.Decompress);
        TarFile.ExtractToDirectory(decompressorStream, outputDir, true);
    }

    public static void ExtractTarball(string tarballPath, string outputDir, string targetFilePath)
    {
        Matcher matcher = new();
        matcher.AddInclude(targetFilePath);

        EnumerateTarball(tarballPath, entry =>
        {
            if (matcher.Match(entry.Name).HasMatches)
            {
                string outputPath = Path.Join(outputDir, entry.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                using FileStream outputFileStream = File.Create(outputPath);
                entry.DataStream.CopyTo(outputFileStream);
                return false;
            }

            return true;
        });
    }

    public static IEnumerable<string> GetTarballContentNames(string tarballPath)
    {
        List<string> names = new();
        EnumerateTarball(tarballPath, entry =>
        {
            names.Add(entry.Name);
            return true;
        });

        return names;
    }

    public static async Task RetryAsync(Func<Task> executor, ITestOutputHelper outputHelper)
    {
        await Utilities.RetryAsync(
            async () =>
            {
                try
                {
                    await executor();
                    return null;
                }
                catch (Exception e)
                {
                    return e;
                }
            },
            outputHelper);
    }

    private static async Task RetryAsync(Func<Task<Exception?>> executor, ITestOutputHelper outputHelper)
    {
        const int maxRetries = 5;
        const int waitFactor = 5;

        int retryCount = 0;

        Exception? exception = await executor();
        while (exception != null)
        {
            retryCount++;
            if (retryCount >= maxRetries)
            {
                throw new InvalidOperationException($"Failed after {retryCount} retries.", exception);
            }

            int waitTime = Convert.ToInt32(Math.Pow(waitFactor, retryCount - 1));
            if (outputHelper != null)
            {
                outputHelper.WriteLine($"Retry {retryCount}/{maxRetries}, retrying in {waitTime} seconds...");
            }

            Thread.Sleep(TimeSpan.FromSeconds(waitTime));
            exception = await executor();
        }
    }
}
