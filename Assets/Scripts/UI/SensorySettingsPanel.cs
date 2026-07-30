using UnityEngine;
using UnityEngine.UI;

public sealed class SensorySettingsPanel : MonoBehaviour
{
    [SerializeField] private Dropdown profileDropdown;
    [SerializeField] private Toggle narrationToggle;
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Toggle captionsToggle;
    [SerializeField] private Toggle reducedMotionToggle;
    [SerializeField] private Toggle autoPlayToggle;

    private SensorySettingsService service;

    public void Configure(SensorySettingsService settings)
    {
        service = settings ?? EngagementRuntimeServices.Sensory;
        Refresh();
    }

    public void SetProfile(int value)
    {
        EnsureService();
        service.SetProfile((SensoryProfile)Mathf.Clamp(value, 0, 2));
        Refresh();
    }

    public void SetNarration(bool value) { EnsureService(); service.SetNarrationEnabled(value); }
    public void SetSfx(bool value) { EnsureService(); service.SetSfxEnabled(value); }
    public void SetCaptions(bool value) { EnsureService(); service.SetCaptionsEnabled(value); }
    public void SetReducedMotion(bool value) { EnsureService(); service.SetReducedMotion(value); }
    public void SetAutoPlay(bool value) { EnsureService(); service.SetAutoPlayNarration(value); }

    public void Refresh()
    {
        EnsureService();
        var current = service.Current;
        if (profileDropdown != null) profileDropdown.SetValueWithoutNotify((int)current.profile);
        if (narrationToggle != null) narrationToggle.SetIsOnWithoutNotify(current.narrationEnabled);
        if (sfxToggle != null) sfxToggle.SetIsOnWithoutNotify(current.sfxEnabled);
        if (captionsToggle != null) captionsToggle.SetIsOnWithoutNotify(current.captionsEnabled);
        if (reducedMotionToggle != null) reducedMotionToggle.SetIsOnWithoutNotify(current.reducedMotion);
        if (autoPlayToggle != null) autoPlayToggle.SetIsOnWithoutNotify(current.autoPlayNarration);
    }

    private void EnsureService()
    {
        if (service == null) service = EngagementRuntimeServices.Sensory;
    }
}
