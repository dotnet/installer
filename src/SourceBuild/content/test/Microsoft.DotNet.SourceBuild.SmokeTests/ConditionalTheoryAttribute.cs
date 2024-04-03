// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;

namespace Microsoft.DotNet.SourceBuild.SmokeTests;

/// <summary>
/// A Theory that conditionally runs based on a type's boolean property member.
/// </summary>
internal sealed class ConditionalTheoryAttribute : TheoryAttribute
{
    public ConditionalTheoryAttribute(Type calleeType, string memberName, string? reason = null)
    {
        ConditionalFactAttribute.EvaluateSkip(calleeType, memberName, reason, (skip) => Skip = skip);
    }
}
