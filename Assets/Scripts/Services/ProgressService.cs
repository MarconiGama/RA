using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ProgressService
{
    private const string Prefix = "ra.progress.";

    [Serializable]
    public sealed class EngagementProgress
    {
        public int detectedCount;
        public int interactionCount;
        public int narrationReplayCount;
        public int hintCount;
        public string firstCompletedAt;
        public string lastCompletedAt;
        public long totalDurationMs;
        public bool completed;
        public string sensoryProfile;
    }

    public void MarkCompleted(string profileId, string contentId)
    {
        ValidateKey(profileId, "profileId");
        ValidateKey(contentId, "contentId");
        PlayerPrefs.SetInt(GetKey(profileId, contentId), 1);
        PlayerPrefs.Save();
    }

    public bool IsCompleted(string profileId, string contentId)
    {
        ValidateKey(profileId, "profileId");
        ValidateKey(contentId, "contentId");
        return PlayerPrefs.GetInt(GetKey(profileId, contentId), 0) == 1;
    }

    public EngagementProgress GetEngagementProgress(string profileId, string contentId)
    {
        ValidateKey(profileId, "profileId");
        ValidateKey(contentId, "contentId");
        var json = PlayerPrefs.GetString(GetEngagementKey(profileId, contentId), string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            return new EngagementProgress
            {
                completed = IsCompleted(profileId, contentId),
                sensoryProfile = SensoryProfile.Calm.ToString()
            };
        }

        var progress = JsonUtility.FromJson<EngagementProgress>(json) ?? new EngagementProgress();
        progress.completed = progress.completed || IsCompleted(profileId, contentId);
        return progress;
    }

    public void RecordDetected(string profileId, string contentId, SensoryProfile sensoryProfile)
    {
        UpdateEngagement(profileId, contentId, sensoryProfile, delegate(EngagementProgress progress)
        {
            progress.detectedCount++;
        });
    }

    public void RecordInteraction(string profileId, string contentId, SensoryProfile sensoryProfile)
    {
        UpdateEngagement(profileId, contentId, sensoryProfile, delegate(EngagementProgress progress)
        {
            progress.interactionCount++;
        });
    }

    public void RecordNarrationReplay(string profileId, string contentId, SensoryProfile sensoryProfile)
    {
        UpdateEngagement(profileId, contentId, sensoryProfile, delegate(EngagementProgress progress)
        {
            progress.narrationReplayCount++;
        });
    }

    public void RecordHint(string profileId, string contentId, SensoryProfile sensoryProfile)
    {
        UpdateEngagement(profileId, contentId, sensoryProfile, delegate(EngagementProgress progress)
        {
            progress.hintCount++;
        });
    }

    public void RecordExperienceCompleted(
        string profileId,
        string contentId,
        SensoryProfile sensoryProfile,
        long durationMs)
    {
        UpdateEngagement(profileId, contentId, sensoryProfile, delegate(EngagementProgress progress)
        {
            var now = DateTime.UtcNow.ToString("o");
            if (string.IsNullOrEmpty(progress.firstCompletedAt))
            {
                progress.firstCompletedAt = now;
            }
            progress.lastCompletedAt = now;
            progress.totalDurationMs += Math.Max(0, durationMs);
            progress.completed = true;
        });
        MarkCompleted(profileId, contentId);
    }

    public int CountCompleted(string profileId, IEnumerable<LearningContentItem> items)
    {
        ValidateKey(profileId, "profileId");
        var count = 0;
        foreach (var item in items)
        {
            if (item != null && IsCompleted(profileId, item.id))
            {
                count++;
            }
        }
        return count;
    }

    public void ResetProfile(string profileId, IEnumerable<LearningContentItem> items)
    {
        ValidateKey(profileId, "profileId");
        foreach (var item in items)
        {
            if (item != null && !string.IsNullOrWhiteSpace(item.id))
            {
                PlayerPrefs.DeleteKey(GetKey(profileId, item.id));
            }
        }
        PlayerPrefs.Save();
    }

    private static string GetKey(string profileId, string contentId)
    {
        return Prefix + profileId.Trim() + "." + contentId.Trim();
    }

    private static string GetEngagementKey(string profileId, string contentId)
    {
        return GetKey(profileId, contentId) + ".engagement.v1";
    }

    private static void UpdateEngagement(
        string profileId,
        string contentId,
        SensoryProfile sensoryProfile,
        Action<EngagementProgress> update)
    {
        ValidateKey(profileId, "profileId");
        ValidateKey(contentId, "contentId");
        var key = GetEngagementKey(profileId, contentId);
        var json = PlayerPrefs.GetString(key, string.Empty);
        var progress = string.IsNullOrEmpty(json)
            ? new EngagementProgress()
            : JsonUtility.FromJson<EngagementProgress>(json) ?? new EngagementProgress();
        progress.sensoryProfile = sensoryProfile.ToString();
        update(progress);
        PlayerPrefs.SetString(key, JsonUtility.ToJson(progress));
        PlayerPrefs.Save();
    }

    private static void ValidateKey(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(name + " é obrigatório.", name);
        }
    }
}
