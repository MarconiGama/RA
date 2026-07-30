using UnityEngine;

public sealed class AudioSettingsService
{
    private readonly SensorySettingsService sensorySettings;

    public AudioSettingsService(SensorySettingsService sensorySettings)
    {
        this.sensorySettings = sensorySettings ?? new SensorySettingsService();
    }

    public bool NarrationEnabled { get { return sensorySettings.Current.narrationEnabled; } }
    public bool SfxEnabled { get { return sensorySettings.Current.sfxEnabled; } }
    public bool AmbientEnabled { get { return sensorySettings.Current.ambientEnabled; } }
    public float NarrationVolume { get { return Mathf.Clamp01(sensorySettings.Current.narrationVolume); } }
    public float SfxVolume { get { return Mathf.Clamp01(sensorySettings.Current.sfxVolume); } }
    public float AmbientVolume { get { return Mathf.Clamp01(sensorySettings.Current.ambientVolume); } }
}
