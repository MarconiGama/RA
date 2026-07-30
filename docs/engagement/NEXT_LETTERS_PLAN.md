# Next letters rollout plan

Sprint E1 completes only A de Arara. The following letters are candidates for staged reuse of the same content schema, state machine, sensory controls, caption/audio channels, interaction coordination and local telemetry. None is fully implemented in this sprint.

| Order | Letter | Word/content | Proposed calm interaction | Production assets needed |
| ---: | --- | --- | --- | --- |
| 1 | B | Boto | Tap for one short, low arc above calm water | Original boto model, water response, four reviewed narration cues, subtle original/cleared SFX |
| 2 | J | Jacaré | Tap for a slow blink and small tail movement | Original jacaré model, reduced-motion blink, narration, subtle SFX |
| 3 | M | Macaco | Tap for one predictable wave | Original macaco model, wave/idle/reduced-motion states, narration, subtle SFX |
| 4 | O | Onça | Tap for a calm greeting; reuse Lumi only if pedagogically reviewed | Reviewed onça treatment, distinct content/companion roles, narration, subtle SFX |
| 5 | U | Uirapuru | Tap for a short hop between two fixed perches | Original bird model, fixed path and reduced-motion response, narration, licensed/original SFX |

## Per-letter gate

Each letter should proceed as its own vertical slice:

1. Confirm the existing ID and target remain unchanged.
2. Obtain pedagogical and accessibility review for word, syllables, phrases and interaction.
3. Add schema-2 configuration while keeping the basic fallback valid.
4. Produce original or documented-license assets within the existing vertex/material/texture budgets.
5. Record the four Portuguese narration cues; keep captions authoritative until licensed recordings exist.
6. Add EditMode validation and the letter's sandbox PlayMode flow, including target loss and Reduced Motion.
7. Integrate only into a copied pilot scene through the technology-neutral tracking boundary.
8. Profile on a representative low-end ARMv7 Android device and build a separate pilot APK.

Do not batch-enable B–Z, replace the baseline APK or modify the original Vuforia scene/database. After these five reviewed slices, evaluate whether the repeated configuration is stable enough for the remaining alphabet rollout.
