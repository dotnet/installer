// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using Xunit;

namespace Microsoft.DotNet.SourceBuild.SmokeTests;

/// <summary>
/// A Fact that conditionally runs based on a type's boolean property member
/// </summary>
internal sealed class ConditionalFactAttribute : FactAttribute
{
    public ConditionalFactAttribute(Type calleeType, string memberName, string? reason = null)
    {
        EvaluateSkip(calleeType, memberName, reason, (skip) => Skip = skip);
    }

    internal static void EvaluateSkip(Type calleeType, string memberName, string? reason = null, Action<string> setSkip)
    {
        TypeInfo typeInfo = calleeType.GetTypeInfo();
        bool shouldRun = (bool?)typeInfo.GetProperty(memberName)?.GetValue(null) ?? false;
        if (!shouldRun)
        {
            setSkip(reason ?? "Skipped");
        }
    }
}