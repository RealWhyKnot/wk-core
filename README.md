# WK Core

[![License: GPLv3](https://img.shields.io/badge/license-GPLv3-blue.svg)](LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2022.3-000000.svg?logo=unity)](https://unity.com/)

Shared Editor-only utilities for the WhyKnot VRChat tools. Installed automatically as a dependency of [vrc-avatar-qol](https://github.com/RealWhyKnot/vrc-avatar-qol) and [vrcfury-qol](https://github.com/RealWhyKnot/vrcfury-qol).

Most users will never interact with this package directly -- VCC pulls it in when you install one of the consumer packages.

## What's inside

- **`WhyKnot.Core.Styling.WkStyles`** -- IMGUI palette (`ColorAccent`, `ColorWarning`, `ColorSuccess`, `ColorInfo`, `ColorDivider`), lazy-init `GUIStyle` typography (`SectionTitle`, `Body`, `Muted`, `Mono`, `PrimaryButton`, `BadgePillStyle`, ...), and widget primitives (`Section`, `Notice`, `BadgePill`, `Divider`, `LabeledField`, `PrimaryButtonInline`, `HelpIcon`). Includes the `NoticeKind` enum (Info / Warning / Success).
- **`WhyKnot.Core.Utilities.PathUtility`** -- `GetGameObjectPath(GameObject)` slash-joined hierarchy path.
- **`WhyKnot.Core.Utilities.AvatarUtility`** -- `FindAvatarRoot(Component)` with Animator-then-scene-root fallback.
- **`WhyKnot.Core.Utilities.FbxMeshUtility`** -- `ResolveEditableMesh(renderer, mesh, suffix, undoLabel, folder)` detects FBX/OBJ/DAE/glTF sub-asset meshes, clones to a writable .asset, rewires the renderer, registers Undo for both the asset creation and the property change.
- **`WhyKnot.Core.Utilities.HumanoidSideMap`** -- Maps Transforms in an avatar's bone hierarchy to Left / Right / Center / Unknown via Humanoid bone ancestry. Computes the left-axis sign from actual `LeftUpperLeg` position rather than coordinate-system assumption.
- **`WhyKnot.Core.Utilities.EditorLogger`** -- Construct once with a `[Prefix]`, every emitted line carries the prefix.
- **`WhyKnot.Core.Reflection.EditorElementWalker`** -- `FindInspectorContent`, `EnumerateEditorWrappers`, `TryGetEditorTarget` for walking UIElements `EditorElement`/`InspectorElement` trees by name (the public concrete types live in internal Editor assemblies). Plus `ApplyBannerChromeStyle`, `ApplyInlineButtonStyle`, `ApplyDangerButtonStyle` for inspector overlay chrome.
- **`WhyKnot.Core.HotReload.EditorHotReload`** -- `[InitializeOnLoad]` `FileSystemWatcher` on `Assets/**/*.cs` that triggers `AssetDatabase.Refresh()` even when Unity is unfocused. Per-assembly compile summary + errors appended to `<ProjectRoot>/Logs/WkCore.log` (rolls over at 512 KB).

## Installation

You normally don't install this manually. Adding [vrc-avatar-qol](https://github.com/RealWhyKnot/vrc-avatar-qol) or [vrcfury-qol](https://github.com/RealWhyKnot/vrcfury-qol) through VCC pulls in `dev.whyknot.core` automatically because both list it as a `vpmDependencies` entry.

If you do need it standalone -- e.g. building a third-party package against the same shared surface -- add the WhyKnot VPM listing to VCC and pick **WK Core** from the package list:

1. <https://vpm.whyknot.dev/> -- the page redirects to a `vcc://` handler URL and VCC opens with the listing pre-filled.
2. Or in VCC: **Settings -> Packages -> Add Repository**, paste `https://vpm.whyknot.dev/index.json`.

Compiles into a `dev.whyknot.core.Editor` Editor-only assembly. No runtime code ships.

## Building a downstream package

To depend on this from another VPM package, add to the consumer's `package.json`:

```json
"vpmDependencies": {
  "dev.whyknot.core": ">=1.0.0"
}
```

And to the consumer's Editor `.asmdef`:

```json
"references": [
  "dev.whyknot.core.Editor"
]
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

Licensed under the GNU General Public License v3.0 or later. See [LICENSE](LICENSE) for the full text.
