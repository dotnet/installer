// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.SourceBuild.SmokeTests;

/// <summary>
/// Shared base class for all smoke tests.
/// </summary>
[Trait("Category", "Default")]
public abstract class SmokeTests
{
    internal DotNetHelper DotNetHelper { get; }
    internal ITestOutputHelper OutputHelper { get; }

    protected SmokeTests(ITestOutputHelper outputHelper)
    {
        DotNetHelper = new DotNetHelper(outputHelper);
        OutputHelper = outputHelper;
    }

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
}
