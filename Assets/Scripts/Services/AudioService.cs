using System.Collections.Generic;
using UnityEngine;

public interface IAudioService
{
    bool Play(string resourcePath);
    void Stop();
}

public sealed class AudioService : IAudioService
{
    private readonly AudioSource source;
    private readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    public AudioService(AudioSource source)
    {
        this.source = source;
    }

    public bool Play(string resourcePath)
    {
        if (source == null || string.IsNullOrWhiteSpace(resourcePath))
        {
            return false;
        }

        AudioClip clip;
        if (!cache.TryGetValue(resourcePath, out clip))
        {
            clip = Resources.Load<AudioClip>(resourcePath);
            cache[resourcePath] = clip;
        }

        if (clip == null)
        {
            Debug.LogWarning("Áudio não encontrado em Resources/" + resourcePath);
            return false;
        }

        source.clip = clip;
        source.Play();
        return true;
    }

    public void Stop()
    {
        if (source != null)
        {
            source.Stop();
        }
    }
}
