# Engagement pilot APK build report

Date: 2026-07-29  
Unity: `2019.4.41f1`  
Method: `AndroidBuildPipeline.BuildEngagementPilotApk`

## Result

- Status: **SUCCEEDED**
- Unity exit code: 0
- C# errors: 0
- Build errors: 0
- Relevant warnings: 1
- Output: `Builds/Android/RealidadeA-engagement-pilot.apk`
- Actual APK size: `62906613` bytes
- SHA-256: `4EE9A48947DF4BB4553E7129C89833E360E1E08A7B45E6C81EC2BBC67B1EEC6F`
- Unity build duration: `00:01:03.7176645`
- Unity build-report total size metric: `313362673` bytes

The sole warning is the pre-existing deprecated `ModelImporter.optimizeMesh` use in `AlphabetModelImporter.cs`; the new engagement importer emits no warnings.

## Android package

- Package ID: `com.AlfaCompany.RAAlfabeto`
- Version: `0.3.0-e1`
- Version code: `210`
- Development/debuggable: yes
- Minimum SDK: 19
- Target/compile SDK: 30
- Architecture: `armeabi-v7a`
- Vuforia native libraries: `libVuforia.so`, `libVuforiaUnityPlayer.so`, `libVuforiaWrapper.so`

## Scenes

The build log explicitly opened and loaded, in order:

1. `Assets/Scenes/Menu.unity`
2. `Assets/Scenes/SampleScene_EngagementPilot.unity`

`SampleScene.unity` was not included or modified. `EngagementSandbox.unity` remains outside the principal/pilot build lists.

## Baseline preservation

- Baseline APK: `Builds/Android/RealidadeA-development.apk`
- Baseline SHA-256 before: `B9D5D36F208100DA259416043E9FF44ADF05BA5D7ED5741DEED2D76147F43677`
- Baseline SHA-256 after: `B9D5D36F208100DA259416043E9FF44ADF05BA5D7ED5741DEED2D76147F43677`
- Baseline changed: no
- Pilot incremental APK size: `113383` bytes (`0.18%`)

The APK, logs and build intermediates remain ignored by Git. Incidental Unity Android/Vuforia configuration serialization was discarded after successful inspection.
