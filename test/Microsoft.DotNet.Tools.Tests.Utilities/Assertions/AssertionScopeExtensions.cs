// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions.Execution;

namespace Microsoft.DotNet.Tools.Test.Utilities
{
    public static class AssertionScopeExtensions
    {
        public static Continuation FailWithPreformatted(this AssertionScope assertionScope, string message)
        {
            if (!assertionScope.Succeeded)
            {
                assertionScope.AddFailure(message);
            }

            return new Continuation(assertionScope, assertionScope.Succeeded);
        }
    }
}