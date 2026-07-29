using System;
using UnityEngine;

[Serializable]
public sealed class TelemetryEvent
{
    public string eventId;
    public string eventType;
    public string profileId;
    public string contentId;
    public string timestampUtc;
    public long durationMs;
}

public interface ITelemetryService
{
    void Track(string eventType, string profileId = null, string contentId = null, long durationMs = 0);
}

public sealed class LocalTelemetryService : ITelemetryService
{
    private const string QueueKey = "ra.telemetry.queue";

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
            profileId = profileId ?? string.Empty,
            contentId = contentId ?? string.Empty,
            timestampUtc = DateTime.UtcNow.ToString("o"),
            durationMs = durationMs
        };

        var line = JsonUtility.ToJson(telemetryEvent);
        var current = PlayerPrefs.GetString(QueueKey, string.Empty);
        PlayerPrefs.SetString(QueueKey, string.IsNullOrEmpty(current) ? line : current + "\n" + line);
        PlayerPrefs.Save();
    }

    public string ExportAndClear()
    {
        var payload = PlayerPrefs.GetString(QueueKey, string.Empty);
        PlayerPrefs.DeleteKey(QueueKey);
        PlayerPrefs.Save();
        return payload;
    }
}
