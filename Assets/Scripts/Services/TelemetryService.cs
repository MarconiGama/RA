using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class TelemetryEvent
{
    public string eventId;
    public string eventType;
    [NonSerialized]
    public string profileId;
    public string contentId;
    public string timestampUtc;
    public long durationMs;
    public string sensoryProfile;
    public bool reducedMotion;
}

public static class EngagementTelemetryEvents
{
    public const string ExperienceStarted = "experience_started";
    public const string TargetAcquired = "target_acquired";
    public const string TargetLost = "target_lost";
    public const string LetterNarrationStarted = "letter_narration_started";
    public const string LetterNarrationCompleted = "letter_narration_completed";
    public const string NarrationReplayed = "narration_replayed";
    public const string ContentRevealed = "content_revealed";
    public const string InteractionPrompted = "interaction_prompted";
    public const string InteractionStarted = "interaction_started";
    public const string InteractionCompleted = "interaction_completed";
    public const string HintShown = "hint_shown";
    public const string ExperienceCompleted = "experience_completed";
    public const string ExperienceAbandoned = "experience_abandoned";
    public const string SensoryProfileChanged = "sensory_profile_changed";
    public const string ReducedMotionEnabled = "reduced_motion_enabled";
    public const string AudioDisabled = "audio_disabled";
}

public interface ITelemetryService
{
    void Track(string eventType, string profileId = null, string contentId = null, long durationMs = 0);
}

public sealed class LocalTelemetryService : ITelemetryService
{
    private const string QueueKey = "ra.telemetry.queue";
    public const int MaxQueueLength = 200;

    public void Track(string eventType, string profileId = null, string contentId = null, long durationMs = 0)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("eventType é obrigatório.", "eventType");
        }

        var telemetryEvent = new TelemetryEvent
        {
            eventId = Guid.NewGuid().ToString("N"),
            eventType = eventType,
            contentId = contentId ?? string.Empty,
            timestampUtc = DateTime.UtcNow.ToString("o"),
            durationMs = durationMs
        };

        var line = JsonUtility.ToJson(telemetryEvent);
        AppendBounded(line);
        PlayerPrefs.Save();
    }

    public void TrackEngagement(
        string eventType,
        string contentId,
        SensoryPreferences sensory,
        long durationMs = 0)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("eventType é obrigatório.", "eventType");
        }

        var telemetryEvent = new TelemetryEvent
        {
            eventId = Guid.NewGuid().ToString("N"),
            eventType = eventType,
            contentId = contentId ?? string.Empty,
            timestampUtc = DateTime.UtcNow.ToString("o"),
            durationMs = Math.Max(0, durationMs),
            sensoryProfile = sensory == null ? SensoryProfile.Calm.ToString() : sensory.profile.ToString(),
            reducedMotion = sensory != null && sensory.reducedMotion
        };
        AppendBounded(JsonUtility.ToJson(telemetryEvent));
        PlayerPrefs.Save();
    }

    public string Export()
    {
        return PlayerPrefs.GetString(QueueKey, string.Empty);
    }

    public int Count
    {
        get
        {
            var payload = Export();
            return string.IsNullOrEmpty(payload) ? 0 : payload.Split('\n').Length;
        }
    }

    public string ExportAndClear()
    {
        var payload = PlayerPrefs.GetString(QueueKey, string.Empty);
        PlayerPrefs.DeleteKey(QueueKey);
        PlayerPrefs.Save();
        return payload;
    }

    private static void AppendBounded(string line)
    {
        var current = PlayerPrefs.GetString(QueueKey, string.Empty);
        var lines = new List<string>();
        if (!string.IsNullOrEmpty(current))
        {
            lines.AddRange(current.Split('\n'));
        }
        lines.Add(line);
        if (lines.Count > MaxQueueLength)
        {
            lines.RemoveRange(0, lines.Count - MaxQueueLength);
        }
        PlayerPrefs.SetString(QueueKey, string.Join("\n", lines.ToArray()));
    }
}
