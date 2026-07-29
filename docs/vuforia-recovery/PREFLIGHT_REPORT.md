# Vuforia 8.3.8 recovery preflight

Date: 2026-07-29  
Status: **PASS**

## Repository gate

- Working directory: `C:/Projetos/RA`
- Authorized branch: `agent/vuforia-8-3-8-recovery`
- HEAD at gate: `d1e935b654ab927c94277d96c0f56472f7d5d4f4`
- Worktree at gate: clean
- Destructive Git operations: none

## Toolchain

- Unity executable: `C:/Program Files/Unity/Hub/Editor/2019.4.41f1/Editor/Unity.exe`
- Unity version: `2019.4.41f1`
- Android Build Support: installed
- Android SDK: installed; tools revision `26.1.1`
- Android NDK: installed; revision `19.0.5232133`
- OpenJDK: installed; Java `1.8.0`

## Project inputs

- `Assets/Scenes/Menu.unity`: present
- `Assets/Scenes/SampleScene.unity`: present
- Alphabet FBX files: 26
- Alphabet materials: 26
- Alphabet prefabs: 26
- Preserved legacy Blender models: 26
- `Packages/manifest.json`: present; no Vuforia dependency at the gate
- `Packages/packages-lock.json`: present; no Vuforia dependency at the gate

## Immutable baselines

- `Menu.unity` SHA-256: `10FF8686EE7981599C7D15E4B721D38D49B0F9B9D95AD3CC10D9A77B2575B4AE`
- `SampleScene.unity` SHA-256: `042393D421736CF73E71A23C44175AB7AE0EC19E66628A6B4A7503F0F525A09E`
- `Packages/manifest.json` SHA-256: `E483F97364FC7E0475AE37C6FBB352C1A525B53DCBF8114AFB29D600B0216141`
- `Packages/packages-lock.json` SHA-256: `855ADB16B9C780A68FEFCF923D1AF85C4CB4EFE7005D1C355D17B084EDD69E2A`
- `Assets/Vuforia/version` SHA-256: `63FF221CFC2C23AD402BF25088586E04BFEAC341CA79F04901302D618ACF86BF`

The branch, base commit, worktree and required local toolchain satisfied the fail-closed gate.
