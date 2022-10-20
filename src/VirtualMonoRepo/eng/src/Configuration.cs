namespace Pipeline;

internal static class Configuration
{
    public const string VmrName = "dotnet/dotnet";
    public const string PipelineRootDir = "src/VirtualMonoRepo/eng";
    
    public const string DarcBotName = "dotnet-maestro[bot]";
    public const string DarcBotEmail = "dotnet-maestro[bot]@users.noreply.github.com";
    
    public const string DarcBotPatVariable = "BotAccount-dotnet-bot-repo-PAT";
    public const string DarcBotVariables = "DotNetBot-GitHub";
}
