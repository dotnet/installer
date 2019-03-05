Param(
  [Parameter(Mandatory=$true)][string] $barToken,
  [Parameter(Mandatory=$true)][string] $gitHubPat,
  [Parameter(Mandatory=$true)][string] $azdoPat,
  [Parameter(Mandatory=$true)][string] $outputFolder,
  [string] $darcVersion = '1.1.0-beta.19154.2',
  [switch] $includeToolset
)

$ErrorActionPreference = "Stop"
. $PSScriptRoot\tools.ps1

function CheckExitCode ([string]$stage)
{
  $exitCode = $LASTEXITCODE
  if ($exitCode  -ne 0) {
    Write-Host "Something failed in stage: '$stage'. Check for errors above. Exiting now..."
    ExitWithExitCode $exitCode
  }
}

try {
  Push-Location $PSScriptRoot
    
  Write-Host "Installing darc..."
  . .\darc-init.ps1 -darcVersion $darcVersion
  CheckExitCode "Running darc-init"

  $DarcExe = "$env:USERPROFILE\.dotnet\tools"
  $DarcExe = Resolve-Path "$DarcExe\darc.exe"
  
  if (!(Test-Path -Path $outputFolder)) {
      Create-Directory $outputFolder
  }
  
  $graphVizFilePath = "$outputFolder\graphviz.txt"
  $options = "get-dependency-graph --graphviz '$graphVizFilePath' --github-pat $gitHubPat --azdev-pat $azdoPat --password $barToken"
  
  if ($includeToolset) {
    Write-Host "Toolsets will be included in the graph..."
    $options += " --include-toolset"
  }

  Write-Host "Generating dependency graph..."
  $darc = Invoke-Expression "& `"$DarcExe`" $options"
  CheckExitCode "Generating dependency graph"
  
  $graph = Get-Content $graphVizFilePath
  Set-Content $graphVizFilePath -Value "Paste the following digraph object in http://www.webgraphviz.com `r`n", $graph
  Write-Host "'$graphVizFilePath' file created!"
}
catch {
  if (!$includeToolset) {
    Write-Host "This might be a toolset repo which includes only toolset dependencies. " -NoNewline -ForegroundColor Yellow
    Write-Host "Since -includeToolset is not set there is no graph to create. Include -includeToolset and try again..." -ForegroundColor Yellow
  }
  Write-Host $_
  Write-Host $_.Exception
  Write-Host $_.ScriptStackTrace
  ExitWithExitCode 1
}