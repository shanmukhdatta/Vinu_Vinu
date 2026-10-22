$ErrorActionPreference = 'Stop'
Push-Location (Join-Path $PSScriptRoot '..')
try {
  Write-Host '--- Checking .NET SDK ---'
  dotnet --info
  Write-Host '--- Managed smoke tests ---'
  dotnet run --project tests/SyncCast.SmokeTests/SyncCast.SmokeTests.csproj --configuration Release
  Write-Host '--- Build and publish Windows app ---'
  & (Join-Path $PSScriptRoot 'build-windows.ps1')
  Write-Host '--- Ready to launch dist/win-x64/SyncCast.exe ---'
} finally { Pop-Location }
