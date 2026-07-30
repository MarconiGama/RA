using System;
using System.Collections;
using UnityEngine;

public sealed class InteractionCoordinator : MonoBehaviour
{
    [SerializeField] private TapInteraction tapInteraction;
    [SerializeField] private float hintDelaySeconds = 5f;
    [SerializeField] private float demonstrationDelaySeconds = 9f;

    private IContentInteraction activeInteraction;
    private Coroutine hintRoutine;
    private bool completionForwarded;

    public event Action<InteractionResult> Completed;
    public event Action HintShown;

    public bool IsActive { get { return activeInteraction != null && activeInteraction.IsActive; } }

    private void Awake()
    {
        if (tapInteraction == null)
        {
            tapInteraction = GetComponentInChildren<TapInteraction>(true);
        }
    }

    public void ActivateTap(float hintDelay = -1f)
    {
        Deactivate();
        if (tapInteraction == null)
        {
            return;
        }
        completionForwarded = false;
        activeInteraction = tapInteraction;
        activeInteraction.Completed += OnInteractionCompleted;
        activeInteraction.Activate();
        hintRoutine = StartCoroutine(HintRoutine(hintDelay < 0f ? hintDelaySeconds : hintDelay));
    }

    public bool TryTriggerTap()
    {
        return tapInteraction != null && tapInteraction.TryTrigger();
    }

    public void Deactivate()
    {
        if (hintRoutine != null)
        {
            StopCoroutine(hintRoutine);
            hintRoutine = null;
        }
        if (activeInteraction != null)
        {
            activeInteraction.Completed -= OnInteractionCompleted;
            activeInteraction.Deactivate();
            activeInteraction = null;
        }
    }

    private IEnumerator HintRoutine(float delay)
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, delay));
        if (IsActive)
        {
            var handler = HintShown;
            if (handler != null)
            {
                handler();
            }
        }
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, demonstrationDelaySeconds - delay));
        if (IsActive)
        {
            activeInteraction.Demonstrate();
        }
        hintRoutine = null;
    }

    private void OnInteractionCompleted(InteractionResult result)
    {
        if (completionForwarded)
        {
            return;
        }
        completionForwarded = true;
        var handler = Completed;
        Deactivate();
        if (handler != null)
        {
            handler(result);
        }
    }

    private void OnDisable()
    {
        Deactivate();
    }
}
