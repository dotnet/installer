using System.Linq;
using Sharpliner;
using Sharpliner.AzureDevOps;

namespace Pipeline;

internal class VmrSyncPipeline : SingleStagePipelineDefinition
{
    #region YAML export settings
    
    public override TargetPathType TargetPathType => TargetPathType.RelativeToGitRoot;
    public override string TargetFile => $"{Configuration.PipelineRootDir}/vmr-synchronization.yml";

    public override string[]? Header => base.Header!.Concat(new[]
    {
        $"This pipeline synchronizes code from product repositories into the VMR (https://github.com/{Configuration.VmrName})"
    }).ToArray();

    #endregion

    public override SingleStagePipeline Pipeline => new()
    {
        Trigger = new("main"),
        Pr = new("main"),

        Parameters =
        {
            StringParameter("targetRef", "Target revision in dotnet/installer to synchronize", variables.Build.SourceVersion),
            StringParameter("vmrBranch", $"{Configuration.VmrName} branch to use", variables.Build.SourceBranchName),
        },

        Resources = new Resources()
        {
            Repositories = new()
            {
                new RepositoryResource("vmr")
                {
                    Type = RepositoryType.GitHub,
                    Name = Configuration.VmrName,
                    Endpoint = "dotnet",
                }
            }
        },

        Variables =
        {
            If.Equal(variables.System.TeamProject, "internal")
                .Group(Configuration.DarcBotVariables)
            .Else
                .Variable(Configuration.DarcBotPatVariable, "N/A"),
        },

        Jobs =
        {
            JobTemplate($"./{VmrSyncJobTemplate.TemplatePath}", new()
            {
                { "targetRef", parameters["targetRef"] },
                { "vmrBranch", parameters["vmrBranch"] },
            })
        }
    };
}
