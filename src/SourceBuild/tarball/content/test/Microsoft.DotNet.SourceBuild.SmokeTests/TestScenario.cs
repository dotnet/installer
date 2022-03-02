// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.DotNet.SourceBuild.SmokeTests
{
    public class TestScenario
    {
        public DotNetActions Commands { get; }
        public DotNetLanguage Language { get; }
        public bool NoHttps { get; set; } = Config.TargetRid.Contains("osx");
        public string ScenarioName { get; }
        public DotNetTemplate Template { get; }

        public TestScenario(string scenarioName, DotNetLanguage language, DotNetTemplate template, DotNetActions commands = DotNetActions.None)
        {
            ScenarioName = scenarioName;
            Template = template;
            Language = language;
            Commands = commands;
        }

        internal void Execute(DotNetHelper dotNetHelper)
        {
            string templateName = Template.GetName();
            string languageName = GetLanguageName();
            string customNewArgs = Template.IsAspNetCore() && NoHttps ? "--no-https" : string.Empty;

            // Can't use '#' in the project name: https://github.com/dotnet/roslyn/issues/51692
            string projectName = $"{ScenarioName}_{templateName}_{languageName.Replace("#", "Sharp")}";
            dotNetHelper.ExecuteNew(templateName, projectName, languageName, customArgs: customNewArgs);

            if (Commands.HasFlag(DotNetActions.Build))
            {
                dotNetHelper.ExecuteBuild(projectName);
            }
            if (Commands.HasFlag(DotNetActions.Run))
            {
                if (Template.IsAspNetCore())
                {
                    dotNetHelper.ExecuteRunWeb(projectName);
                }
                else
                {
                    dotNetHelper.ExecuteRun(projectName);
                }
            }
            if (Commands.HasFlag(DotNetActions.Publish))
            {
                dotNetHelper.ExecutePublish(projectName);
            }
            if (Commands.HasFlag(DotNetActions.PublishComplex))
            {
                dotNetHelper.ExecutePublish(projectName, selfContained: false);
                dotNetHelper.ExecutePublish(projectName, selfContained: true, Config.TargetRid);
                dotNetHelper.ExecutePublish(projectName, selfContained: true, "linux-x64");
            }
            if (Commands.HasFlag(DotNetActions.PublishR2R))
            {
                dotNetHelper.ExecutePublish(projectName, selfContained: true, "linux-x64", trimmed: true, readyToRun: true);
            }
            if (Commands.HasFlag(DotNetActions.Test))
            {
                dotNetHelper.ExecuteTest(projectName);
            }
        }

        private string GetLanguageName() => Language switch
        {
            DotNetLanguage.CSharp => "C#",
            DotNetLanguage.FSharp => "F#",
            DotNetLanguage.VB => "VB",
            _ => throw new NotImplementedException()
        };
    }
}
