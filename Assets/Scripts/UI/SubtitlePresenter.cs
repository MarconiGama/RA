using UnityEngine;
using UnityEngine.UI;

public sealed class SubtitlePresenter : MonoBehaviour
{
    [SerializeField] private Text subtitleText;
    [SerializeField] private CanvasGroup canvasGroup;

    private AudioCueService audioService;
    private SensorySettingsService sensorySettings;

    public string CurrentText { get { return subtitleText == null ? string.Empty : subtitleText.text; } }

    public void Configure(AudioCueService audio, SensorySettingsService sensory)
    {
        Unsubscribe();
        audioService = audio;
        sensorySettings = sensory;
        if (audioService != null)
        {
            audioService.SubtitleStarted += OnSubtitleStarted;
            audioService.SubtitleEnded += OnSubtitleEnded;
        }
        Hide();
    }

    public void Show(string text)
    {
        var captionsEnabled = sensorySettings == null || sensorySettings.Current.captionsEnabled;
        if (!captionsEnabled || string.IsNullOrWhiteSpace(text))
        {
            Hide();
            return;
        }
        if (subtitleText != null) subtitleText.text = text;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void Hide()
    {
        if (subtitleText != null) subtitleText.text = string.Empty;
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    private void OnSubtitleStarted(SubtitleCue cue)
    {
        Show(cue == null ? string.Empty : cue.text);
    }

    private void OnSubtitleEnded(SubtitleCue cue)
    {
        Hide();
    }

    private void Unsubscribe()
    {
        if (audioService != null)
        {
            audioService.SubtitleStarted -= OnSubtitleStarted;
            audioService.SubtitleEnded -= OnSubtitleEnded;
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }
}
