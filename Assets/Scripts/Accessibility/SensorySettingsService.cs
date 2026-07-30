using System;
using UnityEngine;

public sealed class SensorySettingsService
{
    private const string Key = "ra.accessibility.sensory.v1";
    private SensoryPreferences current;

    public event Action<SensoryPreferences> Changed;

    public SensoryPreferences Current
    {
        get { return current; }
    }

    public SensorySettingsService(bool loadPersisted = true)
    {
        current = loadPersisted ? Load() : SensoryPreferences.Create(SensoryProfile.Calm);
    }

    public void SetProfile(SensoryProfile profile)
    {
        current.ApplyProfile(profile);
        PersistAndNotify();
    }

    public void SetNarrationEnabled(bool enabled)
    {
        current.narrationEnabled = enabled;
        PersistAndNotify();
    }

    public void SetSfxEnabled(bool enabled)
    {
        current.sfxEnabled = enabled;
        PersistAndNotify();
    }

    public void SetCaptionsEnabled(bool enabled)
    {
        current.captionsEnabled = enabled;
        PersistAndNotify();
    }

    public void SetReducedMotion(bool enabled)
    {
        current.reducedMotion = enabled;
        PersistAndNotify();
    }

    public void SetAutoPlayNarration(bool enabled)
    {
        current.autoPlayNarration = enabled;
        PersistAndNotify();
    }

    public void SetVolumes(float narration, float sfx, float ambient)
    {
        current.narrationVolume = Mathf.Clamp01(narration);
        current.sfxVolume = Mathf.Clamp01(sfx);
        current.ambientVolume = Mathf.Clamp01(ambient);
        PersistAndNotify();
    }

    private static SensoryPreferences Load()
    {
        var json = PlayerPrefs.GetString(Key, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            return SensoryPreferences.Create(SensoryProfile.Calm);
        }

        var loaded = JsonUtility.FromJson<SensoryPreferences>(json);
        return loaded ?? SensoryPreferences.Create(SensoryProfile.Calm);
    }

    private void PersistAndNotify()
    {
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(current));
        PlayerPrefs.Save();
        var handler = Changed;
        if (handler != null)
        {
            handler(current);
        }
    }
}
