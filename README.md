# WK Core

[![License: GPLv3](https://img.shields.io/badge/license-GPLv3-blue.svg)](LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2022.3-000000.svg?logo=unity)](https://unity.com/)

Shared Editor-only utilities for the WhyKnot VRChat tools. The source of truth for the helpers bundled into [vrc-avatar-qol](https://github.com/RealWhyKnot/vrc-avatar-qol) and [vrcfury-qol](https://github.com/RealWhyKnot/vrcfury-qol) via `scripts/sync-to-downstream.ps1` -- each consumer carries its own copy under `Editor/Internal/` to avoid the VCC `>=` version-floor footgun.

Most users will never install this package directly. It is published to `vpm.whyknot.dev` for two reasons: as a discoverable source artefact for the bundled helpers, and to host the upcoming log-viewer and settings-provider UIs that live in this assembly only (not the bundled copies).

## What's inside

### Styling

- **`WhyKnot.Core.Styling.WkTheme`** -- palette + chrome that `WkStyles` emits. Each theme has a `Pro` and `Personal` `Variant` for the two editor skins. Two presets ship:
  - `WkTheme.WhyKnot` -- the brand palette (black / gray / light blue). Default.
  - `WkTheme.VRCFury` -- matches VRCFury's dark-gray row chrome and warm accents so inspector overlays sitting next to VRCFury components don't visually compete.
- **`WhyKnot.Core.Styling.WkStyles`** -- themed IMGUI palette (`ColorAccent`, `ColorWarning`, `ColorSuccess`, `ColorInfo`, `ColorDanger`, `ColorDivider`, `ColorBackground`, `ColorTextPrimary`, ...), lazy-init `GUIStyle` typography (`SectionTitle`, `Body`, `Muted`, `Mono`, `PrimaryButton`, `BadgePillStyle`, ...), and widget primitives (`Section`, `Notice`, `BadgePill`, `Divider`, `LabeledField`, `PrimaryButtonInline`, `HelpIcon`). Includes the `NoticeKind` enum (Info / Warning / Success). Wrap a tool window's OnGUI body in `using (WkStyles.Scope(WkTheme.VRCFury)) { ... }` to switch palette for the scope.

### Logging

- **`WhyKnot.Core.Logging.WkLogger`** -- per-package file logger. Each WhyKnot package builds one in its `[InitializeOnLoad]` static constructor; the constructor self-registers with `WkLoggerRegistry`. Output goes to `%LocalAppData%/WhyKnot/Logs/<package-id>/session-<timestamp>.log` (machine-wide, project-independent). Levels: `Debug`, `Info`, `Warning`, `Error`, plus `Exception(ex, context)` that dumps the stack. Each line carries timestamp, level tag, source file:line, calling method, and message (via `[CallerMemberName]` / `[CallerFilePath]` / `[CallerLineNumber]`). Per-level Unity-console mirror is configurable; Info/Warning/Error mirror by default, Debug stays file-only.
- **`WhyKnot.Core.Logging.WkLoggerRegistry`** -- process-wide lookup; throws on `Get(packageId)` for unregistered packages so a missing registration is loud.
- **Session retention:** each package keeps at most 3 session log files. With three WhyKnot packages registered, that's a rolling 9 files total no matter how many Unity projects the user switches between.
- **`WhyKnot.Core.Logging.WkCoreLogger`** -- wk-core's own registered logger. Anything inside this package routes through `WkCoreLogger.Instance`.

### Utilities

- **`WhyKnot.Core.Utilities.PathUtility`** -- `GetGameObjectPath(GameObject)` slash-joined hierarchy path.
- **`WhyKnot.Core.Utilities.AvatarUtility`** -- `FindAvatarRoot(Component)` with Animator-then-scene-root fallback.
- **`WhyKnot.Core.Utilities.FbxMeshUtility`** -- `ResolveEditableMesh(renderer, mesh, suffix, undoLabel, folder)` detects FBX/OBJ/DAE/glTF sub-asset meshes, clones to a writable .asset, rewires the renderer, registers Undo for both the asset creation and the property change.
- **`WhyKnot.Core.Utilities.HumanoidSideMap`** -- Maps Transforms in an avatar's bone hierarchy to Left / Right / Center / Unknown via Humanoid bone ancestry. Computes the left-axis sign from actual `LeftUpperLeg` position rather than coordinate-system assumption.

### Reflection / UIElements

- **`WhyKnot.Core.Reflection.EditorElementWalker`** -- `FindInspectorContent`, `EnumerateEditorWrappers`, `TryGetEditorTarget` for walking UIElements `EditorElement`/`InspectorElement` trees by name (the public concrete types live in internal Editor assemblies). Plus `ApplyBannerChromeStyle`, `ApplyInlineButtonStyle`, `ApplyDangerButtonStyle` for inspector overlay chrome.

### Hot reload

- **`WhyKnot.Core.HotReload.EditorHotReload`** -- `[InitializeOnLoad]` `FileSystemWatcher` on `Assets/**/*.cs` that triggers `AssetDatabase.Refresh()` even when Unity is unfocused. Per-assembly compile summary + errors routed through `WkCoreLogger.Instance`.

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
  "dev.whyknot.core": ">=1.1.0"
}
```

And to the consumer's Editor `.asmdef`:

```json
"references": [
  "dev.whyknot.core.Editor"
]
```

Register a logger in your package's `[InitializeOnLoad]` static class:

```csharp
using UnityEditor;
using WhyKnot.Core.Logging;

[InitializeOnLoad]
internal static class MyPackageLogger {
    public const string PackageId = "dev.whyknot.my-package";
    public static readonly WkLogger Instance =
        new WkLogger(PackageId, "My Package", "1.0.0");
    static MyPackageLogger() { }
}
```

Then anywhere in your package:

```csharp
MyPackageLogger.Instance.Info("hello");
MyPackageLogger.Instance.Warning("something off");
MyPackageLogger.Instance.Exception(ex, "while parsing X");
```

If your tool windows render inside a third-party inspector that has its own visual language, pick the matching theme:

```csharp
using (WkStyles.Scope(WkTheme.VRCFury)) {
    WkStyles.Notice(NoticeKind.Info, "Inside VRCFury chrome");
}
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

Licensed under the GNU General Public License v3.0 or later. See [LICENSE](LICENSE) for the full text.
