# Changelog

All notable changes to this project will be documented in this file. Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/); versioning follows [Semantic Versioning](https://semver.org/).

<!-- Entries under "## Unreleased" are appended automatically by the changelog-append GitHub
     workflow on every push to main, then promoted to the versioned section by release.yml when
     a tag is cut. Don't hand-edit Unreleased -- your edits will be overwritten on the next push.
     To override an entry, amend the commit subject before merge. -->

## Unreleased

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
