# Sprint E1 performance report

Date: 2026-07-29
Unity: `2019.4.41f1`

## Sandbox probe

`EngagementPerformanceRunner.RunSandboxProbe` opened `Assets/Scenes/EngagementSandbox.unity` and sampled 120 Play Mode updates in Unity Editor batch mode with the null graphics device. The raw result remains at `Builds/Engagement/sandbox-performance.json` and is ignored by Git.

| Metric | Observed |
| --- | ---: |
| Scene open | 168.82 ms |
| Sample duration | 2.100 s |
| Average update rate | 57.13 FPS |
| Slowest observed update | 103.96 ms |
| Profiler allocated memory at start | 98,609,880 bytes (94.04 MiB) |
| Profiler allocated memory at end | 98,604,528 bytes (94.04 MiB) |
| Net allocated-memory change | -5,352 bytes (-5.23 KiB) |

The average exceeds the 30 FPS integration target. The slowest update includes Editor/batch-mode startup effects and is not a device frame-time guarantee. These values are an automated regression reference, not an Android GPU/thermal measurement; profiling on the minimum supported physical device remains required before production release.

## Content budgets

| Asset | Vertices | Materials | Textures | Source FBX size |
| --- | ---: | ---: | ---: | ---: |
| `PLACEHOLDER_Lumi` | 3,048 | 2 | 0 | 137,372 bytes |
| `PLACEHOLDER_Arara` | 1,216 | 2 | 0 | 81,420 bytes |
| Combined grayboxes | 4,264 | 4 | 0 | 218,792 bytes (0.21 MiB) |

Both models remain below the 15,000-vertex individual budget. They use flat Standard materials, no texture memory, cloth, particles, physical hair or runtime simulation.

## Runtime safeguards

- No `Resources.Load` occurs per frame; content and audio resources are cached after first access.
- No `FindObjectOfType`, LINQ or continuous `Instantiate` occurs in critical update loops.
- The active-instance invariant limits runtime to one experience, one Lumi and one arara.
- Narration uses one channel and is stopped on target loss; SFX is separate and Ambient defaults off.
- Motion uses bounded coroutines and a shorter `ReducedMotionResponse`; cancellation prevents orphaned work.
- The sampled run showed no continuous relevant allocation growth.

## APK impact

- Baseline APK: 62,793,230 bytes.
- Engagement pilot APK: 62,906,613 bytes.
- Increment: 113,383 bytes, or 0.18%.

Status: **PASS for Sprint E1 automated integration target; physical-device profiling pending before production art/audio rollout.**
