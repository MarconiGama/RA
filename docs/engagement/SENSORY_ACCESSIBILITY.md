# Sensory accessibility

## Default

Calm is always the first-run profile. It enables captions, keeps ambient audio off, uses restrained SFX, avoids particles, admits one main animation at a time and leaves autoplay narration off. Reduced Motion is available as an independent local preference.

Balanced enables automatic narration and moderate effects. Expressive permits richer motion and effects, but still prohibits flashing, aggressive audio, overlapping narration and abrupt camera movement.

## Individual controls

Narration, SFX, captions, Reduced Motion and autoplay can each be changed independently. Narration/SFX/Ambient volumes are clamped to 0–1 and persisted in `PlayerPrefs`. Ambient remains disabled in all supplied profiles; enabling it is an explicit future/user action, never an automatic default.

Colour is not the sole status signal. Captions and labelled controls provide redundant textual information. Missing audio never removes the caption or blocks progress.
