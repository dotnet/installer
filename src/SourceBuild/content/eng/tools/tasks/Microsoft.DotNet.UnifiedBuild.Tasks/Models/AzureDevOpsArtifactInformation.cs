// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

namespace Microsoft.DotNet.UnifiedBuild.Tasks;

public class AzureDevOpsArtifactInformation
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Source { get; init; }
    public required AzdoArtifactResources Resource { get; init; }
}

public class AzdoArtifactResources
{
    public required string Type { get; init; }
    public required string Data { get; init; }
    public required AzdoArtifactProperties Properties { get; init; }
    public required string Url { get; init; }
    public required string DownloadUrl { get; init; }
}

public class AzdoArtifactProperties
{
    public required string RootId { get; init; }
    public required string Artifactsize { get; init; }
    public required string HashType { get; init; }
    public required string DomainId { get; init; }
}