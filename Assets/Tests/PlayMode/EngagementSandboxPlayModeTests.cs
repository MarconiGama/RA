using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class EngagementSandboxPlayModeTests
{
    private GameObject root;
    private LearningExperienceController controller;
    private TargetTrackingService tracking;
    private SensorySettingsService sensory;
    private AudioCueService audio;
    private InteractionCoordinator interaction;
    private PlaceholderMotionController motion;
    private ProgressService progress;
    private LocalTelemetryService telemetry;
    private string profileId;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        EngagementRuntimeServices.ResetForTests();
        profileId = "playmode-" + System.Guid.NewGuid().ToString("N");
        tracking = new TargetTrackingService();
        sensory = new SensorySettingsService(false);
        progress = new ProgressService();
        telemetry = new LocalTelemetryService();
        telemetry.ExportAndClear();

        root = new GameObject("SandboxTestRoot");
        audio = root.AddComponent<AudioCueService>();
        var content = new GameObject("PLACEHOLDER_ARARA");
        content.transform.SetParent(root.transform, false);
        content.AddComponent<BoxCollider>();
        content.AddComponent<TapInteraction>();
        motion = content.AddComponent<PlaceholderMotionController>();
        interaction = root.AddComponent<InteractionCoordinator>();
        controller = root.AddComponent<LearningExperienceController>();

        var item = new LearningContentItem
        {
            id = "letter-a",
            target = "A",
            title = "A de Arara",
            letterName = "A",
            word = "Arara",
            interactionType = "tap",
            interactionPrompt = "Toque na arara para ela bater as asas",
            interactionTargetName = "PLACEHOLDER_ARARA",
            contextualSoundResource = "Audio/Engagement/wing-flap-soft",
            targetHoldSeconds = 0f
        };
        var context = new LetterExperienceContext(
            item, tracking, sensory, audio, interaction, null, null, null,
            null, motion, progress, telemetry, profileId);
        controller.Configure(context, ExperienceSequence.ImmediateForTests());
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (root != null) Object.Destroy(root);
        telemetry.ExportAndClear();
        yield return null;
    }

    [UnityTest]
    public IEnumerator FlowA_CompletesLocally()
    {
        tracking.NotifyFound("A");
        yield return WaitForState(LearningExperienceState.WaitingForInteraction);
        Assert.IsTrue(controller.TriggerInteraction());
        yield return WaitForState(LearningExperienceState.Completed);
        Assert.AreEqual(1, controller.CompletionCount);
        Assert.IsTrue(progress.GetEngagementProgress(profileId, "letter-a").completed);
    }

    [UnityTest]
    public IEnumerator TargetLostDuringNarration_CancelsEverything()
    {
        tracking.NotifyFound("A");
        yield return WaitForState(LearningExperienceState.LetterNarration);
        tracking.NotifyLost("A");
        yield return null;
        Assert.AreEqual(LearningExperienceState.TargetLost, controller.State);
        Assert.IsFalse(audio.IsNarrationPlaying);
        Assert.IsFalse(interaction.IsActive);
    }

    [UnityTest]
    public IEnumerator TargetLostDuringInteraction_CancelsEverything()
    {
        tracking.NotifyFound("A");
        yield return WaitForState(LearningExperienceState.WaitingForInteraction);
        tracking.NotifyLost("A");
        yield return null;
        Assert.AreEqual(LearningExperienceState.TargetLost, controller.State);
        Assert.IsFalse(interaction.IsActive);
    }

    [UnityTest]
    public IEnumerator ReplayNarration_IsRecordedAndDoesNotOverlap()
    {
        tracking.NotifyFound("A");
        yield return WaitForState(LearningExperienceState.WaitingForInteraction);
        var first = audio.PlayNarration(string.Empty, new SubtitleCue("Primeira", 1f));
        var second = audio.PlayNarration(string.Empty, new SubtitleCue("Segunda", 1f));
        Assert.IsFalse(first.IsActive);
        Assert.IsTrue(second.IsActive);
        controller.ReplayNarration();
        Assert.AreEqual(1, progress.GetEngagementProgress(profileId, "letter-a").narrationReplayCount);
    }

    [UnityTest]
    public IEnumerator TapCannotFireTwice()
    {
        tracking.NotifyFound("A");
        yield return WaitForState(LearningExperienceState.WaitingForInteraction);
        Assert.IsTrue(controller.TriggerInteraction());
        Assert.IsFalse(controller.TriggerInteraction());
        yield return WaitForState(LearningExperienceState.Completed);
        Assert.AreEqual(1, controller.CompletionCount);
    }

    [UnityTest]
    public IEnumerator HintAppearsAfterConfiguredInactivity()
    {
        tracking.NotifyFound("A");
        yield return WaitForState(LearningExperienceState.WaitingForInteraction);
        var frames = 0;
        while (controller.HintCount == 0 && frames++ < 120) yield return null;
        Assert.AreEqual(1, controller.HintCount);
    }

    [UnityTest]
    public IEnumerator ReducedMotionUsesAlternativeResponse()
    {
        sensory.SetReducedMotion(true);
        tracking.NotifyFound("A");
        yield return WaitForState(LearningExperienceState.WaitingForInteraction);
        controller.TriggerInteraction();
        yield return null;
        Assert.AreEqual("ReducedMotionResponse", motion.LastAnimation);
    }

    [UnityTest]
    public IEnumerator ExitStopsAllAudioAndInteraction()
    {
        tracking.NotifyFound("A");
        yield return WaitForState(LearningExperienceState.WaitingForInteraction);
        audio.PlayNarration(string.Empty, new SubtitleCue("Teste", 2f));
        controller.ExitExperience();
        yield return null;
        Assert.AreEqual(LearningExperienceState.Idle, controller.State);
        Assert.IsFalse(audio.IsNarrationPlaying);
        Assert.IsFalse(interaction.IsActive);
    }

    private IEnumerator WaitForState(LearningExperienceState expected)
    {
        var frames = 0;
        while (controller.State != expected && frames++ < 240) yield return null;
        Assert.AreEqual(expected, controller.State);
    }
}
