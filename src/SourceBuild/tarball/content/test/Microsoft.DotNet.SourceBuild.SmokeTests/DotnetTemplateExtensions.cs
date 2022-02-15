// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.DotNet.SourceBuild.SmokeTests;

public static class DotnetTemplateExtensions
{
    public static string GetName(this DotnetTemplate template) => Enum.GetName(template)?.ToLowerInvariant() ?? throw new NotSupportedException();

    public static bool IsAspNetCore(this DotnetTemplate template) =>
        template == DotnetTemplate.Web
        || template == DotnetTemplate.Mvc
        || template == DotnetTemplate.WebApi
        || template == DotnetTemplate.Razor
        || template == DotnetTemplate.BlazorWasm
        || template == DotnetTemplate.BlazorServer
        || template == DotnetTemplate.Worker
        || template == DotnetTemplate.Angular;
}
