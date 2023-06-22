// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.SourceBuild.SmokeTests
{
    public class SdkRuntimePacksTests : SmokeTests
    {
        private static readonly string _nuGetCacheFolder = "nuget-cache";

        // `dotnet new webapp` will have dependency on the two runtime packs that are distributed with the SDK:
        //   - Microsoft.AspNetCore.App.Runtime.<rid>
        //   - Microsoft.NETCore.App.Runtime.<rid>
        private static readonly string _templateName = DotNetTemplate.WebApp.GetName();

        public SdkRuntimePacksTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        [Fact]
        public void VerifyRuntimePacksForSelfContained()
        {
            string testProjectName = $"{nameof(SdkRuntimePacksTests)}_{_templateName}";
            string projectDirPath = DotNetHelper.ExecuteNew(_templateName, testProjectName);

            // Alter NuGet cache path to verify any packs being pulled
            // Existing cache is used by other tests and can contain packs from previous runs that also do self-contained publishes
            string customProps = $"/p:RestorePackagesPath={_nuGetCacheFolder}";
            DotNetHelper.ExecutePublish(testProjectName, selfContained: true, rid: Config.TargetRid, customProps: customProps);

            string nuGetCachePath = Path.Combine(projectDirPath, _nuGetCacheFolder);
            string failMessage = "Runtime packs were retrieved from NuGet instead of the SDK";

            // As long as the cache is empty and the publish succeeded we can be certain that no pack was pulled
            Assert.False(Directory.Exists(nuGetCachePath) && Directory.GetFiles(nuGetCachePath).Length == 0, failMessage);
        }

    }
}
