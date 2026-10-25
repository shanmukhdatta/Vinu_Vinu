# Preserve the older independent SyncCast reference build.
$ErrorActionPreference = 'Stop'
Push-Location (Join-Path $PSScriptRoot '..')
try {
  dotnet restore legacy/SyncCast/SyncCast.csproj
  if ($LASTEXITCODE -ne 0) { throw 'Legacy restore failed' }
  dotnet build legacy/SyncCast/SyncCast.csproj -c Release --no-restore
  if ($LASTEXITCODE -ne 0) { throw 'Legacy build failed' }
  dotnet run --project legacy/SyncCast.SmokeTests/SyncCast.SmokeTests.csproj -c Release
  if ($LASTEXITCODE -ne 0) { throw 'Legacy smoke tests failed' }
} finally { Pop-Location }
