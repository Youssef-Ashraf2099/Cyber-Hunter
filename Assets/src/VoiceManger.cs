using UnityEngine;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
#endif

public class VoiceHintManager : MonoBehaviour
{
    [Tooltip("Voice commands that trigger hints")]
    [SerializeField] private string[] keywords = { "hint", "help" };

    [Tooltip("AudioSource to play hint clips")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Pre-recorded audio hints about treasure locations")]
    public AudioClip[] hintClips;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private KeywordRecognizer keywordRecognizer;
#endif
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

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        InitializeVoiceRecognizer();
#else
        Debug.LogWarning("Voice recognition is only supported on Windows in this implementation.");
#endif
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
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
#endif

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
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        isRecognizerActive = state;
        if (state && keywordRecognizer != null && !keywordRecognizer.IsRunning)
            keywordRecognizer.Start();
        else if (!state && keywordRecognizer != null && keywordRecognizer.IsRunning)
            keywordRecognizer.Stop();
#else
        Debug.LogWarning("Voice recognition toggle is not supported on this platform.");
#endif
    }

    void OnDestroy()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (keywordRecognizer != null)
        {
            keywordRecognizer.OnPhraseRecognized -= OnPhraseRecognized;
            if (keywordRecognizer.IsRunning)
                keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }
#endif
    }

    void OnApplicationPause(bool pauseStatus)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        ToggleVoiceRecognition(!pauseStatus);
#endif
    }
}
