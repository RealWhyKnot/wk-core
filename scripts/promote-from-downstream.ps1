[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Source,           # path to a downstream .cs file
    [Parameter(Mandatory)][ValidateSet('HotReload','Logging','Reflection','Settings','Styling','Utilities','Pipeline','Animator','Root')]
    [string]$Subdir
)

# Helper for the reverse direction of sync-to-downstream.ps1: take a
# .cs file that lives in a downstream's Editor/ tree and promote it
# into wk-core's appropriate Editor/<Subdir>/ folder with the namespace
# rewritten back to WhyKnot.Core.<Subdir>. Used when a helper grown in
# a downstream proves worth elevating to shared infrastructure.
#
# Example:
#   pwsh scripts/promote-from-downstream.ps1 `
#       -Source D:/Github/VRC/wk-vrc-qol/Editor/Common/BlendShapeUtility.cs `
#       -Subdir Utilities
#
# Result: wk-core/Editor/Utilities/BlendShapeUtility.cs with namespace
# rewritten from (e.g.) WhyKnot.AvatarQol.Intent -> WhyKnot.Core.Utilities.
# Caller then runs sync-to-downstream.ps1 to fan the canonical wk-core
# copy back out to both downstreams.
#
# The rewriter only handles the most-common downstream namespace patterns
# (WhyKnot.AvatarQol.*, UmeVrcfQol.*, plus the namespaced Internal/
# variants). If you're promoting from an unusual namespace, edit the
# rewritten file by hand after promotion.

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Source)) {
    throw "Source file not found: $Source"
}

$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path "$here/.."

$file = Get-Item $Source
$subdirPath = if ($Subdir -eq 'Root') { 'Editor' } else { "Editor/$Subdir" }
$destDir = Join-Path $repoRoot $subdirPath
$destPath = Join-Path $destDir $file.Name
if (-not (Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir -Force | Out-Null }

$targetNamespace = if ($Subdir -eq 'Root') { 'WhyKnot.Core' } else { "WhyKnot.Core.$Subdir" }

# Replace each plausible downstream namespace prefix with the target
# wk-core namespace. The list intentionally includes both the Internal/
# variants and the non-Internal variants so a file promoted from
# Editor/Common/ (with namespace WhyKnot.AvatarQol.Common) lands at
# WhyKnot.Core.<Subdir> correctly.
$rewrites = @{
    'WhyKnot.AvatarQol.Internal.HotReload'   = $targetNamespace
    'WhyKnot.AvatarQol.Internal.Logging'     = $targetNamespace
    'WhyKnot.AvatarQol.Internal.Reflection'  = $targetNamespace
    'WhyKnot.AvatarQol.Internal.Settings'    = $targetNamespace
    'WhyKnot.AvatarQol.Internal.Styling'     = $targetNamespace
    'WhyKnot.AvatarQol.Internal.Utilities'   = $targetNamespace
    'WhyKnot.AvatarQol.Internal.Pipeline'    = $targetNamespace
    'WhyKnot.AvatarQol.Internal.Animator'    = $targetNamespace
    'WhyKnot.AvatarQol.Internal'             = $targetNamespace
    'UmeVrcfQol.Internal.HotReload'          = $targetNamespace
    'UmeVrcfQol.Internal.Logging'            = $targetNamespace
    'UmeVrcfQol.Internal.Reflection'         = $targetNamespace
    'UmeVrcfQol.Internal.Settings'           = $targetNamespace
    'UmeVrcfQol.Internal.Styling'            = $targetNamespace
    'UmeVrcfQol.Internal.Utilities'          = $targetNamespace
    'UmeVrcfQol.Internal.Pipeline'           = $targetNamespace
    'UmeVrcfQol.Internal.Animator'           = $targetNamespace
    'UmeVrcfQol.Internal'                    = $targetNamespace
    'WhyKnot.AvatarQol.Intent'               = $targetNamespace
    'WhyKnot.AvatarQol.Common'               = $targetNamespace
    'WhyKnot.AvatarQol.Tools'                = $targetNamespace
    'WhyKnot.AvatarQol.WeightFixes'          = $targetNamespace
    'WhyKnot.AvatarQol.BoneMerger'           = $targetNamespace
    'WhyKnot.AvatarQol.PhysBonePreset'       = $targetNamespace
    'WhyKnot.AvatarQol'                      = $targetNamespace
    'UmeVrcfQol.Tools'                       = $targetNamespace
    'UmeVrcfQol'                             = $targetNamespace
}

$content = [System.IO.File]::ReadAllText($file.FullName)
$sorted = $rewrites.GetEnumerator() | Sort-Object { $_.Key.Length } -Descending
foreach ($pair in $sorted) {
    $content = $content.Replace($pair.Key, $pair.Value)
}

# Also drop any 'internal' modifier on the class so the promoted helper
# is accessible from the synced Internal/ trees that re-receive it.
$content = $content -replace '(^|\s)internal(\s+(static|sealed|abstract|partial|class|struct|enum))', '$1public$2'

[System.IO.File]::WriteAllText($destPath, $content, (New-Object System.Text.UTF8Encoding($false)))
Write-Host "Promoted $($file.Name) -> $destPath"
Write-Host "Namespace rewritten to: $targetNamespace"
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Review the rewritten file -- some namespace patterns may need manual touch-ups."
Write-Host "  2. Add the new path to \$filesToBundle in scripts/sync-to-downstream.ps1 if it's not already there."
Write-Host "  3. Delete the original downstream copy after the next sync to avoid duplicate types."
Write-Host "  4. Commit + run scripts/sync-to-downstream.ps1 for both downstreams."
