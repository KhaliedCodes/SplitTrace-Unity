using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AudioClipsSO;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private int maxSimultaneousAudios = 5;

    private List<AudioSource> audioSources;
    public AudioClipsSO audioClipsSO;

    AudioClipData audioClipData;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        audioSources = new List<AudioSource>();
        for (int i = 0; i < maxSimultaneousAudios; i++)
        {
            audioSources.Add(gameObject.AddComponent<AudioSource>());
        }
    }

    public void PlayAudioClip(string category, string clipName, bool loop = false)
    {
        AudioClip clip = audioClipsSO.GetAudioClip(category, clipName);

        if (clip == null)
        {
            Debug.LogWarning($"Audio clip '{clipName}' in category '{category}' not found!");
            return;
        }
        AudioSource availableAudioSource = GetAvailableAudioSource();

        if (availableAudioSource == null)
        {
            Debug.LogWarning("No available audio sources. Cannot play audio.");
            return;
        }
        availableAudioSource.clip = clip;
        availableAudioSource.loop = loop;
        availableAudioSource.Play();
    }

    public AudioSource PlayAudioClipAndGetSource(string category, string clipName, bool loop = false)
    {
        AudioClip clip = audioClipsSO.GetAudioClip(category, clipName);

        if (clip == null)
        {
            Debug.LogWarning($"Audio clip '{clipName}' in category '{category}' not found!");
            return null;
        }

        AudioSource availableAudioSource = GetAvailableAudioSource();

        if (availableAudioSource == null)
        {
            Debug.LogWarning("No available audio sources. Cannot play audio.");
            return null;
        }
        availableAudioSource.Stop();

        availableAudioSource.clip = clip;
        availableAudioSource.loop = loop;
        availableAudioSource.Play();

        return availableAudioSource;
    }

    public void StopAllAudio()
    {
        foreach (var source in audioSources)
        {
            source.Stop();
        }
    }

    public bool IsAnyPlaying()
    {
        return audioSources.Exists(source => source.isPlaying);
    }

    private AudioSource GetAvailableAudioSource()
    {
        return audioSources.Find(source => !source.isPlaying);
    }
}
