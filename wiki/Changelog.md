# Changelog

All notable changes to wk-core. Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/); versioning follows [Semantic Versioning](https://semver.org/).

The most recent release is at the top.

<!-- Entries under "## Unreleased" are appended automatically by the changelog-append GitHub
     workflow on every push to main, then promoted to the versioned section by release.yml when
     a tag is cut. Don't hand-edit Unreleased -- your edits will be overwritten on the next push.
     To override an entry, amend the commit subject before merge. -->

## Unreleased

---

## [1.1.1](https://github.com/RealWhyKnot/wk-core/releases/tag/v1.1.1) -- 2026-05-22

### Fixed
- `Editor/Logging/WkLogger.cs` failed to compile inside Unity with CS0119: the class declares a `Debug(...)` method (the log-level entry point), which shadows `UnityEngine.Debug` for unqualified references inside the class body. Two such bare `Debug.LogWarning(...)` / `Debug.LogException(...)` call sites were resolving against the local method instead of `UnityEngine.Debug` and rejected the wrong signatures. Both now use the fully-qualified `UnityEngine.Debug.*` form.

---

## [1.1.0](https://github.com/RealWhyKnot/wk-core/releases/tag/v1.1.0) -- 2026-05-22

### Added
- `WhyKnot.Core.Styling.WkTheme` carries the palette + chrome that `WkStyles` emits. Each theme has Pro and Personal `Variant`s for the two editor skins. Two presets ship: `WkTheme.WhyKnot` (black / gray / light blue -- the brand palette) and `WkTheme.VRCFury` (matches VRCFury's existing dark-gray row chrome and warm accents). Custom themes can be built by populating the same Variant fields.
- `WkStyles.Scope(WkTheme)` pushes a theme onto a stack for the lifetime of a `using` block; nested scopes restore the outer theme on dispose. `WkStyles.CurrentTheme` resolves to the top of the stack, or `WkStyles.DefaultTheme` (initially `WkTheme.WhyKnot`) when nothing is active. The whole `WkStyles` palette -- `ColorAccent`, `ColorWarning`, `ColorSuccess`, `ColorInfo`, `ColorDanger`, `ColorDivider`, `ColorBackground`, `ColorTextPrimary`, `ColorTextMuted`, `ColorBorder` -- now resolves through the active theme.
- `WhyKnot.Core.Logging.WkLogger` -- per-package file logger. Each WhyKnot package builds one in its `[InitializeOnLoad]` static constructor and the logger self-registers with `WkLoggerRegistry`. Output goes to `%LocalAppData%/WhyKnot/Logs/<package-id>/session-<timestamp>.log` (machine-wide, project-independent, so the user has one place to look regardless of which Unity project the issue surfaced in). Levels: `Debug`, `Info`, `Warning`, `Error`, plus `Exception(ex, context)` that dumps the stack trace. Each line carries timestamp, level tag, calling source file:line, and method name (via `[CallerMemberName]` / `[CallerFilePath]` / `[CallerLineNumber]`). Session header logs Unity version, project path, machine/user/batch-mode. Configurable per-level Unity-console mirror; Info / Warning / Error mirror by default, Debug stays file-only.
- Session retention: each package keeps at most 3 session log files. With three WhyKnot packages registered (`dev.whyknot.core`, `dev.whyknot.vrcfury-qol`, `dev.whyknot.avatar-qol`), that caps the total at 9 files no matter how many Unity launches the user does.
- `WhyKnot.Core.Logging.WkCoreLogger` -- wk-core's own registered logger. `EditorHotReload` and any other diagnostic surface inside this package routes through `WkCoreLogger.Instance`.

### Changed
- `EditorHotReload` no longer writes its own `<ProjectRoot>/Logs/WkCore.log` file. All of its output -- watcher activity, refresh ticks, compile started/finished, per-assembly summaries, compile errors, reload events -- now goes through `WkCoreLogger.Instance`. Watcher chatter is logged at `Debug` (file-only); compile errors at `Error` (mirrored to the Unity Console).

### Removed
- `WhyKnot.Core.Utilities.EditorLogger` -- replaced by `WhyKnot.Core.Logging.WkLogger`, which adds file output, session retention, level tags, source-location capture, and exception handling. `EditorLogger` shipped in 1.0.0 but had no downstream callers yet, so this is a clean break.

---

## [1.0.0](https://github.com/RealWhyKnot/wk-core/releases/tag/v1.0.0) -- 2026-05-22

First release. `dev.whyknot.core` is a shared Editor-only utility package consumed as a `vpmDependencies` entry by the WhyKnot VRChat tools. The surface consolidates helpers that were previously duplicated across `vrc-avatar-qol` and `vrcfury-qol`.

### Added
- `WhyKnot.Core.Styling.WkStyles` -- IMGUI palette, lazy-init typography, widget primitives (`Section`, `Notice`, `BadgePill`, `Divider`, `LabeledField`, `PrimaryButtonInline`, `HelpIcon`). Includes the `NoticeKind` enum.
- `WhyKnot.Core.Utilities.PathUtility.GetGameObjectPath` for slash-joined Transform paths.
- `WhyKnot.Core.Utilities.AvatarUtility.FindAvatarRoot` (Animator-then-scene-root fallback).
- `WhyKnot.Core.Utilities.FbxMeshUtility.ResolveEditableMesh` consolidating the per-tool clone-on-FBX pattern; registers Undo for both the created asset and the renderer property change so Ctrl+Z restores both.
- `WhyKnot.Core.Utilities.HumanoidSideMap` mapping Transforms to Left / Right / Center / Unknown.
- `WhyKnot.Core.Utilities.EditorLogger` -- prefixed `Debug.Log` wrapper.
- `WhyKnot.Core.Reflection.EditorElementWalker` -- UIElements walkers and inspector overlay chrome (`ApplyBannerChromeStyle`, `ApplyInlineButtonStyle`, `ApplyDangerButtonStyle`).
- `WhyKnot.Core.HotReload.EditorHotReload` -- `[InitializeOnLoad]` `FileSystemWatcher` on `Assets/**/*.cs` with debounced `AssetDatabase.Refresh()` and per-assembly compile log at `<ProjectRoot>/Logs/WkCore.log`.
- EditMode test suite covering path utility, palette / lazy GUIStyle init, logger prefixing, avatar root lookup, humanoid side map degenerate paths, and FBX mesh utility passthrough.
- VPM package metadata (`package.json`), Editor and Tests asmdefs, GPL-3.0-or-later license, GitHub Actions release / CI / changelog-append / wiki-sync workflows mirroring the convention used in the consumer repos.
