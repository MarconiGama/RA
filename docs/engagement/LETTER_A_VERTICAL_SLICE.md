# Letter A vertical slice — A de Arara

## Scope

Sprint E1 implements one complete experience for target `A`; B–Z retain valid basic fallbacks. Lumi and the arara are original low-complexity grayboxes named with `PLACEHOLDER` until production art review.

## Sequence

1. Receive `TargetFound("A")`.
2. Hold through configurable stability time.
3. Introduce the letter A with calm motion.
4. Present Lumi.
5. Caption “Esta é a letra A.” and play narration if a licensed clip exists.
6. Caption “A de arara.”
7. Reveal the arara.
8. Prompt “Toque na arara para ela bater as asas.”
9. Accept one large-target tap.
10. Play a short wing flap and predictable flight/landing response, or the Reduced Motion response.
11. Caption “Muito bem. A de arara.”
12. Complete locally and expose Replay and Exit.

Target loss at any point stops narration, cancels the current sequence and interaction, records abandonment when appropriate, and leaves the system ready for configured reacquisition. A different target safely terminates A before starting its own fallback.

## Completion

Completion requires one successful arara interaction. It is idempotent: repeated callbacks cannot double-count the same run. Repeat starts a deliberate new run only after completion; there is no automatic loop.

## Content and media status

- Narration phrases are scripted but not represented as final audio without a licensed recording.
- Captions remain fully functional when narration resources are absent.
- One original procedural soft wing effect may be generated for development.
- `PLACEHOLDER_AUDIO=true` until the four Portuguese narration cues have documented production recordings.
