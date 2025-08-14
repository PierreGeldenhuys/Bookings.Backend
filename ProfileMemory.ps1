param(
    [string]$ProcessName = "Bookings.Api",
    [string]$Duration = "00:05:00",
    [string]$OutputFile = "counters.csv"
)

# Ensure dotnet-counters is installed
if (-not (Get-Command dotnet-counters -ErrorAction SilentlyContinue)) {
    Write-Host "Installing dotnet-counters..."
    dotnet tool install -g dotnet-counters
    $env:PATH += ";$env:USERPROFILE\.dotnet\tools"
}

# Find PID of the process
$p = Get-Process | Where-Object { $_.ProcessName -like "*$ProcessName*" } | Select-Object -First 1
if (-not $p) {
    Write-Error "Process matching '$ProcessName' not found."
    exit 1
}
Write-Host "Found PID: $p.id ($($p.ProcessName))"

# Prompt user for live monitor or CSV export
$mode = Read-Host "Choose mode: [1] Live Monitor  [2] Export CSV  [default = 1]"
switch ($mode) {
    "2" {
        Write-Host "Collecting counters for $Duration..."
        dotnet-counters collect `
            --process-id $p.id `
            --counters System.Runtime,Microsoft.AspNetCore.Hosting `
            --format csv `
            --duration $Duration `
            --output $OutputFile
        Write-Host "CSV written to: $OutputFile"
    }
    default {
        Write-Host "Starting live monitor (press Ctrl+C to stop)..."
        dotnet-counters monitor `
            --process-id $p.id `
            System.Runtime `
            Microsoft.AspNetCore.Hosting
    }
}
