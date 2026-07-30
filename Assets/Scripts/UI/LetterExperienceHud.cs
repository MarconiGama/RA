using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class LetterExperienceHud : MonoBehaviour
{
    [SerializeField] private RectTransform safeAreaRoot;
    [SerializeField] private Text letterText;
    [SerializeField] private Text wordText;
    [SerializeField] private Text actionText;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button actionButton;

    public event Action ReplayRequested;
    public event Action ExitRequested;
    public event Action ActionRequested;

    private void Awake()
    {
        if (replayButton != null) replayButton.onClick.AddListener(RequestReplay);
        if (exitButton != null) exitButton.onClick.AddListener(RequestExit);
        if (actionButton != null) actionButton.onClick.AddListener(RequestAction);
        ApplySafeArea();
    }

    public void SetContent(string letter, string word)
    {
        if (letterText != null) letterText.text = letter ?? string.Empty;
        if (wordText != null) wordText.text = word ?? string.Empty;
    }

    public void SetAction(string label, bool interactable)
    {
        if (actionText != null) actionText.text = label ?? string.Empty;
        if (actionButton != null) actionButton.interactable = interactable;
    }

    public void SetReplayAvailable(bool available)
    {
        if (replayButton != null) replayButton.interactable = available;
    }

    public void RequestReplay()
    {
        var handler = ReplayRequested;
        if (handler != null) handler();
    }

    public void RequestExit()
    {
        var handler = ExitRequested;
        if (handler != null) handler();
    }

    public void RequestAction()
    {
        var handler = ActionRequested;
        if (handler != null) handler();
    }

    public void ApplySafeArea()
    {
        if (safeAreaRoot == null || Screen.width <= 0 || Screen.height <= 0)
        {
            return;
        }
        var safe = Screen.safeArea;
        safeAreaRoot.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
        safeAreaRoot.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
        safeAreaRoot.offsetMin = Vector2.zero;
        safeAreaRoot.offsetMax = Vector2.zero;
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplySafeArea();
    }
}
