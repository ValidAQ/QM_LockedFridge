<#
.SYNOPSIS
    Converts README.md to Steam-flavoured markup using [MarkdownToSteam](https://github.com/NBKRedSpy/MarkdownToSteam).
.DESCRIPTION
    Runs MarkdownToSteam.exe against the project README.md and writes the
    converted output to README.steam.txt in the project root.
.EXAMPLE
    .\scripts\markdown_to_steam.ps1
#>
[CmdletBinding()]
param()

# Adjust the path to MarkdownToSteam.exe as needed
$ToolPath = 'C:\Data\Code\Modding\Quasimorph\MarkdownToSteam\MarkdownToSteam.exe'
$Root     = Resolve-Path (Join-Path $PSScriptRoot '..')
$ReadMe   = Join-Path $Root 'README.md'
$Output   = Join-Path $Root 'README.steam.txt'

if (-not (Test-Path $ToolPath)) {
    Write-Error "MarkdownToSteam.exe not found at: $ToolPath"
    exit 1
}

if (-not (Test-Path $ReadMe)) {
    Write-Error "README.md not found at: $ReadMe"
    exit 1
}

& $ToolPath -i $ReadMe -o $Output
if ($LASTEXITCODE -ne 0) {
    Write-Error "MarkdownToSteam.exe failed with exit code $LASTEXITCODE."
    exit $LASTEXITCODE
}

Write-Host "Steam markup written to: $Output"
