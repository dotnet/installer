// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.DotNet.Cli.Utils;

namespace Microsoft.DotNet.Tools.Test.Utilities
{
    public class DotnetWorkloadCommand : DotnetCommand
    {
        private readonly string _extraArgs = string.Empty;

        public DotnetWorkloadCommand(string dotnetUnderTest)
            : base(dotnetUnderTest)
        {
            // Workaround for https://github.com/dotnet/sdk/issues/18450
            // .. use a local directory next to the project, as the tmp dir

            string tmpDir = Path.Combine(Path.GetDirectoryName(dotnetUnderTest), Path.GetRandomFileName());
            Directory.CreateDirectory(tmpDir);
            _extraArgs = $"--temp-dir {tmpDir}";
        }

        public DotnetWorkloadCommand()
            : base(RepoDirectoriesProvider.DotnetUnderTest)
        {
        }

        public override CommandResult Execute(string args = "") => base.Execute($"workload {GetFullArgs(args)}");
        public override CommandResult ExecuteWithCapturedOutput(string args = "") => base.ExecuteWithCapturedOutput($"workload {GetFullArgs(args)}");

        public CommandResult Install(string args = "") => base.Execute($"workload install {GetFullArgs(args)}");
        public CommandResult InstallWithCapturedOutput(string args = "") => base.ExecuteWithCapturedOutput($"workload install {GetFullArgs(args)}");

        private string GetFullArgs(string args) => $"{args} {_extraArgs}";
    }
}
