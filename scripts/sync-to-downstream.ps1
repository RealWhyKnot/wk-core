[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('vrcfury-qol', 'avatar-qol')]
    [string]$Target,
    # Override the destination repo root if your sibling layout differs from
    # D:\Github\VRC\<repo>. Pass an absolute path -- the script appends
    # Editor\Internal\ underneath it.
    [string]$DestRepoRoot
)

# Bundles wk-core's shared utility source into a downstream package with
# the namespace rewritten so the two downstream packages can coexist in
# one Unity project without duplicate-type errors. Drops wk-core into the
# downstream as an internal source set rather than a VPM dependency,
# eliminating the version-coupling footgun where VCC's >= floor lets users
# pair a new downstream with an old wk-core.
#
# Run any time wk-core source changes; both downstreams need their own
# re-sync invocation.

$ErrorActionPreference = 'Stop'

$here   = Split-Path -Parent $MyInvocation.MyCommand.Path
$src    = Resolve-Path "$here/.."
$repoMap = @{
    'vrcfury-qol' = @{
        Repo      = 'vrcfury-qol'
        Namespace = 'UmeVrcfQol.Internal'
    }
    'avatar-qol' = @{
        Repo      = 'vrc-avatar-qol'
        Namespace = 'WhyKnot.AvatarQol.Internal'
    }
}
$cfg = $repoMap[$Target]

if (-not $DestRepoRoot) {
    $DestRepoRoot = Resolve-Path "$src/../$($cfg.Repo)" -ErrorAction SilentlyContinue
    if (-not $DestRepoRoot) {
        throw "Could not locate sibling repo at $src/../$($cfg.Repo). Pass -DestRepoRoot to override."
    }
}
$internal = Join-Path $DestRepoRoot 'Editor/Internal'

# Files to bundle. WkCoreLogger and AssemblyInfo are wk-core-only and
# stay behind; everything else is shared infrastructure each downstream
# needs.
$filesToBundle = @(
    'Editor/HotReload/EditorHotReload.cs',
    'Editor/Logging/WkLogger.cs',
    'Editor/Logging/WkLoggerRegistry.cs',
    'Editor/Reflection/EditorElementWalker.cs',
    'Editor/Styling/WkStyles.cs',
    'Editor/Styling/WkTheme.cs',
    'Editor/Utilities/AvatarUtility.cs',
    'Editor/Utilities/FbxMeshUtility.cs',
    'Editor/Utilities/HumanoidSideMap.cs',
    'Editor/Utilities/PathUtility.cs'
)

# Sub-namespace mapping. Source namespace -> Destination sub-namespace.
# Same shape on both sides so file references inside the copied set keep
# working after the rewrite (e.g. WkStyles using WkTheme from the same
# Styling sub-namespace).
$nsMap = @{
    'WhyKnot.Core.HotReload'   = "$($cfg.Namespace).HotReload"
    'WhyKnot.Core.Logging'     = "$($cfg.Namespace).Logging"
    'WhyKnot.Core.Reflection'  = "$($cfg.Namespace).Reflection"
    'WhyKnot.Core.Styling'     = "$($cfg.Namespace).Styling"
    'WhyKnot.Core.Utilities'   = "$($cfg.Namespace).Utilities"
}

Write-Host "wk-core source : $src"
Write-Host "downstream     : $DestRepoRoot"
Write-Host "destination    : $internal"
Write-Host "namespace root : $($cfg.Namespace)"
Write-Host ""

# Clean out the previous bundle so removed files in wk-core get pruned
# rather than persisting as stale duplicates in the downstream.
if (Test-Path $internal) {
    Write-Host "Pruning existing $internal"
    Remove-Item -Recurse -Force $internal
}
New-Item -ItemType Directory -Path $internal -Force | Out-Null

function Rewrite-Namespaces([string]$text) {
    foreach ($pair in $nsMap.GetEnumerator()) {
        $text = $text.Replace($pair.Key, $pair.Value)
    }
    return $text
}

# Copy + rewrite each bundled file.
foreach ($rel in $filesToBundle) {
    $srcPath = Join-Path $src $rel
    if (-not (Test-Path $srcPath)) {
        Write-Warning "Source missing, skipping: $rel"
        continue
    }
    # Drop the leading 'Editor/' so the destination tree is
    # Editor/Internal/<Sub>/<File>.cs rather than Editor/Internal/Editor/...
    $dstRel = $rel -replace '^Editor/', ''
    $dstPath = Join-Path $internal $dstRel
    $dstDir  = Split-Path -Parent $dstPath
    if (-not (Test-Path $dstDir)) { New-Item -ItemType Directory -Path $dstDir -Force | Out-Null }

    $content = [System.IO.File]::ReadAllText($srcPath)
    $rewritten = Rewrite-Namespaces $content

    # Write UTF-8 no-BOM regardless of PowerShell version. Set-Content on
    # 5.1 emits a BOM that some Unity importers complain about.
    [System.IO.File]::WriteAllText($dstPath, $rewritten, (New-Object System.Text.UTF8Encoding($false)))
    Write-Host "  copied  $dstRel"
}

# Now rewrite using-statements and any direct WhyKnot.Core.* references
# inside the downstream's own .cs files so they bind to the bundled copy
# rather than to a (now absent) wk-core package.
Write-Host ""
Write-Host "Rewriting WhyKnot.Core.* references in $DestRepoRoot/Editor + Runtime ..."
$rootsToScan = @('Editor', 'Runtime') | ForEach-Object {
    $candidate = Join-Path $DestRepoRoot $_
    if (Test-Path $candidate) { $candidate }
}
$rewriteCount = 0
foreach ($root in $rootsToScan) {
    $cs = Get-ChildItem -Path $root -Recurse -Filter *.cs -File -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -notmatch '\\Internal\\' }
    foreach ($f in $cs) {
        $original = [System.IO.File]::ReadAllText($f.FullName)
        if ($original -notmatch 'WhyKnot\.Core\.') { continue }
        $updated = Rewrite-Namespaces $original
        if ($updated -ne $original) {
            [System.IO.File]::WriteAllText($f.FullName, $updated, (New-Object System.Text.UTF8Encoding($false)))
            $rewriteCount++
            Write-Host "  rewrote $($f.FullName.Substring($DestRepoRoot.ToString().Length + 1))"
        }
    }
}

Write-Host ""
Write-Host "Done. Bundled $($filesToBundle.Count) files; rewrote $rewriteCount downstream files."
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Drop the dev.whyknot.core entry from $($cfg.Repo)/package.json vpmDependencies."
Write-Host "  2. Compile in Unity to verify."
Write-Host "  3. Bump version, commit, tag, release."
