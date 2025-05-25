#if UNITY_STANDALONE_WIN || UNITY_WSA
using UnityEngine;
using UnityEngine.Windows.Speech;

public class VoiceHintManager : MonoBehaviour
{
    [Tooltip("Voice commands that trigger hints")]
    [SerializeField] private string[] keywords = { "hint", "help" };

    [Tooltip("AudioSource to play hint clips")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Pre-recorded audio hints about treasure locations")]
    public AudioClip[] hintClips;

    private KeywordRecognizer keywordRecognizer;
    private bool isRecognizerActive = true;

    void Start()
    {
        // Set up audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.LogWarning("No AudioSource found - created one automatically.");
            }
        }

        InitializeVoiceRecognizer();
    }

    private void InitializeVoiceRecognizer()
    {
        if (keywords == null || keywords.Length == 0)
        {
            Debug.LogError("No keywords assigned for voice recognition!");
            return;
        }

        keywordRecognizer = new KeywordRecognizer(keywords, ConfidenceLevel.Low);
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();
        isRecognizerActive = true;
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (!isRecognizerActive) return;

        Debug.Log($"Voice command detected: {args.text}");
        GiveHint();
    }

    public void GiveHint()
    {
        if (hintClips == null || hintClips.Length == 0)
        {
            Debug.LogWarning("No hint clips assigned!");
            return;
        }

        if (audioSource.isPlaying)
            audioSource.Stop();

        int randomIndex = Random.Range(0, hintClips.Length);
        audioSource.PlayOneShot(hintClips[randomIndex]);
    }

    public void ToggleVoiceRecognition(bool state)
    {
        isRecognizerActive = state;
        if (keywordRecognizer == null)
            return;

        if (state && !keywordRecognizer.IsRunning)
            keywordRecognizer.Start();
        else if (!state && keywordRecognizer.IsRunning)
            keywordRecognizer.Stop();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // Pause recognition when app loses focus
        if (keywordRecognizer != null)
            ToggleVoiceRecognition(!pauseStatus);
    }


    void OnDestroy()
    {
        if (keywordRecognizer != null)
        {
            keywordRecognizer.OnPhraseRecognized -= OnPhraseRecognized;
            if (keywordRecognizer.IsRunning)
                keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }
    }

    //void OnApplicationPause(bool pauseStatus)
    //{
    //    // Pause recognition when app loses focus
    //    ToggleVoiceRecognition(!pauseStatus);
    //}
}
#endif
