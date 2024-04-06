[CmdletBinding(PositionalBinding=$false)]
Param(
  # Common settings
  [switch][Alias('bl')]$binaryLog,
  [string][Alias('c')]$configuration = "Release",
  [string][Alias('v')]$verbosity = "minimal",

  # Actions
  [switch]$clean,
  [switch][Alias('h')]$help,
  [switch][Alias('t')]$test,

  # Advanced settings
  [switch]$buildRepoTests,
  [switch]$testNoBuild,
  [switch]$ci,
  [switch][Alias('cwb')]$cleanWhileBuilding,
  [switch][Alias('nobl')]$excludeCIBinarylog,
  [switch] $prepareMachine,
  [Parameter(ValueFromRemainingArguments=$true)][String[]]$properties
)

function Get-Usage() {
  Write-Host "Common settings:"
  Write-Host "  -binaryLog              Output binary log (short: -bl)"
  Write-Host "  -configuration <value>  Build configuration: 'Debug' or 'Release' (short: -c). [Default: Release]"
  Write-Host "  -verbosity <value>      Msbuild verbosity: q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic] (short: -v)"
  Write-Host ""

  Write-Host "Actions:"
  Write-Host "  -clean                  Clean the solution"
  Write-Host "  -help                   Print help and exit (short: -h)"
  Write-Host "  -test                   Run tests (repo tests omitted by default) (short: -t)"
  Write-Host "                          Use in conjunction with -testNoBuild to run tests without building"
  Write-Host ""

  Write-Host "Advanced settings:"
  Write-Host "  -buildRepoTests         Build repository tests"
  Write-Host "  -testNoBuild            Run tests without building when invoked with -test"
  Write-Host "  -ci                     Set when running on CI server"
  Write-Host "  -cleanWhileBuilding     Cleans each repo after building (reduces disk space usage, short: -cwb)"
  Write-Host "  -excludeCIBinarylog     Don't output binary log (short: -nobl)"
  Write-Host "  -prepareMachine         Prepare machine for CI run, clean up processes after build"
  Write-Host ""
}

$useGlobalNuGetCache=$false

. $PSScriptRoot\common\tools.ps1

if ($help) {
  Get-Usage
  exit 0
}

$arguments = @()
$targets = "/t:Build"

# This repo uses the VSTest integration instead of the Arcade Test target
if ($test) {
  $arguments += "/p:Test=true"

  if ($testNoBuild) {
    $targets = "/t:VSTest"
    $arguments += "/p:VSTestNoBuild=true"
  } else {
    $targets += ";VSTest"
  }
}

if ($buildRepoTests) {
  $arguments += "/p:DotNetBuildTests=true"
}

if ($cleanWhileBuilding) {
  $arguments += "/p:CleanWhileBuilding=true"
}

function Build {
  InitializeToolset

  # Manually unset NUGET_PACKAGES as InitializeToolset sets it unconditionally.
  # The env var shouldn't be set so that the RestorePackagesPath msbuild property is respected.
  $env:NUGET_PACKAGES=''

  $bl = if ($binaryLog) { '/bl:' + (Join-Path $LogDir 'Build.binlog') } else { '' }

  $buildProj = Join-Path $RepoRoot 'build.proj'
  MSBuild -restore `
    $buildProj `
    $bl `
    $targets `
    /p:Configuration=$configuration `
    @properties `
    @arguments
}

try {
  if ($clean) {
    if (Test-Path $ArtifactsDir) {
      Remove-Item -Recurse -Force $ArtifactsDir
      Write-Host 'Artifacts directory deleted.'
    }
    exit 0
  }

  if ($ci) {
    if (-not $excludeCIBinarylog) {
      $binaryLog = $true
    }
  }

  Build
}
catch {
  Write-Host $_.ScriptStackTrace
  Write-PipelineTelemetryError -Category 'Build' -Message $_
  ExitWithExitCode 1
}

ExitWithExitCode 0
