// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if !SOURCE_BUILD
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.DotNet.VersionTools.Automation;
using Microsoft.DotNet.VersionTools.Automation.GitHubApi;
using System.IO;

namespace Microsoft.DotNet.Cli.Build
{
    public class WriteVersionToVersionsRepo : Task
    {
        [Required]
        public string BranchName { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Version { get; set; }

        [Required]
        public string GitHubPassword { get; set; }

        [Required]
        public string VersionsRepoPath { get; set; }
        
        public override bool Execute()
        {
            GitHubAuth auth = new GitHubAuth(GitHubPassword);

            GitHubWriteVersionUpdater repoUpdater = new GitHubWriteVersionUpdater(auth);
            repoUpdater.UpdateBuildInfoAsync(
                Name,
                Version,
                VersionsRepoPath).Wait();

            return true;
        }
    }
}
#endif
