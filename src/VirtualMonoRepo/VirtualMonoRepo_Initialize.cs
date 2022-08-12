// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.DotNet.DarcLib.VirtualMonoRepo;

namespace Microsoft.DotNet.VirtualMonoRepo.Tasks;

public class VirtualMonoRepo_Initialize : Task
{
    [Required]
    public string Repository { get; set; }

    public string Revision { get; set; }

    public override bool Execute()
    {
        throw new NotImplementedException();
    }
}
