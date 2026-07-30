# Sprint E1 test report

Date: 2026-07-29  
Unity: `2019.4.41f1`

## Compilation

- Batch compile errors: 0
- Missing Vuforia assemblies: 0
- Duplicate assemblies/plugins: 0

## EditMode

Results: `Builds/Tests/engagement-editmode-results.xml` (ignored by Git)  
Log: `Builds/Tests/engagement-editmode.log` (ignored by Git)

- Total: 18
- Passed: 18
- Failed: 0
- Skipped: 0
- Inconclusive: 0

Coverage includes schema 1.0 compatibility, schema 2.0, preserved A–Z identities, complete A configuration, B–Z fallback, missing optional media, Calm default, legacy progress, bounded non-personal telemetry, original scene hashes, standalone sandbox and pilot target-A integrity.

## PlayMode

Results: `Builds/Tests/engagement-playmode-results.xml` (ignored by Git)  
Log: `Builds/Tests/engagement-playmode.log` (ignored by Git)

- Total: 8
- Passed: 8
- Failed: 0
- Skipped: 0
- Inconclusive: 0

Passing scenarios:

1. Complete local A flow.
2. TargetLost during narration cancels audio and experience.
3. TargetLost while waiting for interaction cancels interaction.
4. Replay is counted and narration does not overlap.
5. Tap cannot complete twice.
6. Hint appears after configured inactivity.
7. Reduced Motion selects `ReducedMotionResponse`.
8. Exit stops all audio and interaction.

The Unity/Vuforia test runner transiently created `QCAR/somedata16` and serialized the legacy CLR version `0.0` into `VuforiaConfiguration.asset`. Both incidental outputs were discarded after results capture; protected Vuforia content remains at its baseline.
