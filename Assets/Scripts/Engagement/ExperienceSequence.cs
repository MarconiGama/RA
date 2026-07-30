using System;
using UnityEngine;

[Serializable]
public sealed class ExperienceSequence
{
    [Min(0f)] public float targetHoldSeconds = 0.75f;
    [Min(0f)] public float introductionSeconds = 0.8f;
    [Min(0f)] public float letterNarrationSeconds = 1.4f;
    [Min(0f)] public float wordNarrationSeconds = 1.2f;
    [Min(0f)] public float promptNarrationSeconds = 2.0f;
    [Min(0f)] public float interactionSeconds = 2.2f;
    [Min(0f)] public float positiveFeedbackSeconds = 1.8f;
    [Min(0f)] public float hintDelaySeconds = 5f;
    public bool restartAfterReacquisition = true;

    public static ExperienceSequence ImmediateForTests()
    {
        return new ExperienceSequence
        {
            targetHoldSeconds = 0f,
            introductionSeconds = 0f,
            letterNarrationSeconds = 0.02f,
            wordNarrationSeconds = 0.02f,
            promptNarrationSeconds = 0.02f,
            interactionSeconds = 0.02f,
            positiveFeedbackSeconds = 0.02f,
            hintDelaySeconds = 0.02f
        };
    }
}
