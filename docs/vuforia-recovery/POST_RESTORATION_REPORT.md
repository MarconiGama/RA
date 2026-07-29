# Post-restoration compile report

Date: 2026-07-29  
Unity: `2019.4.41f1`  
Log: `Builds/Vuforia/post-restoration-compile.log` (ignored by Git)

## Result

- Unity batch exit code: `0`
- C# compile errors: `0`
- Missing Vuforia assembly/API errors: `0`
- Duplicate plugin/assembly errors: `0`
- Namespace conflicts: `0`
- Android plugin import errors: `0`

The restored managed API compiled the unchanged Vuforia 8.3.8 legacy scripts. Native plugins were imported using the official platform-specific `.meta` settings, so only the appropriate managed/native variant is enabled per target.

## Scene and package integrity

- `Menu.unity` exists, is enabled, and remains first in Build Settings.
- `SampleScene.unity` exists and is enabled second.
- `Menu.unity` SHA-256 after compile: `10FF8686EE7981599C7D15E4B721D38D49B0F9B9D95AD3CC10D9A77B2575B4AE`
- `SampleScene.unity` SHA-256 after compile: `042393D421736CF73E71A23C44175AB7AE0EC19E66628A6B4A7503F0F525A09E`
- Scene hash changes from preflight: `0`
- `Packages/manifest.json` changes from preflight: `0`
- Existing Vuforia scripts replaced: `0`
- Existing Vuforia GUIDs replaced: `0`

Serialized Image Target references remain in `SampleScene.unity`; their managed assembly GUID is present in the restored official plugin metadata. Full scene serialization and Android inclusion are additionally exercised by the development build phase.

The phase-6 gate passed, so the alphabet import pipeline may proceed.
