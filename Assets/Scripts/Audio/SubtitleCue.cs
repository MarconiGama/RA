using System;

[Serializable]
public sealed class SubtitleCue
{
    public string text;
    public float fallbackDuration = 1.5f;

    public SubtitleCue()
    {
    }

    public SubtitleCue(string text, float fallbackDuration)
    {
        this.text = text;
        this.fallbackDuration = fallbackDuration;
    }
}
