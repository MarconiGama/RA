using System;
using System.Collections;
using UnityEngine;

public sealed class LearningExperienceController : MonoBehaviour
{
    [SerializeField] private string targetName = "A";
    [SerializeField] private string contentResourcePath = "Content/alphabet-pt-br";
    [SerializeField] private ExperienceSequence sequence = new ExperienceSequence();
    [SerializeField] private GameObject letterRoot;
    [SerializeField] private GameObject lumiRoot;
    [SerializeField] private GameObject contentRoot;
    [SerializeField] private AudioCueService audioCueService;
    [SerializeField] private InteractionCoordinator interactionCoordinator;
    [SerializeField] private LetterExperienceHud hud;
    [SerializeField] private SubtitlePresenter subtitles;
    [SerializeField] private ExperienceStepIndicator stepIndicator;
    [SerializeField] private PlaceholderMotionController lumiMotion;
    [SerializeField] private PlaceholderMotionController contentMotion;

    private LetterExperienceContext context;
    private Coroutine activeRoutine;
    private int sequenceGeneration;
    private bool targetPresent;
    private bool completionRecorded;
    private bool subscriptionsActive;
    private DateTime startedAtUtc;

    public static LearningExperienceController ActiveInstance { get; private set; }

    public event Action<LearningExperienceState> StateChanged;
    public event Action ExperienceCompleted;

    public LearningExperienceState State { get; private set; }
    public bool TargetPresent { get { return targetPresent; } }
    public bool IsCompleted { get { return State == LearningExperienceState.Completed; } }
    public int CompletionCount { get; private set; }
    public int HintCount { get; private set; }

    private void Awake()
    {
        ResolveSceneReferences();
        SetRoots(false, false, false);
        SetState(LearningExperienceState.Idle);
    }

    private void Start()
    {
        if (context == null)
        {
            AutoConfigure();
        }
        Subscribe();
    }

    public void Configure(LetterExperienceContext value, ExperienceSequence configuredSequence = null)
    {
        Unsubscribe();
        context = value;
        if (configuredSequence != null)
        {
            sequence = configuredSequence;
        }
        if (context != null && context.Content != null)
        {
            targetName = context.Content.target;
        }
        BindPresentation();
        Subscribe();
    }

    public void SimulateTargetFound()
    {
        EnsureContext();
        context.Tracking.NotifyFound(targetName);
    }

    public void SimulateTargetLost()
    {
        EnsureContext();
        context.Tracking.NotifyLost(targetName);
    }

    public bool TriggerInteraction()
    {
        return context != null && context.Interaction != null && context.Interaction.TryTriggerTap();
    }

    public void ReplayNarration()
    {
        if (context == null || context.Audio == null || context.Content == null ||
            State == LearningExperienceState.Idle || State == LearningExperienceState.TargetLost ||
            State == LearningExperienceState.Exiting)
        {
            return;
        }
        context.Audio.ReplayNarration();
        context.Progress.RecordNarrationReplay(
            context.ProfileId,
            context.Content.id,
            context.Sensory.Current.profile);
        Track(EngagementTelemetryEvents.NarrationReplayed);
    }

    public void RepeatExperience()
    {
        if (!targetPresent || State != LearningExperienceState.Completed)
        {
            return;
        }
        completionRecorded = false;
        BeginExperience();
    }

    public void ExitExperience()
    {
        CancelActive(LearningExperienceState.Exiting, true);
        targetPresent = false;
        SetState(LearningExperienceState.Idle);
    }

    public void PauseExperience()
    {
        CancelSequenceOnly();
        if (context != null && context.Audio != null) context.Audio.StopAll();
        if (context != null && context.Interaction != null) context.Interaction.Deactivate();
        SetState(LearningExperienceState.Paused);
    }

    private void AutoConfigure()
    {
        var pack = new ContentRepository(contentResourcePath).Load();
        var content = ContentRepository.FindByTarget(pack, targetName);
        context = new LetterExperienceContext(
            content,
            EngagementRuntimeServices.Tracking,
            EngagementRuntimeServices.Sensory,
            audioCueService,
            interactionCoordinator,
            hud,
            subtitles,
            stepIndicator,
            lumiMotion,
            contentMotion,
            EngagementRuntimeServices.Progress,
            EngagementRuntimeServices.Telemetry);
        BindPresentation();
    }

    private void EnsureContext()
    {
        if (context == null) AutoConfigure();
        Subscribe();
    }

    private void BindPresentation()
    {
        if (context == null) return;
        if (context.Hud != null && context.Content != null)
        {
            context.Hud.SetContent(context.Content.EffectiveLetterName, context.Content.EffectiveWord);
        }
        if (context.Subtitles != null)
        {
            context.Subtitles.Configure(context.Audio, context.Sensory);
        }
        if (context.Audio != null)
        {
            context.Audio.Configure(new AudioSettingsService(context.Sensory));
        }
    }

    private void Subscribe()
    {
        if (subscriptionsActive || context == null) return;
        context.Tracking.TargetFound += OnTargetFound;
        context.Tracking.TargetLost += OnTargetLost;
        if (context.Interaction != null)
        {
            context.Interaction.Completed += OnInteractionCompleted;
            context.Interaction.HintShown += OnHintShown;
        }
        if (context.Hud != null)
        {
            context.Hud.ReplayRequested += ReplayNarration;
            context.Hud.ExitRequested += ExitExperience;
            context.Hud.ActionRequested += TriggerActionFromHud;
        }
        subscriptionsActive = true;
    }

    private void Unsubscribe()
    {
        if (!subscriptionsActive || context == null) return;
        context.Tracking.TargetFound -= OnTargetFound;
        context.Tracking.TargetLost -= OnTargetLost;
        if (context.Interaction != null)
        {
            context.Interaction.Completed -= OnInteractionCompleted;
            context.Interaction.HintShown -= OnHintShown;
        }
        if (context.Hud != null)
        {
            context.Hud.ReplayRequested -= ReplayNarration;
            context.Hud.ExitRequested -= ExitExperience;
            context.Hud.ActionRequested -= TriggerActionFromHud;
        }
        subscriptionsActive = false;
    }

    private void OnTargetFound(string foundTarget)
    {
        if (!string.Equals(foundTarget, targetName, StringComparison.OrdinalIgnoreCase))
        {
            if (ActiveInstance == this) CancelActive(LearningExperienceState.TargetLost, true);
            return;
        }
        targetPresent = true;
        if (State == LearningExperienceState.Completed && !sequence.restartAfterReacquisition)
        {
            return;
        }
        BeginExperience();
    }

    private void BeginExperience()
    {
        if (ActiveInstance != null && ActiveInstance != this)
        {
            ActiveInstance.ExitExperience();
        }
        ActiveInstance = this;
        CancelSequenceOnly();
        completionRecorded = false;
        startedAtUtc = DateTime.UtcNow;
        context.Progress.RecordDetected(context.ProfileId, context.Content.id, context.Sensory.Current.profile);
        Track(EngagementTelemetryEvents.TargetAcquired);
        Track(EngagementTelemetryEvents.ExperienceStarted);
        SetState(LearningExperienceState.TargetAcquired);
        activeRoutine = StartCoroutine(RunExperience(sequenceGeneration));
    }

    private IEnumerator RunExperience(int generation)
    {
        SetState(LearningExperienceState.Stabilizing);
        yield return WaitCancelable(Mathf.Max(sequence.targetHoldSeconds, context.Content.targetHoldSeconds), generation);
        if (!IsCurrent(generation)) yield break;

        SetRoots(true, true, false);
        SetState(LearningExperienceState.Introduction);
        if (context.LumiMotion != null) context.LumiMotion.Play("Enter", context.Sensory.Current.reducedMotion);
        yield return WaitCancelable(sequence.introductionSeconds, generation);
        if (!IsCurrent(generation)) yield break;

        SetState(LearningExperienceState.LetterNarration);
        PlayNarration(context.Content.narrationLetterResource, "Esta é a letra A", sequence.letterNarrationSeconds);
        Track(EngagementTelemetryEvents.LetterNarrationStarted);
        yield return WaitCancelable(sequence.letterNarrationSeconds, generation);
        if (!IsCurrent(generation)) yield break;
        Track(EngagementTelemetryEvents.LetterNarrationCompleted);

        SetRoots(true, true, true);
        SetState(LearningExperienceState.ContentReveal);
        if (context.ContentMotion != null) context.ContentMotion.Play("Enter", context.Sensory.Current.reducedMotion);
        Track(EngagementTelemetryEvents.ContentRevealed);
        PlayNarration(context.Content.narrationWordResource, "A de arara", sequence.wordNarrationSeconds);
        yield return WaitCancelable(sequence.wordNarrationSeconds, generation);
        if (!IsCurrent(generation)) yield break;

        SetState(LearningExperienceState.InteractionPrompt);
        PlayNarration(context.Content.narrationPromptResource, context.Content.interactionPrompt, sequence.promptNarrationSeconds);
        Track(EngagementTelemetryEvents.InteractionPrompted);
        yield return WaitCancelable(sequence.promptNarrationSeconds, generation);
        if (!IsCurrent(generation)) yield break;

        SetState(LearningExperienceState.WaitingForInteraction);
        if (context.Hud != null) context.Hud.SetAction("Toque na arara", true);
        if (context.Interaction != null) context.Interaction.ActivateTap(sequence.hintDelaySeconds);
        activeRoutine = null;
    }

    private void OnInteractionCompleted(InteractionResult result)
    {
        if (State != LearningExperienceState.WaitingForInteraction || completionRecorded)
        {
            return;
        }
        Track(EngagementTelemetryEvents.InteractionStarted);
        context.Progress.RecordInteraction(context.ProfileId, context.Content.id, context.Sensory.Current.profile);
        SetState(LearningExperienceState.InteractionRunning);
        if (context.Audio != null) context.Audio.PlaySfx(context.Content.contextualSoundResource);
        if (context.ContentMotion != null) context.ContentMotion.Play("ShortFlight", context.Sensory.Current.reducedMotion);
        activeRoutine = StartCoroutine(CompleteInteraction(sequenceGeneration, result));
    }

    private IEnumerator CompleteInteraction(int generation, InteractionResult result)
    {
        yield return WaitCancelable(sequence.interactionSeconds, generation);
        if (!IsCurrent(generation)) yield break;
        Track(EngagementTelemetryEvents.InteractionCompleted, result == null ? 0 : result.durationMs);

        SetState(LearningExperienceState.PositiveFeedback);
        if (context.LumiMotion != null) context.LumiMotion.Play("CelebrateCalm", context.Sensory.Current.reducedMotion);
        PlayNarration(context.Content.narrationSuccessResource, "Muito bem. A de arara", sequence.positiveFeedbackSeconds);
        yield return WaitCancelable(sequence.positiveFeedbackSeconds, generation);
        if (!IsCurrent(generation)) yield break;
        CompleteExperience();
        activeRoutine = null;
    }

    private void CompleteExperience()
    {
        if (completionRecorded) return;
        completionRecorded = true;
        CompletionCount++;
        var duration = (long)Math.Max(0, (DateTime.UtcNow - startedAtUtc).TotalMilliseconds);
        context.Progress.RecordExperienceCompleted(
            context.ProfileId,
            context.Content.id,
            context.Sensory.Current.profile,
            duration);
        Track(EngagementTelemetryEvents.ExperienceCompleted, duration);
        SetState(LearningExperienceState.Completed);
        if (context.Hud != null)
        {
            context.Hud.SetAction("Repetir experiência", true);
            context.Hud.SetReplayAvailable(true);
        }
        var handler = ExperienceCompleted;
        if (handler != null) handler();
    }

    private void OnTargetLost(string lostTarget)
    {
        if (!string.Equals(lostTarget, targetName, StringComparison.OrdinalIgnoreCase)) return;
        targetPresent = false;
        Track(EngagementTelemetryEvents.TargetLost);
        CancelActive(LearningExperienceState.TargetLost, !completionRecorded);
    }

    private void CancelActive(LearningExperienceState terminalState, bool abandoned)
    {
        CancelSequenceOnly();
        if (context != null)
        {
            if (context.Audio != null) context.Audio.StopAll();
            if (context.Interaction != null) context.Interaction.Deactivate();
            if (context.LumiMotion != null) context.LumiMotion.StopMotion();
            if (context.ContentMotion != null) context.ContentMotion.StopMotion();
            if (abandoned && State != LearningExperienceState.Idle && State != LearningExperienceState.Completed)
            {
                Track(EngagementTelemetryEvents.ExperienceAbandoned);
            }
        }
        SetRoots(false, false, false);
        SetState(terminalState);
        if (ActiveInstance == this) ActiveInstance = null;
    }

    private void CancelSequenceOnly()
    {
        sequenceGeneration++;
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
    }

    private IEnumerator WaitCancelable(float seconds, int generation)
    {
        var elapsed = 0f;
        var duration = Mathf.Max(0f, seconds);
        while (elapsed < duration && IsCurrent(generation))
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private bool IsCurrent(int generation)
    {
        return generation == sequenceGeneration && targetPresent && isActiveAndEnabled;
    }

    private void PlayNarration(string resourcePath, string text, float duration)
    {
        if (context.Audio != null)
        {
            context.Audio.PlayNarration(resourcePath, new SubtitleCue(text, duration));
        }
        else if (context.Subtitles != null)
        {
            context.Subtitles.Show(text);
        }
    }

    private void OnHintShown()
    {
        HintCount++;
        context.Progress.RecordHint(context.ProfileId, context.Content.id, context.Sensory.Current.profile);
        Track(EngagementTelemetryEvents.HintShown);
        if (context.Hud != null) context.Hud.SetAction("Toque com calma na arara", true);
    }

    private void Track(string eventType, long durationMs = 0)
    {
        if (context != null && context.Telemetry != null && context.Content != null)
        {
            context.Telemetry.TrackEngagement(eventType, context.Content.id, context.Sensory.Current, durationMs);
        }
    }

    private void TriggerActionFromHud()
    {
        if (State == LearningExperienceState.Completed) RepeatExperience();
        else TriggerInteraction();
    }

    private void SetState(LearningExperienceState value)
    {
        State = value;
        if (context != null && context.StepIndicator != null) context.StepIndicator.SetState(value);
        var handler = StateChanged;
        if (handler != null) handler(value);
    }

    private void SetRoots(bool letter, bool lumi, bool contentVisible)
    {
        if (letterRoot != null) letterRoot.SetActive(letter);
        if (lumiRoot != null) lumiRoot.SetActive(lumi);
        if (contentRoot != null) contentRoot.SetActive(contentVisible);
    }

    private void ResolveSceneReferences()
    {
        if (audioCueService == null) audioCueService = GetComponentInChildren<AudioCueService>(true);
        if (interactionCoordinator == null) interactionCoordinator = GetComponentInChildren<InteractionCoordinator>(true);
        if (hud == null) hud = GetComponentInChildren<LetterExperienceHud>(true);
        if (subtitles == null) subtitles = GetComponentInChildren<SubtitlePresenter>(true);
        if (stepIndicator == null) stepIndicator = GetComponentInChildren<ExperienceStepIndicator>(true);
        var motions = GetComponentsInChildren<PlaceholderMotionController>(true);
        if (motions.Length > 0 && lumiMotion == null) lumiMotion = motions[0];
        if (motions.Length > 1 && contentMotion == null) contentMotion = motions[1];
    }

    private void OnDisable()
    {
        if (context != null) CancelActive(LearningExperienceState.Paused, false);
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
        if (ActiveInstance == this) ActiveInstance = null;
    }
}
