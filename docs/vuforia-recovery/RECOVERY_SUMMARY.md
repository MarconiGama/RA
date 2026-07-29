# Vuforia 8.3.8 recovery summary

Date: 2026-07-29  
Repository: `C:/Projetos/RA`  
Branch: `agent/vuforia-8-3-8-recovery`  
Base: `d1e935b654ab927c94277d96c0f56472f7d5d4f4`

## Outcome

Status: **RECOVERED_AND_VALIDATED**

The exact Vuforia 8.3.8 binary/API layer was recovered from the official Unity registry distribution, correlated with the original project history, restored without replacing the legacy source layer, compiled in Unity 2019.4.41f1, validated through the A-Z import pipeline and EditMode tests, and packaged into a development APK.

## Vuforia provenance

- Expected engine version: `8.3.8`
- Official distribution: `com.ptc.vuforia.engine@8.3.9`
- Internal engine version: `8.3.8:51eeb9905dc6614d47b5575149be374f`
- Official tarball SHA-256: `7F79E8C04F1A20F1C0CA5EB8BCBC5E78A7D41BEEEBFA2745209841B656D1A190`
- Historical/official Android DLL SHA-256: `8A3E9D6A097DADAFFF726CE796E1F2C9E26C7A5ECDA6515560AE28E1CBEDE12D`
- Original Git evidence: commit `31f48d98ee8a38859510007f39dea5400f5fc6eb`, blob `1be6e7b948e5a1045d10162c4603c49523c030b5`
- Official scripts matching project scripts: 7/7
- Official script/asmdef GUIDs matching project GUIDs: 8/8
- Vuforia 8.5/10/11 installed: no

## Verification matrix

| Gate | Result |
| --- | --- |
| Compile errors before | 44 `CS0246` occurrences / 22 unique |
| Compile errors after | 0 |
| Missing Vuforia assemblies after | 0 |
| Duplicate plugins/namespace conflicts | 0 |
| Alphabet importer | passed, 26/26 |
| Alphabet FBX/materials/prefabs | 26 / 26 / 26 |
| Preserved legacy `.blend` models | 26 |
| EditMode tests | 6 passed, 0 failed, 0 skipped |
| Menu scene loaded by Android build | yes, first |
| SampleScene loaded by Android build | yes, second |
| Scene files modified | no |
| Existing Vuforia scripts replaced | no |
| Existing GUIDs replaced | no |
| Development APK | succeeded |

## APK

- Path: `Builds/Android/RealidadeA-development.apk`
- Size: `62793230` bytes
- SHA-256: `B9D5D36F208100DA259416043E9FF44ADF05BA5D7ED5741DEED2D76147F43677`
- Architecture: `armeabi-v7a`
- Development/debuggable: yes
- APK tracked by Git: no

## Scene integrity

- `Menu.unity`: `10FF8686EE7981599C7D15E4B721D38D49B0F9B9D95AD3CC10D9A77B2575B4AE`
- `SampleScene.unity`: `042393D421736CF73E71A23C44175AB7AE0EC19E66628A6B4A7503F0F525A09E`
- Both hashes equal the preflight baselines.

## Commit sequence

1. `b170749` — `docs: inventory legacy Vuforia integration`
2. `0872803` — `fix: restore compatible Vuforia runtime`
3. `5c66498` — `test: validate Unity project with Vuforia`
4. `17b7c85` — `build: produce Android development baseline`
5. `docs: document Vuforia recovery` — this consolidated handoff

`Library`, `Temp`, `Logs`, `Builds`, APKs, caches, keystores and credentials are not versioned.

## Next safe step

Install the development APK on an ARMv7-compatible Android test device, grant camera permission, and smoke-test recognition against the preserved `Alfabeto` targets. Device deployment was outside this workspace-only recovery.
