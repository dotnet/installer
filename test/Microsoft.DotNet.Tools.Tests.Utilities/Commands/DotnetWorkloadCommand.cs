// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.DotNet.Cli.Utils;

namespace Microsoft.DotNet.Tools.Test.Utilities
{
    public class DotnetWorkloadCommand : DotnetCommand
    {
        public DotnetWorkloadCommand(string dotnetUnderTest)
            : base(dotnetUnderTest)
        {
        }

        public DotnetWorkloadCommand()
            : base(RepoDirectoriesProvider.DotnetUnderTest)
        {
        }

        public override CommandResult Execute(string args = "")
        {
            args = $"workload {args}";
            return base.Execute(args);
        }

        public override CommandResult ExecuteWithCapturedOutput(string args = "")
        {
            args = $"workload {args}";
            return base.ExecuteWithCapturedOutput(args);
        }
    }
}
