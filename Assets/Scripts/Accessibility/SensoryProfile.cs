using System;

public enum SensoryProfile
{
    Calm = 0,
    Balanced = 1,
    Expressive = 2
}

[Serializable]
public sealed class SensoryPreferences
{
    public SensoryProfile profile = SensoryProfile.Calm;
    public bool narrationEnabled = true;
    public bool sfxEnabled = true;
    public bool captionsEnabled = true;
    public bool reducedMotion;
    public bool autoPlayNarration;
    public bool ambientEnabled;
    public float narrationVolume = 0.8f;
    public float sfxVolume = 0.35f;
    public float ambientVolume;

    public static SensoryPreferences Create(SensoryProfile profile)
    {
        var preferences = new SensoryPreferences();
        preferences.ApplyProfile(profile);
        return preferences;
    }

    public void ApplyProfile(SensoryProfile value)
    {
        profile = value;
        narrationEnabled = true;
        sfxEnabled = true;
        captionsEnabled = true;
        ambientEnabled = false;

        switch (value)
        {
            case SensoryProfile.Balanced:
                reducedMotion = false;
                autoPlayNarration = true;
                narrationVolume = 0.85f;
                sfxVolume = 0.55f;
                ambientVolume = 0f;
                break;
            case SensoryProfile.Expressive:
                reducedMotion = false;
                autoPlayNarration = true;
                narrationVolume = 0.9f;
                sfxVolume = 0.7f;
                ambientVolume = 0f;
                break;
            default:
                reducedMotion = false;
                autoPlayNarration = false;
                narrationVolume = 0.75f;
                sfxVolume = 0.3f;
                ambientVolume = 0f;
                break;
        }
    }
}
