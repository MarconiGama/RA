# Controlled Vuforia restoration manifest

Date: 2026-07-29  
Checkpoint commit: `b170749` (`docs: inventory legacy Vuforia integration`)

## Source and decision

The restored files come exclusively from the official Unity registry tarball `com.ptc.vuforia.engine@8.3.9`, SHA-256 `7F79E8C04F1A20F1C0CA5EB8BCBC5E78A7D41BEEEBFA2745209841B656D1A190`. Its internal SDK version is `8.3.8:51eeb9905dc6614d47b5575149be374f`. Its Android managed DLL exactly matches the Vuforia binary preserved by the original project commit `31f48d9`.

The destination `Assets/Vuforia/Plugins` and `Assets/Vuforia/Plugins.meta` did not exist immediately before restoration. No existing file or `.meta` was overwritten.

## Restored scope

Only the plugin layer needed for Windows Editor compilation, serialized managed-type resolution, and Android build was restored:

- `Assets/Vuforia/Plugins/Managed` with the official Runtime, Editor, Android, iOS and Windows Store managed variants and their `.meta`/XML files. The small platform variants preserve the original assembly GUID mappings used by serialized scenes.
- `Assets/Vuforia/Plugins/Win64/VuforiaWrapper.dll` for Windows Editor play/runtime loading.
- `Assets/Vuforia/Plugins/Android/VuforiaWrapper.aar` for Android native runtime packaging.
- Official folder and file `.meta` files from the same package.

No Vuforia script, material, prefab, shader, texture, database, configuration asset, scene or pre-existing GUID was replaced. No package dependency was added to `Packages/manifest.json`, avoiding duplicate copies of the legacy open-source scripts.

## Binary hashes

| Restored binary | Bytes | SHA-256 |
| --- | ---: | --- |
| `Managed/Android/Vuforia.UnityExtensions.dll` | 352256 | `8A3E9D6A097DADAFFF726CE796E1F2C9E26C7A5ECDA6515560AE28E1CBEDE12D` |
| `Managed/Editor/Vuforia.UnityExtensions.Editor.dll` | 378880 | `917CE370C622FAB2AA73F8D9DCAD764409A52876D9BFD8075E8E2DAB077535F5` |
| `Managed/iOS/Vuforia.UnityExtensions.dll` | 350208 | `88D5276957907D6E5F32ADE85437F7F912C7F4729AA551C128E0C3F94BDD6BEC` |
| `Managed/Runtime/Vuforia.UnityExtensions.dll` | 350208 | `325F075E9F5DEF9189DEEFC9415E334FB7502043119B4A91DA5CA737DFD04051` |
| `Managed/WindowsStoreApps/Vuforia.UnityExtensions.dll` | 351232 | `4ADA93CCD52C18051DD2571418BDE0FDF37BB2CFBE7C38836DFB9A8AA074F83C` |
| `Android/VuforiaWrapper.aar` | 32849894 | `573462C5BA0C2CA522A6D1585E0CFD6743A8B0D56A0DF21612CF7F260BE9E795` |
| `Win64/VuforiaWrapper.dll` | 20476416 | `094B8DB24807603AB87844C58A33F87BA8485F6021DDC66B7C34029B8E13BDE3` |

All 32 restored files (binaries, XML documentation, folder/file metadata) were hash-compared with the extracted official tarball after copying; mismatches: 0.
