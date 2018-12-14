// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.DotNet.Tools.Test.Utilities
{
    public class RequiresAspNetCore : FactAttribute
    {
        public RequiresAspNetCore()
        {
            var  d = new RepoDirectoriesProvider();
            
            if (d.Stage2AspNetCore == null)
            {
                this.Skip = $"This test requires a AspNetCore but isn't present.";
            }
        }
    }
}
