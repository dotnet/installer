// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if !SOURCE_BUILD
using Microsoft.DotNet.VersionTools.Automation;
using Microsoft.DotNet.VersionTools.Automation.GitHubApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.DotNet.Cli.Build
{
    // This code is a slightly modified port of GitHubVersionsRepoUpdater from 
    // https://raw.githubusercontent.com/dotnet/buildtools/master/src/Microsoft.DotNet.VersionTools/Automation/GitHubVersionsRepoUpdater.cs
    internal class GitHubWriteVersionUpdater: VersionsRepoUpdater
    {
        private const int MaxTries = 10;
        private const int RetryMillisecondsDelay = 5000;

        private GitHubAuth _gitHubAuth;
        private GitHubProject _project;

        public GitHubWriteVersionUpdater(
            GitHubAuth gitHubAuth,
            string versionsRepoOwner = null,
            string versionsRepo = null)
            : this(
                gitHubAuth,
                new GitHubProject(versionsRepo ?? "versions", versionsRepoOwner))
        {
        }

        public GitHubWriteVersionUpdater(GitHubAuth gitHubAuth, GitHubProject project)
        {
            if (gitHubAuth == null)
            {
                throw new ArgumentNullException(nameof(gitHubAuth));
            }
            _gitHubAuth = gitHubAuth;

            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }
            _project = project;
        }
        /// <param name="updateLatestVersion">If true, updates Latest.txt with a prerelease moniker. If there isn't one, makes the file empty.</param>
        /// <param name="updateLatestPackageList">If true, updates Latest_Packages.txt.</param>
        public async Task UpdateBuildInfoAsync(
            string versionIdentifier,
            string version,
            string versionsRepoPath,
            bool updateLatestPackageList = true,
            bool updateLatestVersion = true)
        {
            if (versionIdentifier == null)
            {
                throw new ArgumentNullException(nameof(versionIdentifier));
            }
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }
            if (versionsRepoPath == null)
            {
                throw new ArgumentNullException(nameof(versionsRepoPath));
            }

            using (GitHubClient client = new GitHubClient(_gitHubAuth))
            {
                for (int i = 0; i < MaxTries; i++)
                {
                    try
                    {
                        // Master commit to use as new commit's parent.
                        string masterRef = "heads/master";
                        GitReference currentMaster = await client.GetReferenceAsync(_project, masterRef);
                        string masterSha = currentMaster.Object.Sha;

                        List<GitObject> objects = new List<GitObject>();

                        if (updateLatestVersion)
                        {
                            objects.Add(new GitObject
                            {
                                Path = $"{versionsRepoPath}/Latest.txt",
                                Type = GitObject.TypeBlob,
                                Mode = GitObject.ModeFile,
                                Content = version
                            });
                        }
                       if (updateLatestPackageList)
                       {
                           objects.Add(new GitObject
                           {
                               Path = $"{versionsRepoPath}/Latest_Packages.txt",
                               Type = GitObject.TypeBlob,
                               Mode = GitObject.ModeFile,
                               Content = $"{versionIdentifier} {version}{Environment.NewLine}"
                           });
                       }
                        string message = $"Updating {versionsRepoPath}";

                        GitTree tree = await client.PostTreeAsync(_project, masterSha, objects.ToArray());
                        GitCommit commit = await client.PostCommitAsync(_project, message, tree.Sha, new[] { masterSha });

                        // Only fast-forward. Don't overwrite other changes: throw exception instead.
                        await client.PatchReferenceAsync(_project, masterRef, commit.Sha, force: false);

                        Trace.TraceInformation($"Committed build-info update on attempt {i + 1}.");
                        break;
                    }
                    catch (HttpRequestException ex)
                    {
                        int nextTry = i + 1;
                        if (nextTry < MaxTries)
                        {
                            Trace.TraceInformation($"Encountered exception committing build-info update: {ex.Message}");
                            Trace.TraceInformation($"Trying again in {RetryMillisecondsDelay}ms. {MaxTries - nextTry} tries left.");
                            await Task.Delay(RetryMillisecondsDelay);
                        }
                        else
                        {
                            Trace.TraceInformation("Encountered exception committing build-info update.");
                            throw;
                        }
                    }
                }
            }
        }
    }
}
#endif
