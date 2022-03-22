// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit.Abstractions;

namespace Microsoft.DotNet.SourceBuild.SmokeTests
{
    public class PoisonTests : SmokeTests
    {
        public PoisonTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        [SkippableFact(Config.PoisonReportPathEnv, skipOnNullOrWhiteSpace: true)]
        public void VerifyUsage()
        {
            // TODO: Do we need to pre-process the poison report to strip content that changes over time and distro?
            // See the SdkContentTests for how specific rid and versions are abstracted.
            // If needed, RemoveRids and RemoveVersionedPaths should be moved to the BaselineHelper

            // TODO: Should poison diffs result in failures or warnings?  
            // The answer is related to how much is changing in the poison report which may be a direct result of the SdkContent changing.

            BaselineHelper.CompareFiles("PoisonUsage.txt", Config.PoisonReportPath, OutputHelper);
        }
    }
}
