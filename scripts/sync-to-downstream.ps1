[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('wk-vrcfury-qol', 'wk-vrc-qol', 'vrcfury-qol', 'avatar-qol')]
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

$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$src = Resolve-Path "$here/.."

# Repo map. Each target lists its sibling repo directory name (under
# D:\Github\VRC by convention) and the namespace root used inside its
# Editor/Internal/ tree. The 'vrcfury-qol' / 'avatar-qol' aliases stay so
# existing muscle memory keeps working post-rename.
$repoMap = @{
    'wk-vrcfury-qol' = @{ Repo = 'wk-vrcfury-qol'; Namespace = 'UmeVrcfQol.Internal' }
    'wk-vrc-qol'     = @{ Repo = 'wk-vrc-qol';     Namespace = 'WhyKnot.AvatarQol.Internal' }
    'vrcfury-qol'    = @{ Repo = 'wk-vrcfury-qol'; Namespace = 'UmeVrcfQol.Internal' }
    'avatar-qol'     = @{ Repo = 'wk-vrc-qol';     Namespace = 'WhyKnot.AvatarQol.Internal' }
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
# needs. Files at Editor/<Name>.cs (no sub-folder) land in the bare
# WhyKnot.Core namespace and rely on the bare-root rewrite entry below.
$filesToBundle = @(
    'Editor/HotReload/EditorHotReload.cs',
    'Editor/HotReload/WkHotReloadStatus.cs',
    'Editor/Logging/WkLogger.cs',
    'Editor/Logging/WkLoggerRegistry.cs',
    'Editor/Logging/WkLogContext.cs',
    'Editor/Logging/WkLogViewerWindow.cs',
    'Editor/Reflection/EditorElementWalker.cs',
    'Editor/Reflection/WkReflection.cs',
    'Editor/Reflection/WkReflectionCache.cs',
    'Editor/Reflection/WkGlobalId.cs',
    'Editor/Reflection/WkJsonClone.cs',
    'Editor/Settings/WkSettingsProvider.cs',
    'Editor/Styling/WkStyles.cs',
    'Editor/Styling/WkTheme.cs',
    'Editor/Styling/WkUiElements.cs',
    'Editor/Utilities/AvatarUtility.cs',
    'Editor/Utilities/BlendShapeUtility.cs',
    'Editor/Utilities/FbxMeshUtility.cs',
    'Editor/Utilities/FolderUtility.cs',
    'Editor/Utilities/HumanoidSideMap.cs',
    'Editor/Utilities/MeshUtility.cs',
    'Editor/Utilities/PathUtility.cs',
    'Editor/Utilities/UndoUtility.cs',
    'Editor/Utilities/WkEditorPrefs.cs',
    'Editor/Utilities/WkEditorTicker.cs',
    'Editor/Pipeline/AnimatorControllerUtility.cs',
    'Editor/Pipeline/AvatarIntentMode.cs',
    'Editor/Pipeline/WkAvatarPipeline.cs',
    'Editor/Pipeline/WkAvatarPipelineFallback.cs',
    'Editor/Pipeline/WkAvatarPipelineNdmf.cs',
    'Editor/Pipeline/WkAvatarPipelineTypes.cs',
    'Editor/Pipeline/WkAvatarPreviewSession.cs',
    'Editor/Pipeline/WkGeneratedAssetScope.cs',
    'Editor/Pipeline/WkRawSdkHook.cs',
    'Editor/Animator/IWkAnimatorBuilder.cs',
    'Editor/Animator/VrcExpressionUtility.cs',
    'Editor/Animator/WkAac.cs',
    'Editor/Animator/WkAacImpl.cs',
    'Editor/WkInspectorEditor.cs',
    'Editor/WkMenuPaths.cs',
    'Editor/WkToolWindow.cs'
)

# USS stylesheets and their meta files. The meta files are tracked too
# so the synced GUIDs stay stable; without that, each sync would
# invalidate UI Toolkit asset references in the downstream Unity project.
$ussFilesToBundle = @(
    'Editor/Styling/USS/wk-theme.uss',
    'Editor/Styling/USS/wk-theme-whyknot.uss',
    'Editor/Styling/USS/wk-theme-vrcfury.uss'
)

# Sub-namespace mapping. Source namespace -> Destination sub-namespace.
# Same shape on both sides so file references inside the copied set keep
# working after the rewrite (e.g. WkStyles using WkTheme from the same
# Styling sub-namespace).
#
# The bare 'WhyKnot.Core' entry catches root-level types (WkToolWindow,
# WkInspectorEditor, WkMenuPaths). Rewrite-Namespaces iterates in
# length-descending order so the longer 'WhyKnot.Core.<sub>' entries
# match first; the bare entry only fires on what's left over.
$nsMap = @{
    'WhyKnot.Core.HotReload'   = "$($cfg.Namespace).HotReload"
    'WhyKnot.Core.Logging'     = "$($cfg.Namespace).Logging"
    'WhyKnot.Core.Reflection'  = "$($cfg.Namespace).Reflection"
    'WhyKnot.Core.Settings'    = "$($cfg.Namespace).Settings"
    'WhyKnot.Core.Styling'     = "$($cfg.Namespace).Styling"
    'WhyKnot.Core.Utilities'   = "$($cfg.Namespace).Utilities"
    'WhyKnot.Core.Pipeline'    = "$($cfg.Namespace).Pipeline"
    'WhyKnot.Core.Animator'    = "$($cfg.Namespace).Animator"
    'WhyKnot.Core'             = "$($cfg.Namespace)"
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
    # Iterate by key length descending so the more-specific
    # 'WhyKnot.Core.Styling' entry matches before the bare-root
    # 'WhyKnot.Core' entry; otherwise the bare-root rewrite would
    # consume the prefix and leave a dangling '.Styling' segment.
    $sorted = $nsMap.GetEnumerator() | Sort-Object { $_.Key.Length } -Descending
    foreach ($pair in $sorted) {
        $text = $text.Replace($pair.Key, $pair.Value)
    }
    return $text
}

# Copy + rewrite each bundled C# file.
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

# Copy USS files verbatim (no namespace rewrite -- USS doesn't reference
# C# namespaces). Companion .uss.meta files are copied alongside so
# Unity asset GUIDs stay stable across syncs.
foreach ($rel in $ussFilesToBundle) {
    $srcPath = Join-Path $src $rel
    if (-not (Test-Path $srcPath)) {
        Write-Warning "USS source missing, skipping: $rel"
        continue
    }
    $dstRel = $rel -replace '^Editor/', ''
    $dstPath = Join-Path $internal $dstRel
    $dstDir  = Split-Path -Parent $dstPath
    if (-not (Test-Path $dstDir)) { New-Item -ItemType Directory -Path $dstDir -Force | Out-Null }

    Copy-Item -LiteralPath $srcPath -Destination $dstPath -Force
    Write-Host "  copied  $dstRel"

    $metaSrc = "$srcPath.meta"
    if (Test-Path $metaSrc) {
        Copy-Item -LiteralPath $metaSrc -Destination "$dstPath.meta" -Force
        Write-Host "  copied  $dstRel.meta"
    }
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
Write-Host "Done. Bundled $($filesToBundle.Count) C# files + $($ussFilesToBundle.Count) USS files; rewrote $rewriteCount downstream files."
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Confirm the downstream Editor asmdef declares any optional-integration"
Write-Host "     versionDefines you want active (WK_NDMF on nadena.dev.ndmf, WK_VRC_SDK_AVATARS"
Write-Host "     on com.vrchat.avatars). The synced pipeline / animator / preview code is"
Write-Host "     guarded by these symbols and compiles to no-ops when they're undefined."
Write-Host "  2. Compile in Unity to verify."
Write-Host "  3. Bump version, commit, tag, release."
