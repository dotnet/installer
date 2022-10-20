using System;
using System.Collections.Generic;
using System.Linq;
using Sharpliner;
using Sharpliner.AzureDevOps;
using Sharpliner.AzureDevOps.ConditionedExpressions;

namespace Pipeline;

internal class VmrSyncJobTemplate : JobTemplateDefinition
{
    #region YAML export settings

    public const string TemplatePath = "common/templates/jobs/vmr-synchronization.yml";
    public override TargetPathType TargetPathType => TargetPathType.RelativeToGitRoot;
    public override string TargetFile => $"{Configuration.PipelineRootDir}/{TemplatePath}";

    #endregion

    #region Environment definitions

    private static string InstallerPath => $"{variables.Agent.BuildDirectory}/installer";
    private static string VmrPath => $"{variables.Agent.BuildDirectory}/vmr";

    #endregion
    
    public override List<Parameter> Parameters { get; } = new()
    {
        StringParameter("targetRef", "Target revision in dotnet/installer to synchronize", variables.Build.SourceVersion),
        StringParameter("vmrBranch", $"{Configuration.VmrName} branch to use", variables.Build.SourceBranchName),
    };

    public override string[]? Header => base.Header!.Concat(new[]
    {
        $"These steps synchronize code from product repositories into the VMR (https://github.com/{Configuration.VmrName})"
    }).ToArray();

    public override ConditionedList<JobBase> Definition => new()
    {
        new Job("Synchronize", $"Synchronize {Configuration.VmrName}")
        {
            //Pool = 
            //    If.Equal(variables.System.TeamProject, "public")
            //        .Pool(new HostedPool()
            //        {
            //            Name = "NetCore-Public",
            //            Demands = new() { "ImageOverride -equals Build.Ubuntu.1804.Amd64.Open" }
            //        })
            //    .Else
            //        .Pool(new HostedPool()
            //        {
            //            Name = "NetCore1ESPool-Internal",
            //            Demands = new() { "ImageOverride -equals Build.Ubuntu.1804.Amd64" }
            //        }),
            Pool = new HostedPool(vmImage: "ubuntu-latest"),

            Timeout = TimeSpan.FromHours(2),
            Steps =
            {
                Checkout.Self with
                {
                    DisplayName = "Checkout dotnet/installer",
                    Clean = true,
                    Path = "installer",
                    FetchDepth = 0,
                },

                Checkout.Repository("vmr") with
                {
                    DisplayName = $"Checkout {Configuration.VmrName}",
                    Timeout = TimeSpan.FromMinutes(30),
                    Clean = true,
                    Path = "vmr",
                    FetchDepth = 0,
                },

                // Restore .NET SDK and the darc CLI
                Script.Inline(
                    "source ./eng/common/tools.sh",
                    "InitializeDotNetCli true",
                    "./.dotnet/dotnet tool restore") with
                {
                    DisplayName = $"Restore toolset",
                    WorkingDirectory = InstallerPath,
                },

                // For pull requests, we need to make sure that the commit from the PR is available in the clone
                If.IsPullRequest
                    .Step(Script.Inline($"cp -r {InstallerPath} {variables.Agent.TempDirectory}/installer") with
                    {
                        DisplayName = "[PR Only] Prepare dotnet/installer clone",
                    }),

                Script.Inline(
                    $"{InstallerPath}/.dotnet/dotnet darc vmr update " +
                    $"--vmr {VmrPath} " +
                    $"--tmp {variables.Agent.TempDirectory} " +
                    $"--azdev-pat {variables.System.AccessToken} " +
                    $"--github-pat {variables[Configuration.DarcBotPatVariable]} " +
                    "--recursive " +
                    "--verbose " +
                    $"installer:{parameters["targetRef"]}") with
                {
                    DisplayName = $"Synchronize {Configuration.VmrName}",
                    Timeout = TimeSpan.FromMinutes(90),
                    WorkingDirectory = InstallerPath,
                },

                If.And(IsNotPullRequest, Equal(variables.System.TeamProject, "internal"))
                    .Step(Script.Inline(
                        $"""
                        set -x
                        git config --global user.email '{Configuration.DarcBotEmail}' && git config --global user.name '{Configuration.DarcBotName}'
                        git remote add dotnet 'https://{variables[Configuration.DarcBotPatVariable]}@github.com/{Configuration.VmrName}.git'
                        git fetch dotnet
                        git branch {parameters["vmrBranch"]}
                        git branch --set-upstream-to=dotnet/{parameters["vmrBranch"]} {parameters["vmrBranch"]} || echo 'Branch {parameters["vmrBranch"]} not found in remote'
                        git push dotnet {parameters["vmrBranch"]}
                        """) with
                        {
                            DisplayName = $"Push changes to {Configuration.VmrName}",
                            Timeout = TimeSpan.FromHours(1),
                            WorkingDirectory = VmrPath,
                        })
            }
        }
    };
}
