using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class AudioCueService : MonoBehaviour
{
    [SerializeField] private AudioSource narrationSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private float stopFadeSeconds = 0.08f;

    private readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();
    private AudioSettingsService settings;
    private AudioPlaybackHandle narrationHandle;
    private Coroutine narrationRoutine;
    private SubtitleCue activeSubtitle;
    private string lastNarrationResource;
    private SubtitleCue lastNarrationSubtitle;
    private int nextHandleId;

    public event Action<SubtitleCue> SubtitleStarted;
    public event Action<SubtitleCue> SubtitleEnded;
    public event Action<AudioPlaybackHandle> NarrationStarted;
    public event Action<AudioPlaybackHandle> NarrationCompleted;

    public bool IsNarrationPlaying { get { return narrationHandle != null && narrationHandle.IsActive; } }
    public int CachedClipCount { get { return clipCache.Count; } }
    public bool AmbientEnabled { get { return settings != null && settings.AmbientEnabled; } }
    public float StopFadeSeconds { get { return stopFadeSeconds; } }

    private void Awake()
    {
        EnsureSources();
        if (settings == null)
        {
            settings = new AudioSettingsService(new SensorySettingsService());
        }
        ApplyVolumes();
    }

    public void Configure(AudioSettingsService audioSettings)
    {
        settings = audioSettings ?? new AudioSettingsService(new SensorySettingsService());
        EnsureSources();
        ApplyVolumes();
    }

    public AudioPlaybackHandle PlayNarration(string resourcePath, SubtitleCue subtitle)
    {
        StopNarration();
        EnsureSources();
        lastNarrationResource = resourcePath ?? string.Empty;
        lastNarrationSubtitle = subtitle;

        narrationHandle = NewHandle(AudioCueChannel.Narration, resourcePath);
        var clip = LoadClip(resourcePath);
        narrationHandle.ClipWasAvailable = clip != null;
        var duration = subtitle == null ? 0.1f : Mathf.Max(0.05f, subtitle.fallbackDuration);

        if (settings == null)
        {
            settings = new AudioSettingsService(new SensorySettingsService());
        }

        if (clip != null && settings.NarrationEnabled)
        {
            narrationSource.clip = clip;
            narrationSource.volume = settings.NarrationVolume;
            narrationSource.Play();
            duration = Mathf.Max(duration, clip.length);
        }

        BeginSubtitle(subtitle);
        var started = NarrationStarted;
        if (started != null)
        {
            started(narrationHandle);
        }
        narrationRoutine = StartCoroutine(CompleteNarrationAfter(narrationHandle, duration));
        return narrationHandle;
    }

    public AudioPlaybackHandle ReplayNarration()
    {
        return PlayNarration(lastNarrationResource, lastNarrationSubtitle);
    }

    public AudioPlaybackHandle PlaySfx(string resourcePath)
    {
        EnsureSources();
        var handle = NewHandle(AudioCueChannel.Sfx, resourcePath);
        var clip = LoadClip(resourcePath);
        handle.ClipWasAvailable = clip != null;
        if (settings == null)
        {
            settings = new AudioSettingsService(new SensorySettingsService());
        }

        if (clip != null && settings.SfxEnabled)
        {
            sfxSource.PlayOneShot(clip, settings.SfxVolume);
        }
        handle.IsActive = false;
        return handle;
    }

    public void PlayAmbient(string resourcePath)
    {
        EnsureSources();
        if (settings == null || !settings.AmbientEnabled)
        {
            ambientSource.Stop();
            return;
        }

        var clip = LoadClip(resourcePath);
        if (clip == null)
        {
            return;
        }
        ambientSource.clip = clip;
        ambientSource.loop = true;
        ambientSource.volume = settings.AmbientVolume;
        ambientSource.Play();
    }

    public void StopNarration()
    {
        if (narrationRoutine != null)
        {
            StopCoroutine(narrationRoutine);
            narrationRoutine = null;
        }
        if (narrationSource != null)
        {
            narrationSource.Stop();
            narrationSource.clip = null;
        }
        CompleteNarration(narrationHandle);
    }

    public void StopAll()
    {
        StopNarration();
        if (sfxSource != null)
        {
            sfxSource.Stop();
        }
        if (ambientSource != null)
        {
            ambientSource.Stop();
        }
    }

    public void FadeAndStopNarration()
    {
        if (!isActiveAndEnabled || narrationSource == null || !narrationSource.isPlaying)
        {
            StopNarration();
            return;
        }
        StartCoroutine(FadeNarrationRoutine());
    }

    private IEnumerator CompleteNarrationAfter(AudioPlaybackHandle handle, float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        if (handle != null && handle.IsActive)
        {
            CompleteNarration(handle);
        }
        narrationRoutine = null;
    }

    private IEnumerator FadeNarrationRoutine()
    {
        var original = narrationSource.volume;
        var duration = Mathf.Max(0.01f, stopFadeSeconds);
        var elapsed = 0f;
        while (elapsed < duration && narrationSource.isPlaying)
        {
            elapsed += Time.unscaledDeltaTime;
            narrationSource.volume = Mathf.Lerp(original, 0f, elapsed / duration);
            yield return null;
        }
        StopNarration();
        ApplyVolumes();
    }

    private AudioClip LoadClip(string resourcePath)
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            return null;
        }
        AudioClip clip;
        if (!clipCache.TryGetValue(resourcePath, out clip))
        {
            clip = Resources.Load<AudioClip>(resourcePath);
            clipCache[resourcePath] = clip;
            if (clip == null)
            {
                Debug.LogWarning("PLACEHOLDER_AUDIO: recurso opcional ausente em Resources/" + resourcePath);
            }
        }
        return clip;
    }

    private void BeginSubtitle(SubtitleCue subtitle)
    {
        EndSubtitle();
        activeSubtitle = subtitle;
        if (subtitle != null)
        {
            var handler = SubtitleStarted;
            if (handler != null)
            {
                handler(subtitle);
            }
        }
    }

    private void EndSubtitle()
    {
        if (activeSubtitle == null)
        {
            return;
        }
        var cue = activeSubtitle;
        activeSubtitle = null;
        var handler = SubtitleEnded;
        if (handler != null)
        {
            handler(cue);
        }
    }

    private void CompleteNarration(AudioPlaybackHandle handle)
    {
        if (handle == null || !handle.IsActive)
        {
            EndSubtitle();
            return;
        }
        handle.IsActive = false;
        EndSubtitle();
        if (narrationHandle == handle)
        {
            narrationHandle = null;
        }
        var completed = NarrationCompleted;
        if (completed != null)
        {
            completed(handle);
        }
    }

    private AudioPlaybackHandle NewHandle(AudioCueChannel channel, string resourcePath)
    {
        nextHandleId++;
        return new AudioPlaybackHandle(nextHandleId, channel, resourcePath, StopHandle);
    }

    private void StopHandle(AudioPlaybackHandle handle)
    {
        if (handle != null && handle.Channel == AudioCueChannel.Narration)
        {
            StopNarration();
        }
        else if (handle != null)
        {
            handle.IsActive = false;
        }
    }

    private void EnsureSources()
    {
        if (narrationSource == null)
        {
            narrationSource = CreateSource("Narration", false);
        }
        if (sfxSource == null)
        {
            sfxSource = CreateSource("Sfx", false);
        }
        if (ambientSource == null)
        {
            ambientSource = CreateSource("Ambient", true);
        }
    }

    private AudioSource CreateSource(string channelName, bool loop)
    {
        var child = new GameObject("Audio_" + channelName);
        child.transform.SetParent(transform, false);
        var source = child.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        return source;
    }

    private void ApplyVolumes()
    {
        if (settings == null)
        {
            return;
        }
        narrationSource.volume = settings.NarrationVolume;
        sfxSource.volume = settings.SfxVolume;
        ambientSource.volume = settings.AmbientVolume;
    }

    private void OnDisable()
    {
        StopAll();
    }
}
