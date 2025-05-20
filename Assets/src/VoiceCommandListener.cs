using UnityEngine;
using System.Collections.Generic;

public class VoiceCommandListener : MonoBehaviour, ISpeechToTextListener
{
    [Header("Audio Clips for Commands")]
    public AudioClip hintClip;
    public AudioClip clueClip;
    public AudioClip decryptClip;

    private AudioSource audioSource;

    // Mapping keywords to their corresponding audio clips
    private Dictionary<string, AudioClip> keywordAudioMap;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component missing on this GameObject.");
            return;
        }

        // Initialize the keyword-audio mapping
        keywordAudioMap = new Dictionary<string, AudioClip>
        {
            { "hint", hintClip },
            { "clue", clueClip },
            { "decrypt", decryptClip }
        };

        // Initialize the speech-to-text plugin with the preferred language (e.g., "en-US")
        SpeechToText.Initialize("en-US");

        // Request microphone permission
        SpeechToText.RequestPermissionAsync(permission =>
        {
            if (permission == SpeechToText.Permission.Granted)
            {
                StartListening();
            }
            else
            {
                Debug.LogWarning("Microphone permission denied.");
            }
        });
    }

    void StartListening()
    {
        if (SpeechToText.IsServiceAvailable())
        {
            bool started = SpeechToText.Start(this);
            if (!started)
            {
                Debug.LogError("Failed to start speech recognition.");
            }
        }
        else
        {
            Debug.LogWarning("Speech recognition service is not available.");
        }
    }

    public void OnResultReceived(string spokenText, int? errorCode)
    {
        if (!string.IsNullOrEmpty(spokenText))
        {
            string lowerText = spokenText.ToLower();
            foreach (var keyword in keywordAudioMap.Keys)
            {
                if (lowerText.Contains(keyword))
                {
                    PlayAudio(keywordAudioMap[keyword]);
                    return;
                }
            }
            Debug.Log("No matching keyword found in the spoken text.");
        }
        else
        {
            Debug.LogWarning("Received empty or null speech recognition result.");
        }

        // Restart listening for the next command
        StartListening();
    }

    public void OnPartialResultReceived(string spokenText)
    {
        // Optional: Handle partial results if needed
    }

    public void OnReadyForSpeech()
    {
        Debug.Log("Speech recognition is ready.");
    }

    public void OnBeginningOfSpeech()
    {
        Debug.Log("Speech has started.");
    }

    public void OnVoiceLevelChanged(float normalizedVoiceLevel)
    {
        // Optional: Use this to visualize voice input level
    }

    private void PlayAudio(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
            Debug.Log($"Playing audio for keyword.");
        }
        else
        {
            Debug.LogWarning("AudioClip is null.");
        }
    }
}
