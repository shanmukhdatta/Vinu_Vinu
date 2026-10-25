# Run local checks for the primary Windows app and synthetic endpoint verifier.
$ErrorActionPreference = 'Stop'
Push-Location (Join-Path $PSScriptRoot '..')
try {
  dotnet --info
  dotnet restore VinuVinu.slnx
  if ($LASTEXITCODE -ne 0) { throw 'Solution restore failed.' }
  dotnet test tests/VinuVinu.Tests/VinuVinu.Tests.csproj -c Release
  if ($LASTEXITCODE -ne 0) { throw 'App unit tests failed.' }
  dotnet run --project tools/VinuVerifier/VinuVerifier.csproj -c Release -- --self-test
  if ($LASTEXITCODE -ne 0) { throw 'Verifier self-test failed.' }
  & (Join-Path $PSScriptRoot 'build-windows.ps1')
  Write-Host 'Source, tests and application build completed. Real Bluetooth playback must be checked manually.'
} finally { Pop-Location }
