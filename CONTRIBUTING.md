# Contributing

`wk-core` is a small Unity Editor utility package shared between the WhyKnot VRChat tools. It is installed automatically as a dependency of [vrc-avatar-qol](https://github.com/RealWhyKnot/vrc-avatar-qol) and [vrcfury-qol](https://github.com/RealWhyKnot/vrcfury-qol); most users will never interact with it directly.

Bug reports and PRs are welcome.

## What lives here

- IMGUI styling primitives (`WkStyles`, palette, notice colors).
- Path/mesh/avatar helpers (`PathUtility`, `FbxMeshUtility`, `AvatarUtility`, `HumanoidSideMap`).
- UIElements walkers for the custom-inspector overlay pattern (`EditorElementWalker`).
- Hot-reload `FileSystemWatcher` + compile-error log (`EditorHotReload`).
- Prefixed `Debug.Log` wrapper (`EditorLogger`).

Domain logic -- PhysBone math, mesh-fix pipelines, VRCFury-specific reflection -- stays in the downstream packages.

## Setting up the dev loop

**Prerequisites:** Unity 2022.3.x, git.

Iterate on this package by deploying it directly into a Unity avatar project:

```powershell
scripts\deploy-to-local.ps1 -AvatarPath C:\Path\To\YourAvatar
```

That mirrors the package contents into `<AvatarPath>\Packages\dev.whyknot.core\` using the same exclusion list as the release zip, so the deployed tree is byte-equivalent to what VCC ships.

## Tests

`Tests/Editor/` is an EditMode NUnit suite. Run via **Window -> General -> Test Runner -> EditMode -> Run All**.

## Submitting a PR

- Branch from `main`. Open PRs against `main`.
- The [PR template](.github/PULL_REQUEST_TEMPLATE.md) auto-populates the description.
- Touched behavior? Update or add tests in `Tests/Editor/`.
- Keep PRs focused; mixing unrelated changes makes review harder.

## Commit message style

- Conventional-ish prefixes are appreciated: `feat:`, `fix:`, `docs:`, `chore:`, `refactor:`, `ci:`.
- Subject <=72 characters.
- The body is for the *why*. The diff shows the *what*.

## Reporting security issues

Please don't file a public issue for a security vulnerability. Use GitHub's **Security tab -> Report a vulnerability** for a private disclosure. See [SECURITY.md](.github/SECURITY.md).
