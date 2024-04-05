// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.UnifiedBuild.BaselineComparison.Tests;

public abstract class TestBase
{
    const string LogsDirectorySwitch = Config.ConfigSwitchPrefix + nameof(LogsDirectory);

    public static string LogsDirectory { get; } = (string)(AppContext.GetData(LogsDirectorySwitch) ?? throw new InvalidOperationException("Logs directory must be specified"));

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
