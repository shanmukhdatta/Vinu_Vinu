# Build the primary open-source Vinu Vinu Windows desktop application.
$ErrorActionPreference = 'Stop'
Push-Location (Join-Path $PSScriptRoot '..')
try {
  dotnet restore src/VinuVinu/VinuVinu.csproj
  if ($LASTEXITCODE -ne 0) { throw 'App restore failed.' }
  dotnet build src/VinuVinu/VinuVinu.csproj -c Release --no-restore -p:PublishSingleFile=false
  if ($LASTEXITCODE -ne 0) { throw 'App build failed.' }
  dotnet publish src/VinuVinu/VinuVinu.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist/VinuVinu
  if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }
  Write-Host "Built executable: $(Join-Path (Get-Location) 'dist/VinuVinu/VinuVinu.exe')"
} finally { Pop-Location }
