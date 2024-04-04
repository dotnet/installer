// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.UnifiedBuild.BaselineTests;

public abstract class TestBase 
{
    public static string LogsDirectory { get; } = Path.Combine(Directory.GetCurrentDirectory(), "logs");

    public ITestOutputHelper OutputHelper { get; }

    public TestBase(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
        if (!Directory.Exists(LogsDirectory))
        {
            Directory.CreateDirectory(LogsDirectory);
        }
    }
}
