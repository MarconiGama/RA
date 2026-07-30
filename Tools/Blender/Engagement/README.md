# Engagement graybox pipeline

Run `build-engagement-grayboxes.ps1` with Blender 2.93.18. It creates original, low-complexity, clearly named placeholder sources and FBX exports for Lumi and the arara.

The generated files are grayboxes, not final art. Each character uses at most two flat materials and stores the requested named actions in its `.blend`. Runtime motion remains controlled by `PlaceholderMotionController`, so Reduced Motion and future production asset replacement do not change the experience state machine.
