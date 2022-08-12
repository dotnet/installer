// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.DotNet.DarcLib;
using Microsoft.DotNet.DarcLib.Helpers;
using Microsoft.DotNet.DarcLib.VirtualMonoRepo;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.DotNet.VirtualMonoRepo.Tasks;

public class VirtualMonoRepo_Initialize : Build.Utilities.Task
{
    [Required]
    public string Repository { get; set; }

    [Required]
    public string VmrPath { get; set; }

    [Required]
    public string TmpPath { get; set; }

    public string Revision { get; set; }

    public override bool Execute()
    {
        var vmrManager = CreateVmrManager();

        var mapping = vmrManager.Mappings.FirstOrDefault(m => m.Name == Repository)
            ?? throw new Exception($"No repository mapping named `{Repository}` found!");

        vmrManager.InitializeVmr(mapping, Revision, false, default).GetAwaiter().GetResult();

        return true;
    }

    private IVmrManager CreateVmrManager()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddTransient<IProcessManager>(s => ActivatorUtilities.CreateInstance<ProcessManager>(s, "git"))
            .AddSingleton<ISourceMappingParser, SourceMappingParser>()
            .AddSingleton<IVmrManagerFactory, VmrManagerFactory>()
            .AddSingleton<IRemoteFactory>(s => ActivatorUtilities.CreateInstance<RemoteFactory>(s, TmpPath))
            .AddSingleton<IVmrManager>(s =>
            {
                var factory = s.GetRequiredService<IVmrManagerFactory>();
                return factory.CreateVmrManager(s, VmrPath, TmpPath).GetAwaiter().GetResult();
            });

        return services.BuildServiceProvider().GetRequiredService<IVmrManager>();
    }
}
