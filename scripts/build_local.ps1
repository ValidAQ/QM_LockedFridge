<#
.SYNOPSIS
    Builds the mod and stages the output to the build/ subfolder.
.DESCRIPTION
    Finds the .csproj in the src/ folder and runs a Release build.
    The compiled DLL and modmanifest.json are copied to <project root>/build/.
.EXAMPLE
    .\scripts\build_local.ps1
#>
[CmdletBinding()]
param()

$Root   = Resolve-Path (Join-Path $PSScriptRoot '..')
$csproj = Get-ChildItem -Path (Join-Path $Root 'src') -Filter '*.csproj' | Select-Object -First 1

if (-not $csproj) {
    Write-Error 'No .csproj file found in src/.'
    exit 1
}

$BuildDir = Join-Path $Root 'build'

dotnet build $csproj.FullName -c Release /p:DeployPath="$BuildDir\\"
if ($LASTEXITCODE -ne 0) {
    Write-Error 'Build failed.'
    exit $LASTEXITCODE
}
Write-Host "Build succeeded. Mod contents staged to: $BuildDir"
