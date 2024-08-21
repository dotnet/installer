module TableGenerator.App

open System
open TableGenerator.Shared
open TableGenerator.Reference
open TableGenerator.Table

let inputBranches =
        [ { GitBranchName = "main"
            DisplayName = "main<br>(9.0.x&nbsp;Runtime)"
            AkaMsChannel = Some("9.0.1xx/daily") }
          { GitBranchName = "release/9.0.1xx-rc1"
            DisplayName = "9.0.1xx-rc1<br>(9.0-rc1&nbsp;Runtime)"
            AkaMsChannel = Some("9.0.1xx-rc1/daily") }
          { GitBranchName = "release/8.0.4xx"
            DisplayName = "8.0.4xx<br>(8.0&nbsp;Runtime)"
            AkaMsChannel = Some("8.0.4xx/daily") }]


let referentNotes = """Reference notes:
> **1**: Our Debian packages are put together slightly differently than the other OS specific installers. Instead of combining everything, we have separate component packages that depend on each other. If you're installing the SDK from the .deb file (via dpkg or similar), then you'll need to install the corresponding dependencies first:
> * [Host, Host FX Resolver, and Shared Framework](https://github.com/dotnet/runtime/blob/main/docs/project/dogfooding.md#nightly-builds-table)
> * [ASP.NET Core Shared Framework](https://github.com/aspnet/AspNetCore/blob/main/docs/DailyBuilds.md)"""

let wholeTable branches =
    String.Join
        (Environment.NewLine + Environment.NewLine,
         [ table branches
           referentNotes
           referenceList branches ])

[<EntryPoint>]
let main argv =
    Console.WriteLine(wholeTable inputBranches)
    0
