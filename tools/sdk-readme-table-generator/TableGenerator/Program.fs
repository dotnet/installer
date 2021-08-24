﻿module TableGenerator.App

open System
open TableGenerator.Shared
open TableGenerator.Reference
open TableGenerator.Table

let inputBranches =
        [ { GitBranchName = "release/6.0.1xx"
            DisplayName = "Release/6.0.1XX<br>(6.0.x&nbsp;Runtime)"
            AkaMsChannel = Some("6.0/daily") }
          { GitBranchName = "release/6.0.1xx-rc1"
            DisplayName = "Release/6.0.1XX-rc1<br>(6.0 Runtime)"
            AkaMsChannel = Some("6.0.1XX-rc1/daily") }
          { GitBranchName = "release/5.0.4xx"
            DisplayName = "Release/5.0.4XX<br>(5.0 Runtime)"
            AkaMsChannel = Some("5.0.4xx/daily") }
          { GitBranchName = "release/5.0.2xx"
            DisplayName = "Release/5.0.2XX<br>(5.0 Runtime)"
            AkaMsChannel = Some("5.0.2xx/daily") }
          { GitBranchName = "release/3.1.4xx"
            DisplayName = "Release/3.1.4XX<br>(3.1.x Runtime)"
            AkaMsChannel = None }
          { GitBranchName = "release/3.1.1xx"
            DisplayName = "Release/3.1.1XX<br>(3.1.x Runtime)"
            AkaMsChannel = None }]


let referentNotes = """Reference notes:
> **1**: Our Debian packages are put together slightly differently than the other OS specific installers. Instead of combining everything, we have separate component packages that depend on each other. If you're installing the SDK from the .deb file (via dpkg or similar), then you'll need to install the corresponding dependencies first:
> * [Host, Host FX Resolver, and Shared Framework](https://github.com/dotnet/runtime#daily-builds)
> * [ASP.NET Core Shared Framework](https://github.com/aspnet/AspNetCore/blob/main/docs/DailyBuilds.md)

.NET Core SDK 2.x downloads can be found here: [.NET Core SDK 2.x Installers and Binaries](Downloads2.x.md)"""

let sdksha2 =
    "[sdk-shas-2.2.1XX]: https://github.com/dotnet/versions/tree/master/build-info/dotnet/product/cli/release/2.2#built-repositories"

let wholeTable branches =
    String.Join
        (Environment.NewLine + Environment.NewLine,
         [ table branches
           referentNotes
           referenceList branches
           sdksha2 ])

[<EntryPoint>]
let main argv =
    Console.WriteLine(wholeTable inputBranches)
    0
