# Initial Unity compile report

Date: 2026-07-29  
Unity: `2019.4.41f1`  
Log: `Builds/Vuforia/initial-compile.log` (ignored by Git)

## Result

- Unity batch exit: failure
- `CS0246` occurrences: 44
- Unique `CS0246` diagnostics: 22
- `CS0103` occurrences: 0
- Duplicate-plugin errors: 0
- Android plugin errors: 0
- Scene mutation: none

The 22 diagnostics were emitted twice during recompilation. All are explained by the absent Vuforia managed runtime. Missing types were:

- `VuforiaMonoBehaviour`
- `VuforiaUnity`
- `IObjectRecoEventHandler`
- `TargetFinder`
- `ITrackableEventHandler`
- `TrackableBehaviour`
- `IUnityCompiledFacade`
- `IUnityRenderPipeline`
- `IUnityAndroidPermissions`

The log also contains Unity Licensing Client IPC/signature noise, but the editor proceeded through asset refresh and script compilation; those messages are not the compile failure being recovered.

No corrective project change was made before this report and the candidate search checkpoint.
