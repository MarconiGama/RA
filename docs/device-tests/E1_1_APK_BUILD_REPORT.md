# E1.1 device QA APK build report

Date: 2026-07-30

## Build result

- Method: `AndroidBuildPipeline.BuildEngagementPilotQaApk`
- Status: **SUCCEEDED**
- Unity exit code: 0
- Compile errors: 0
- Output: `Builds/Android/RealidadeA-engagement-pilot-e1.1.apk`
- Size: 62,906,922 bytes
- SHA-256: `9C7A33DF63A1D65FFE64AFC948CA0BEB366F3F3971A72BD9339E6E233EE7FED9`
- Unity build-report duration: `00:00:50.2391029`
- Unity build-report total-size metric: 313,364,290 bytes

## Package inspection

- Package ID: `com.AlfaCompany.RAAlfabeto`
- Version name: `0.3.1-e1`
- Version code: `211`
- Development/debuggable: yes
- Minimum SDK: 19
- Target/compile SDK: 30
- Native architecture: `armeabi-v7a` only
- Camera permission and required camera feature: present
- Vuforia managed assemblies, databases and ARMv7 native libraries: present

## Validated scene set

Route validation completed before `BuildPipeline.BuildPlayer`. The build log loaded exactly:

1. `Assets/Scenes/Menu_EngagementPilot.unity`
2. `Assets/Scenes/SampleScene_EngagementPilot.unity`

The build profile excludes `Menu.unity`, `SampleScene.unity` and `EngagementSandbox.unity`. The APK contains exactly two Unity level payloads (`level0` and `level1`).

## Preservation

- Baseline APK SHA-256: `B9D5D36F208100DA259416043E9FF44ADF05BA5D7ED5741DEED2D76147F43677`
- Original defective E1 pilot SHA-256: `4EE9A48947DF4BB4553E7129C89833E360E1E08A7B45E6C81EC2BBC67B1EEC6F`
- Both prior APKs remain unchanged.
- APKs, logs and build intermediates remain ignored by Git.
