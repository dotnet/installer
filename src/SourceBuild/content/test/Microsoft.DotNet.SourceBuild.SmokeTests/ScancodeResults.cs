// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.DotNet.SourceBuild.SmokeTests;

public class ScancodeResults
{
    [JsonPropertyName("files")]
    public List<ScancodeFileResult> Files { get; set; } = new();
}

public class ScancodeFileResult
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("detected_license_expression")]
    public string? LicenseExpression { get; set; }
}
