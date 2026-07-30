using UnityEngine;
using UnityEngine.UI;

public sealed class ExperienceStepIndicator : MonoBehaviour
{
    [SerializeField] private Text label;

    public string CurrentLabel { get { return label == null ? string.Empty : label.text; } }

    public void SetState(LearningExperienceState state)
    {
        var value = "Ouça";
        if (state == LearningExperienceState.ContentReveal ||
            state == LearningExperienceState.InteractionPrompt)
        {
            value = "Descubra";
        }
        else if (state == LearningExperienceState.WaitingForInteraction ||
                 state == LearningExperienceState.InteractionRunning ||
                 state == LearningExperienceState.PositiveFeedback ||
                 state == LearningExperienceState.Completed)
        {
            value = "Interaja";
        }
        if (label != null) label.text = value;
    }
}
