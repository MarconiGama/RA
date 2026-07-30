# E1 Android pilot menu route blocker

Date: 2026-07-30

## Classification

- Severity: **BLOCKER**
- Code: `MENU_PILOT_SCENE_ROUTE_MISMATCH`
- Status at discovery: confirmed on a physical Android device

## Symptom

The engagement pilot APK opens `Menu` correctly, but pressing Play does not open the camera/AR scene. The menu remains unable to reach the experience.

## Confirmed cause

`Assets/Scenes/Menu.unity` serializes `SceneChange.sceneName` as `SampleScene`, and `ButtonPlay.OnClick` invokes `SceneChange.sChange`. The E1 pilot APK contains `Menu.unity` and `SampleScene_EngagementPilot.unity`, but does not contain `SampleScene`. `SceneManager.LoadScene("SampleScene")` therefore requests a scene absent from the pilot build.

## Affected device and artifact

- Manufacturer: Motorola
- Model: motorola edge 50 fusion
- Android: 16 / API 36
- Supported ABIs: `arm64-v8a, armeabi-v7a, armeabi`
- Affected APK: `Builds/Android/RealidadeA-engagement-pilot.apk`
- Affected APK SHA-256: `4EE9A48947DF4BB4553E7129C89833E360E1E08A7B45E6C81EC2BBC67B1EEC6F`

## Reproduction

1. Install the affected E1 pilot APK.
2. Launch package `com.AlfaCompany.RAAlfabeto`.
3. Wait for `Menu` to open.
4. Press Play.
5. Observe that the pilot camera scene does not become active.

## Impact

The packaged A de Arara experience is unreachable through the normal user path. Sandbox and direct scene validation do not mitigate the release blocker because the installed APK must navigate from its menu.

## Chosen correction

Create `Menu_EngagementPilot.unity` as a Unity-managed copy of the original menu and change only its `SceneChange.sceneName` to `SampleScene_EngagementPilot`. Preserve `ButtonPlay.OnClick -> SceneChange.sChange`. Add build-profile route validation, a backward-compatible runtime route guard, navigation tests and a separate `0.3.1-e1` QA APK. The original menu, original `SampleScene`, existing E1 pilot scene and both prior APKs remain unchanged.
