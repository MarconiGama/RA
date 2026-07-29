using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ProgressService
{
    private const string Prefix = "ra.progress.";

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

    private static void ValidateKey(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(name + " é obrigatório.", name);
        }
    }
}
