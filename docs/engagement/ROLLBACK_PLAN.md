# Sprint E1 rollback plan

Sprint E1 is isolated from the recovered baseline. `Menu.unity`, `SampleScene.unity`, the original Image Target database, Vuforia content, package manifests, existing GUIDs and legacy `RaA.blend`–`RaZ.blend` sources are not changed. The baseline APK is preserved separately and the pilot uses only `SampleScene_EngagementPilot.unity`.

## Immediate operational rollback

1. Stop distributing `Builds/Android/RealidadeA-engagement-pilot.apk`.
2. Continue using the preserved `Builds/Android/RealidadeA-development.apk` with SHA-256 `B9D5D36F208100DA259416043E9FF44ADF05BA5D7ED5741DEED2D76147F43677`.
3. Do not add `EngagementSandbox.unity` or `SampleScene_EngagementPilot.unity` to the baseline build method.

This rollback requires no scene, database or Vuforia migration.

## Source rollback

The work is contained in ten ordered commits after base `6eebad083789dd1041009fc81ea5264cf77e2043`. If source removal is required, create a dedicated rollback branch and revert the engagement commits in reverse order with ordinary `git revert`, review the resulting diff, compile and run the protected-hash checks. Do not use destructive reset, clean, force push or delete the preserved baseline artifacts.

For a partial rollback, the safest seam is the pilot integration commit: removing the copied pilot scene and its adapter does not alter the legacy `SampleScene.unity`. Content schema 2.0 is backward compatible, so enriched fields may remain while runtime engagement is disabled.

## Verification after rollback

- `Menu.unity` SHA-256 remains `10FF8686EE7981599C7D15E4B721D38D49B0F9B9D95AD3CC10D9A77B2575B4AE`.
- `SampleScene.unity` SHA-256 remains `042393D421736CF73E71A23C44175AB7AE0EC19E66628A6B4A7503F0F525A09E`.
- Baseline APK SHA-256 matches the value above.
- Unity compiles with zero errors and the legacy A–Z experience still opens from the baseline scenes.
