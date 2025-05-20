using System.Collections.Generic;
using UnityEngine;

public class Hinter : MonoBehaviour
{
    [SerializeField] private List<AudioClip> audioClips; // Assign clips in Inspector
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Call this method from your button's OnClick event
    public void PlayRandomHint()
    {
        if (audioClips != null && audioClips.Count > 0)
        {
            int index = Random.Range(0, audioClips.Count);
            audioSource.clip = audioClips[index];
            audioSource.Play();
        }
    }
}
