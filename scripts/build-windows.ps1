$ErrorActionPreference = 'Stop'
Push-Location (Join-Path $PSScriptRoot '..')
try {
  dotnet restore src/SyncCast/SyncCast.csproj
  dotnet build src/SyncCast/SyncCast.csproj --configuration Release --no-restore
  dotnet publish src/SyncCast/SyncCast.csproj --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --output dist/win-x64
  Write-Host "EXE: $(Join-Path (Get-Location) 'dist/win-x64/SyncCast.exe')"
} finally { Pop-Location }
