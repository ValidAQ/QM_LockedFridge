<#
.SYNOPSIS
    Creates a new mod project from this template in a sibling folder.
.DESCRIPTION
    Copies the entire template repository to a sibling folder named after the
    new mod, then replaces all occurrences of "QM_ModTemplate" in file contents
    and renames any files whose names contain the old name.

    Build artifacts (src\bin, src\obj) and the git history (.git) are excluded
    from the copy so the new project starts fresh.
.PARAMETER NewName
    The new mod name (letters, digits, and underscores; must start with a letter).
    Example: QM_LockedFridge
.EXAMPLE
    .\scripts\create_project.ps1 -NewName QM_LockedFridge
.EXAMPLE
    .\scripts\create_project.ps1 -NewName QM_LockedFridge -WhatIf
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[A-Za-z][A-Za-z0-9_]*$')]
    [string] $NewName
)

$OldName   = 'QM_ModTemplate'
$SrcRoot   = Resolve-Path (Join-Path $PSScriptRoot '..')
$DestRoot  = Join-Path (Split-Path $SrcRoot -Parent) $NewName
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)

# Folders/files to exclude from the copy (relative to the repo root, lowercase for comparison)
$ExcludeDirs  = @('.git', 'src\bin', 'src\obj')
$ExcludeFiles = @('scripts\build_local.bat', 'scripts\build_steam.bat')

if ($NewName -eq $OldName) {
    Write-Error "New name is the same as the current name ('$OldName'). Nothing to do."
    exit 1
}

if (Test-Path $DestRoot) {
    Write-Error "Destination folder already exists: $DestRoot"
    exit 1
}

# ── 1. Copy files, skipping excluded directories ───────────────────────────────
Write-Host "Creating new project at: $DestRoot"
Write-Host ""
Write-Host "Copying files..."

$allFiles = Get-ChildItem -Path $SrcRoot -Recurse -File
$excludeDirPrefixes = $ExcludeDirs | ForEach-Object {
    ($_.Replace('\', '/') + '/').ToLower()
}

foreach ($file in $allFiles) {
    $rel = $file.FullName.Substring($SrcRoot.Path.Length).TrimStart('\')

    # Skip if this file lives inside an excluded directory
    $skip = $false
    $relNorm = $rel.Replace('\', '/').ToLower()
    foreach ($excludePrefix in $excludeDirPrefixes) {
        if ($relNorm.StartsWith($excludePrefix)) {
            $skip = $true
            break
        }
    }
    if (-not $skip) {
        foreach ($excl in $ExcludeFiles) {
            if ($rel.ToLower() -eq $excl.ToLower()) {
                $skip = $true
                break
            }
        }
    }
    if ($skip) { continue }

    # Rename the file itself if its name contains the old project name
    $newRel  = $rel.Replace($OldName, $NewName)
    $destFile = Join-Path $DestRoot $newRel

    if ($PSCmdlet.ShouldProcess($newRel, 'Copy')) {
        $null = New-Item -ItemType Directory -Path (Split-Path $destFile) -Force

        # Replace content for text files; binary-copy everything else
        $textExtensions = @('.cs', '.csproj', '.sln', '.json', '.md', '.ps1', '.txt', '.xml', '.props', '.targets', '.config', '.yaml', '.yml', '.gitignore')
        if ($textExtensions -contains $file.Extension.ToLower()) {
            $content = [System.IO.File]::ReadAllText($file.FullName, $utf8NoBom)
            $content = $content.Replace($OldName, $NewName)
            [System.IO.File]::WriteAllText($destFile, $content, $utf8NoBom)
        } else {
            Copy-Item -Path $file.FullName -Destination $destFile
        }

        if ($rel -ne $newRel) {
            Write-Host "  Copied (renamed): $rel  ->  $newRel"
        }
    }
}

Write-Host ""
Write-Host "Done. New project created at: $DestRoot"
