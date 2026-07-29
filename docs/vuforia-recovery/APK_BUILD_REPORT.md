# Android development APK report

Date: 2026-07-29  
Unity: `2019.4.41f1`  
Build method: `AndroidBuildPipeline.BuildDevelopmentApk`

## Result

- Status: **SUCCEEDED**
- Unity exit code: `0`
- Build report errors: `0`
- C# errors: `0`
- Output: `Builds/Android/RealidadeA-development.apk`
- File size: `62793230` bytes
- SHA-256: `B9D5D36F208100DA259416043E9FF44ADF05BA5D7ED5741DEED2D76147F43677`
- Unity build duration: `00:02:20.2165395`
- Development/debuggable: yes

The APK and build log are intentionally ignored by Git.

## Package inspection

Inspection tool: Android SDK Build Tools `30.0.2/aapt.exe`

- Package ID: `com.AlfaCompany.RAAlfabeto`
- Version name: `0.2.0`
- Version code: `200`
- Minimum SDK: `19`
- Target SDK: `30`
- Compile SDK: `30`
- Launch activity: `com.unity3d.player.UnityPlayerActivity`
- Architecture: `armeabi-v7a`

`armeabi-v7a` is a valid Unity 2019/Vuforia 8.3.8 Android development target. The APK contains:

- `lib/armeabi-v7a/libVuforia.so`
- `lib/armeabi-v7a/libVuforiaUnityPlayer.so`
- `lib/armeabi-v7a/libVuforiaWrapper.so`
- `assets/bin/Data/Managed/Vuforia.UnityExtensions.dll`
- `assets/bin/Data/Managed/VuforiaScripts.dll`
- the `Alfabeto` and `RealidadeA` Vuforia datasets

## Scene validation

The build log explicitly records:

- `Opening scene 'Assets/Scenes/Menu.unity'`
- `Loaded scene 'Assets/Scenes/Menu.unity'`
- `Opening scene 'Assets/Scenes/SampleScene.unity'`
- `Loaded scene 'Assets/Scenes/SampleScene.unity'`

`Menu.unity` was passed first by `ProjectValidation.GetRequiredScenes()`, followed by `SampleScene.unity`. Both scene hashes remained equal to the preflight baselines after the build.

Unity's Android target switch transiently upgraded/serialized `VuforiaConfiguration.asset`, `GraphicsSettings.asset`, and `ProjectSettings.asset`. Those incidental working-tree changes were discarded after the successful artifact inspection; the build pipeline's version and target configuration remains reproducible from code.
