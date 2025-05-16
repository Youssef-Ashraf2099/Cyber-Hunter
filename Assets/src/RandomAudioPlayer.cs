using UnityEngine;
using System.Collections.Generic;

public class RandomAudioPlayer : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("List of audio clips to play randomly")]
    public List<AudioClip> audioClips;

    [Tooltip("AudioSource component (will create one if not assigned)")]
    public AudioSource audioSource;

    [Header("Playback Settings")]
    [Tooltip("Play a random clip when the game starts")]
    public bool playOnStart = true;

    [Tooltip("Minimum delay before playing (seconds)")]
    public float minDelay = 0f;

    [Tooltip("Maximum delay before playing (seconds)")]
    public float maxDelay = 1f;

    [Tooltip("Minimum pitch variation (-value to +value)")]
    [Range(0f, 1f)]
    public float pitchVariation = 0.1f;

    [Tooltip("Minimum volume variation (0-1)")]
    [Range(0f, 1f)]
    public float volumeVariation = 0.1f;

    private void Awake()
    {
        // Ensure we have an AudioSource component
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlayRandomAudioWithDelay();
        }
    }

    public void PlayRandomAudioWithDelay()
    {
        if (audioClips == null || audioClips.Count == 0)
        {
            Debug.LogWarning("No audio clips assigned to RandomAudioPlayer", this);
            return;
        }

        // Calculate random delay
        float delay = Random.Range(minDelay, maxDelay);

        // Invoke the playback after the delay
        Invoke("PlayRandomAudio", delay);
    }

    private void PlayRandomAudio()
    {
        // Select a random clip
        AudioClip clip = audioClips[Random.Range(0, audioClips.Count)];

        // Apply random pitch variation if needed
        if (pitchVariation > 0)
        {
            audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        }

        // Apply random volume variation if needed
        if (volumeVariation > 0)
        {
            audioSource.volume = Mathf.Clamp01(1f + Random.Range(-volumeVariation, volumeVariation));
        }

        // Play the clip
        audioSource.PlayOneShot(clip);
    }

    // Public method to play from other scripts if needed
    public void PlayRandomClip()
    {
        PlayRandomAudioWithDelay();
    }
}