# Candidate Vuforia artifacts

Date: 2026-07-29

## Search scope

The search covered the complete Git history, local branches/tags, `C:/Projetos`, the pre-opening backup `C:/Projetos/RA-backup-antes-primeira-abertura`, Downloads, Desktop, Documents, Unity Package Manager caches, Asset Store caches, project archives, and the official Unity package registry.

## Accepted candidate

Source: official Unity Package Manager registry  
Registry package: `com.ptc.vuforia.engine@8.3.9`  
Official URL: `https://download.packages.unity.com/com.ptc.vuforia.engine/-/com.ptc.vuforia.engine-8.3.9.tgz`  
Tarball size: `149240669` bytes  
Tarball SHA-256: `7F79E8C04F1A20F1C0CA5EB8BCBC5E78A7D41BEEEBFA2745209841B656D1A190`  
Registry SHA-1: `ab8311fc1c6908dafe1a396850540b1feeaedb58`

Evidence that this is the exact compatible Vuforia SDK rather than a later runtime:

1. The official package internal version file is `8.3.8:51eeb9905dc6614d47b5575149be374f`.
2. Its Android `Vuforia.UnityExtensions.dll` is `352256` bytes with SHA-256 `8A3E9D6A097DADAFFF726CE796E1F2C9E26C7A5ECDA6515560AE28E1CBEDE12D`.
3. Git commit `31f48d98ee8a38859510007f39dea5400f5fc6eb` preserves the same DLL as blob `1be6e7b948e5a1045d10162c4603c49523c030b5`; the extracted blob has the identical size and SHA-256.
4. The pre-opening backup contains that historical player DLL at `Library/PlayerDataCache/Android/Data/Managed/Vuforia.UnityExtensions.dll`.
5. Seven of seven package C# source files are byte-identical to `Assets/Vuforia`; all eight source/asmdef GUIDs match.

The package's `8.3.9` number is therefore an official package/distribution revision whose contained Engine SDK is exactly 8.3.8.

## Rejected candidates

- Local Unity cache `com.ptc.vuforia.engine@8.5.9`: rejected because the contained SDK/API is 8.5.9, not the requested 8.3.8.
- Registry versions 8.0.101, 8.1.9, 8.1.12, 8.5.8 and 8.5.9: rejected as different SDK revisions.
- Vuforia 10/11 downloads: rejected by the recovery constraints and incompatible API generation.

No DLL or unitypackage from an unofficial site was downloaded or used.
