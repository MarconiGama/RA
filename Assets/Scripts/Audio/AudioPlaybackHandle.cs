using System;

public enum AudioCueChannel
{
    Narration,
    Sfx,
    Ambient
}

public sealed class AudioPlaybackHandle
{
    private readonly Action<AudioPlaybackHandle> stopAction;

    public int Id { get; private set; }
    public AudioCueChannel Channel { get; private set; }
    public string ResourcePath { get; private set; }
    public bool IsActive { get; internal set; }
    public bool ClipWasAvailable { get; internal set; }

    internal AudioPlaybackHandle(
        int id,
        AudioCueChannel channel,
        string resourcePath,
        Action<AudioPlaybackHandle> stopAction)
    {
        Id = id;
        Channel = channel;
        ResourcePath = resourcePath ?? string.Empty;
        this.stopAction = stopAction;
        IsActive = true;
    }

    public void Stop()
    {
        if (IsActive && stopAction != null)
        {
            stopAction(this);
        }
    }
}
