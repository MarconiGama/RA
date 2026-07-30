# Sprint E1 preflight

Date: 2026-07-29  
Status: **PASS**

## Repository gate

- Working directory: `C:/Projetos/RA`
- Required branch: `agent/engagement-learning-loop`
- HEAD: `6eebad083789dd1041009fc81ea5264cf77e2043`
- Required base: `6eebad083789dd1041009fc81ea5264cf77e2043`
- Worktree: clean
- Remote branch at gate: `origin/agent/engagement-learning-loop` at the same base commit

## Runtime gate

- Unity executable: `C:/Program Files/Unity/Hub/Editor/2019.4.41f1/Editor/Unity.exe`
- Project Unity version: `2019.4.41f1 (fb553f8fdd6c)`
- Batch compile exit code: `0`
- C# errors: `0`
- Missing Vuforia assemblies: `0`
- Vuforia internal version: `8.3.8`
- Android Vuforia managed DLL SHA-256: `8A3E9D6A097DADAFFF726CE796E1F2C9E26C7A5ECDA6515560AE28E1CBEDE12D`

## Preserved baselines

- Alphabet FBX: 26
- Alphabet prefabs: 26
- Legacy `RaA.blend`–`RaZ.blend`: 26
- Baseline APK: present, `62793230` bytes
- Baseline APK SHA-256: `B9D5D36F208100DA259416043E9FF44ADF05BA5D7ED5741DEED2D76147F43677`
- `Menu.unity` SHA-256: `10FF8686EE7981599C7D15E4B721D38D49B0F9B9D95AD3CC10D9A77B2575B4AE`
- `SampleScene.unity` SHA-256: `042393D421736CF73E71A23C44175AB7AE0EC19E66628A6B4A7503F0F525A09E`

The fail-closed gate passed. Protected scenes, Vuforia content, package manifests, original image-target database, GUIDs and legacy models may not be changed by Sprint E1.
