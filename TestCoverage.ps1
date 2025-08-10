param(
  [string]$Solution = ".",
  [switch]$MsBuildMode,
  [int]$Threshold = 0,
  [ValidateSet('line','branch','method')]
  [string]$ThresholdType = 'line',
  [string]$RunSettings = "",
  [string]$ReportDir = "coverage-report",
  [switch]$Open
)

$ErrorActionPreference = 'Stop'

function Assert-Tool {
  param([string]$Name, [string]$CheckCmd = $Name, [string]$InstallHint = "")
  try {
    & $CheckCmd --version | Out-Null
  } catch {
    throw "Required tool '$Name' not found. $InstallHint"
  }
}

Write-Host "==> Ensuring required tools..." -ForegroundColor Cyan
Assert-Tool -Name "dotnet" -InstallHint "Install .NET SDK: https://dotnet.microsoft.com/download"

$reportGenOk = $true
try { reportgenerator -? | Out-Null } catch { $reportGenOk = $false }
if (-not $reportGenOk) {
  Write-Host "==> Installing ReportGenerator global tool..." -ForegroundColor Yellow
  dotnet tool install -g dotnet-reportgenerator-globaltool | Out-Null
  $userTools = [IO.Path]::Combine($env:USERPROFILE, ".dotnet", "tools")
  if (-not ($env:PATH -split ';' | Where-Object { $_ -eq $userTools })) {
    $env:PATH = "$userTools;$env:PATH"
  }
}

if (-not (Test-Path $ReportDir)) { New-Item -ItemType Directory -Path $ReportDir | Out-Null }

$autoRunSettings = Join-Path $env:TEMP "coverlet.autogen.runsettings"
if (-not $MsBuildMode -and [string]::IsNullOrWhiteSpace($RunSettings)) {
  @"
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat Code Coverage">
        <Configuration>
          <Format>cobertura</Format>
          <ExcludeByAttribute>ExcludeFromCodeCoverage</ExcludeByAttribute>
          <Exclude>[xunit.*]*,[Moq.*]*,[NSubstitute.*]*</Exclude>
          <ExcludeByFile>**/Migrations/*.cs;**/*Designer.cs</ExcludeByFile>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
"@ | Set-Content -Path $autoRunSettings -Encoding UTF8
  $RunSettings = $autoRunSettings
}

# Run tests
Write-Host "Running tests ($([bool]$MsBuildMode ? 'MSBuild mode' : 'Collector mode'))..." -ForegroundColor Cyan
$testSucceeded = $true
try {
  if ($MsBuildMode) {
    $props = @(
      "/p:CollectCoverage=true",
      "/p:CoverletOutputFormat=cobertura",
      "/p:CoverletOutput=TestResults/coverage.cobertura.xml",
      "/p:Exclude=""[xunit.*]*,[Moq.*]*,[NSubstitute.*]*""",
      "/p:ExcludeByFile=""**/Migrations/*.cs;**/*Designer.cs""",
      "/p:ExcludeByAttribute=""ExcludeFromCodeCoverage"""
    )

    if ($Threshold -gt 0) {
      $props += "/p:Threshold=$Threshold"
      $props += "/p:ThresholdType=$ThresholdType"
      $props += "/p:ThresholdStat=total"
      Write-Host "   Threshold: $Threshold% ($ThresholdType, total)" -ForegroundColor DarkGray
    }

    dotnet test $Solution @props
  } else {
    $args = @("--collect:`"XPlat Code Coverage`"")
    if (-not [string]::IsNullOrWhiteSpace($RunSettings)) {
      $args += @("--settings", $RunSettings)
    }
    dotnet test $Solution @args
  }
} catch {
  $testSucceeded = $false
  Write-Host "dotnet test failed." -ForegroundColor Red
  throw
} finally {
  if (Test-Path $autoRunSettings) { Remove-Item $autoRunSettings -Force }
}

# Generate HTML report
Write-Host "Generating HTML coverage report..." -ForegroundColor Cyan
# Pick up all cobertura files produced by either mode
$reportGlob = "**/coverage.cobertura.xml"
reportgenerator `
  -reports:$reportGlob `
  -targetdir:$ReportDir `
  -reporttypes:Html `
  -assemblyfilters:+* `
  | Out-Null

$index = Join-Path $ReportDir "index.html"
if (-not (Test-Path $index)) {
  Write-Host "No coverage report generated. Looked for $reportGlob" -ForegroundColor Red
  exit 1
}

Write-Host "Coverage report: $index" -ForegroundColor Green

if ($Open) {
  try {
    if ($IsWindows) { Start-Process $index }
    elseif ($IsMacOS) { & open $index }
    else { & xdg-open $index }
  } catch {
    Write-Host "Could not auto-open the report. Open manually: $index" -ForegroundColor Yellow
  }
}

# Exit with non-zero if tests failed (dotnet test would have thrown already)
if (-not $testSucceeded) { exit 1 }
