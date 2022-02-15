// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.DotNet.SourceBuild.SmokeTests;

/// <summary>
/// Web project create, build, run, publish scenario tests.
/// <see cref="BaseScenarioTests"/> for related basic scenarios.
/// They are encapsulated in a separate testclass so that they can be run in parallel.
/// </summary>
public class WebScenarioTests : SmokeTests
{
    public WebScenarioTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Theory]
    [MemberData(nameof(GetScenarioObjects))]
    public void VerifyScenario(TestScenario scenario) => scenario.Execute(DotNetHelper);

    private static IEnumerable<object[]> GetScenarioObjects() => GetScenarios().Select(scenario => new object[] { scenario });

    private static IEnumerable<TestScenario> GetScenarios()
    {
        foreach (DotnetLanguage language in new[] { DotnetLanguage.CSharp, DotnetLanguage.FSharp })
        {
            yield return new(nameof(WebScenarioTests), language, DotnetTemplate.Web,    DotnetActions.Build | DotnetActions.Run | DotnetActions.PublishComplex);
            yield return new(nameof(WebScenarioTests), language, DotnetTemplate.Mvc,    DotnetActions.Build | DotnetActions.Run | DotnetActions.Publish) { NoHttps = true };
            yield return new(nameof(WebScenarioTests), language, DotnetTemplate.WebApi, DotnetActions.Build | DotnetActions.Run | DotnetActions.Publish);
        }

        yield return new(nameof(WebScenarioTests), DotnetLanguage.CSharp, DotnetTemplate.Razor,         DotnetActions.Build | DotnetActions.Run | DotnetActions.Publish);
        yield return new(nameof(WebScenarioTests), DotnetLanguage.CSharp, DotnetTemplate.BlazorWasm,    DotnetActions.Build | DotnetActions.Run | DotnetActions.Publish);
        yield return new(nameof(WebScenarioTests), DotnetLanguage.CSharp, DotnetTemplate.BlazorServer,  DotnetActions.Build | DotnetActions.Run | DotnetActions.Publish);
        yield return new(nameof(WebScenarioTests), DotnetLanguage.CSharp, DotnetTemplate.Worker);
        yield return new(nameof(WebScenarioTests), DotnetLanguage.CSharp, DotnetTemplate.Angular);
    }
}
