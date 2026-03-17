<#
.SYNOPSIS
    Builds the mod and deploys it to the local Steam Workshop folder.
.DESCRIPTION
    Reads the Workshop item ID from SteamWorkshopId.txt, then runs a Release
    build that copies the output to the matching workshop content directory.
    Prints the in-game console command needed to publish the update.
.EXAMPLE
    .\scripts\build_steam.ps1
#>
[CmdletBinding()]
param()

$Root = Resolve-Path (Join-Path $PSScriptRoot '..')

$idFile = Join-Path $Root 'SteamWorkshopId.txt'
if (-not (Test-Path $idFile)) {
    Write-Error 'SteamWorkshopId.txt not found.'
    exit 1
}

$steamId = (Get-Content $idFile -Raw).Trim()
if (-not $steamId) {
    Write-Error 'SteamWorkshopId.txt is empty.'
    exit 1
}

$csproj = Get-ChildItem -Path (Join-Path $Root 'src') -Filter '*.csproj' | Select-Object -First 1
if (-not $csproj) {
    Write-Error 'No .csproj file found in src/.'
    exit 1
}

dotnet build $csproj.FullName -c Release /p:SteamId=$steamId
if ($LASTEXITCODE -ne 0) {
    Write-Error 'Build and deploy failed.'
    exit $LASTEXITCODE
}
Write-Host "Build and deploy to local Steam workshop directory for $steamId succeeded."

$tmpPath = Join-Path $Root 'src\.vs\.workshop_path.tmp'
if (Test-Path $tmpPath) {
    $workshopPath = (Get-Content $tmpPath -Raw).Trim()
    if ($workshopPath) {
        Write-Host ""
        Write-Host "To publish this version, run in the in-game console:"
        Write-Host "  mod_updateworkshopitem $steamId $workshopPath TRUE"
    }
}
