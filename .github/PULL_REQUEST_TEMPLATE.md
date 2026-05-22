<!-- Linked issue (optional but appreciated): Closes #__ -->

## Summary

<!-- 1-3 sentences on the *why*, not just the what. The diff already shows the what. -->

## Checklist

- [ ] Compiles in Unity 2022.3.x with no console errors or warnings introduced by this change.
- [ ] Tests added or updated in `Tests/Editor/` and pass via EditMode Test Runner.
- [ ] If a public helper signature changed, downstream call sites in vrc-avatar-qol / vrcfury-qol still compile.
- [ ] No `Debug.Log` debug noise left behind (use `EditorLogger` for keeper diagnostics).

## Downstream impact

<!-- If this PR changes a public surface, list which downstream packages need a coordinated bump and why. -->
