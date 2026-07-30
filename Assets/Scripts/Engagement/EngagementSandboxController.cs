using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public sealed class EngagementSandboxController : MonoBehaviour
{
    [SerializeField] private LearningExperienceController experience;
    [SerializeField] private SensorySettingsPanel sensoryPanel;
    [SerializeField] private Text currentStateText;
    [SerializeField] private Text exportStatusText;

    private void Awake()
    {
        EngagementRuntimeServices.ResetForTests();
        if (experience == null) experience = GetComponentInChildren<LearningExperienceController>(true);
        if (sensoryPanel != null) sensoryPanel.Configure(EngagementRuntimeServices.Sensory);
    }

    private void OnEnable()
    {
        if (experience != null) experience.StateChanged += OnStateChanged;
        OnStateChanged(experience == null ? LearningExperienceState.Idle : experience.State);
    }

    public void SimulateTargetFound()
    {
        if (experience != null) experience.SimulateTargetFound();
    }

    public void SimulateTargetLost()
    {
        if (experience != null) experience.SimulateTargetLost();
    }

    public void TriggerTap()
    {
        if (experience != null) experience.TriggerInteraction();
    }

    public void ReplayNarration()
    {
        if (experience != null) experience.ReplayNarration();
    }

    public void RepeatExperience()
    {
        if (experience != null) experience.RepeatExperience();
    }

    public void ExitExperience()
    {
        if (experience != null) experience.ExitExperience();
    }

    public void SetSensoryProfile(int profile)
    {
        EngagementRuntimeServices.Sensory.SetProfile((SensoryProfile)Mathf.Clamp(profile, 0, 2));
        EngagementRuntimeServices.Telemetry.TrackEngagement(
            EngagementTelemetryEvents.SensoryProfileChanged,
            "letter-a",
            EngagementRuntimeServices.Sensory.Current);
        if (sensoryPanel != null) sensoryPanel.Refresh();
    }

    public void CycleSensoryProfile()
    {
        var next = ((int)EngagementRuntimeServices.Sensory.Current.profile + 1) % 3;
        SetSensoryProfile(next);
    }

    public void SetReducedMotion(bool enabled)
    {
        EngagementRuntimeServices.Sensory.SetReducedMotion(enabled);
        if (enabled)
        {
            EngagementRuntimeServices.Telemetry.TrackEngagement(
                EngagementTelemetryEvents.ReducedMotionEnabled,
                "letter-a",
                EngagementRuntimeServices.Sensory.Current);
        }
        if (sensoryPanel != null) sensoryPanel.Refresh();
    }

    public void ToggleReducedMotion()
    {
        SetReducedMotion(!EngagementRuntimeServices.Sensory.Current.reducedMotion);
    }

    public void ExportTelemetry()
    {
        var directory = Path.Combine(Application.persistentDataPath, "RA");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "engagement-telemetry.jsonl");
        File.WriteAllText(path, EngagementRuntimeServices.Telemetry.Export());
        if (exportStatusText != null) exportStatusText.text = "Telemetria local exportada: " + path;
    }

    private void OnStateChanged(LearningExperienceState state)
    {
        if (currentStateText != null) currentStateText.text = "Estado: " + state;
    }

    private void OnDisable()
    {
        if (experience != null) experience.StateChanged -= OnStateChanged;
    }
}
