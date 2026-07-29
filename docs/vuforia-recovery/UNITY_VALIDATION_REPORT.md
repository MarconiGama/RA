# Unity import and EditMode validation

Date: 2026-07-29  
Unity: `2019.4.41f1`

## Alphabet import

Command method: `AlphabetModelImporter.RunBatch`  
Log: `Builds/Art/Alphabet/unity-import-main.log` (ignored by Git)

- Unity exit code: `0`
- C# errors: `0`
- Pipeline exceptions: `0`
- Approval marker: `RA Alphabet: 26 modelos e prefabs aprovados.`
- FBX: 26
- Materials: 26
- Prefabs: 26
- Preserved legacy `.blend` files: 26
- Modified legacy models: 0
- Modified Vuforia sources: 0
- Modified scenes: 0

## EditMode tests

Results: `Builds/Tests/editmode-results.xml` (ignored by Git)  
Log: `Builds/Tests/editmode.log` (ignored by Git)

- Result: `Passed`
- Total: 6
- Passed: 6
- Failed: 0
- Skipped: 0
- Inconclusive: 0
- C# errors: 0

Passing cases:

1. `ContentRepositoryTests.Validate_AcceptsValidPack`
2. `ContentRepositoryTests.Validate_RejectsDuplicateTargets`
3. `ContentRepositoryTests.Validate_RejectsEmptyItems`
4. `ProjectValidationTests.RequiredScenes_AreStableAndOrdered`
5. `AlphabetAssetTests.Alphabet_HasCompleteNormalizedAssetSet`
6. `AlphabetAssetTests.ContentJson_ReferencesEveryGeneratedPrefab`

The Vuforia editor attempted to append `0.0` to `eulaAcceptedVersions` because the legacy managed assemblies expose CLR assembly version `0.0.0.0`. That incidental serialization was discarded; `Assets/Resources/VuforiaConfiguration.asset` remains byte-identical to the repository baseline.
