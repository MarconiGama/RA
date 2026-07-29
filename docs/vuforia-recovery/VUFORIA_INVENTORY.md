# Legacy Vuforia inventory

Date: 2026-07-29  
Audit state: before restoration

## Identified integration

`Assets/Vuforia/version` identifies `8.3.8:0d3b9c072b133f77cada5745a61666b1`. The project contains the Vuforia 8.3.8 open-source scripts, editor initializer, materials, shaders, prefabs, fonts, emulator database and preserved `.meta` files. The namespace expected by these sources is `Vuforia`.

The seven C# source files in the official Unity package `com.ptc.vuforia.engine@8.3.9` are byte-identical to the project copies. All eight script/assembly-definition `.meta` GUIDs also match the official package. The package label is a distribution revision: its internal `Vuforia/Editor/EditorResources/version` is `8.3.8:51eeb9905dc6614d47b5575149be374f`.

## Missing binary layer at audit time

No Vuforia `.dll`, `.aar`, `.so` or `.jar` existed in tracked project content. `Packages/manifest.json` did not declare `com.ptc.vuforia.engine`. Consequently, the project was missing:

- managed Vuforia runtime API assemblies;
- managed Vuforia editor assembly;
- native Windows Editor wrapper;
- Android managed assembly and native wrapper AAR.

No unrelated Vuforia unitypackage or binary was found in `Assets/Plugins`, `Assets/Editor`, `Packages`, or `ProjectSettings`.

## Expected legacy API surface

The sources and serialized content require the Vuforia 8.x API, including:

- `VuforiaMonoBehaviour`, `VuforiaUnity`, `VuforiaRuntime`, and `VuforiaConfiguration`;
- `TrackableBehaviour` and `ITrackableEventHandler`;
- `TargetFinder` and `IObjectRecoEventHandler`;
- internal bridge contracts `IUnityCompiledFacade`, `IUnityRenderPipeline`, and `IUnityAndroidPermissions`;
- serialized `ImageTargetBehaviour` and Vuforia camera behaviour types from `Vuforia.UnityExtensions.dll`.

No project source reference to the Vuforia 10/11 `ObserverBehaviour` API was found. This rules out a modern API substitution.

## Assembly definitions

- `Assets/Vuforia/Scripts/VuforiaScripts.asmdef`: assembly `VuforiaScripts`, auto-referenced, no explicit precompiled references.
- `Assets/Vuforia/Editor/Scripts/VuforiaEditorScripts.asmdef`: editor-only assembly referencing `VuforiaScripts`.
- Both assembly-definition GUIDs match the official 8.3.8-content package.

## Scene reference audit

- `Menu.unity` and `SampleScene.unity` were not edited during the audit.
- `SampleScene.unity` retains the GUID `bab6fa851cf5a1a4bba3cec5f191cb8e`, which is an official Vuforia 8.3.8 managed plugin GUID.
- It also retains the GUIDs for `DefaultTrackableEventHandler`, `DefaultInitializationErrorHandler`, and all A-Z image-target textures.
- `EditorBuildSettings.asset` enables `Menu.unity` first and `SampleScene.unity` second.
- The apparent `f70555...` reference in `Menu.unity` belongs to Unity UI serialized component types; it is not a Vuforia binary candidate.

The complete machine-readable pre-restoration inventory is in `vuforia-files.json`; textual source references are in `vuforia-references.txt`.
