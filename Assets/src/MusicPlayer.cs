using UnityEngine;
using System.Collections.Generic;

public class MusicPlayer : MonoBehaviour
{
    // List of music tracks assignable in the Inspector
    public List<AudioClip> tracks = new List<AudioClip>();

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        PlayRandomMusic();
    }

    void PlayRandomMusic()
    {
        if (tracks.Count == 0)
        {
            Debug.LogWarning("No music tracks assigned.");
            return;
        }

        // Choose a random track from the list
        int trackIndex = Random.Range(0, tracks.Count);
        AudioClip selectedClip = tracks[trackIndex];

        audioSource.clip = selectedClip;
        audioSource.Play();
    }
}
