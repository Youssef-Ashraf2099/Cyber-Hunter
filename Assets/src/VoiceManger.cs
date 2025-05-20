using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
using UnityEngine.Windows.Speech;
#endif

[RequireComponent(typeof(AudioSource))]
public class VoiceHintManager : MonoBehaviour
{
    [SerializeField] private string[] keywords = { "hint", "help" };
    [SerializeField] private AudioSource audioSource;
    public AudioClip[] hintClips;

    private KeywordRecognizer keywordRecognizer;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

#if UNITY_ANDROID
        // Check and request microphone permission
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
            Invoke(nameof(InitializeVoiceRecognizer), 1f); // Retry after delay
            return;
        }
#endif

        InitializeVoiceRecognizer();
    }

    private void InitializeVoiceRecognizer()
    {
        if (keywords == null || keywords.Length == 0)
        {
            Debug.LogError("No keywords assigned in Inspector!");
            return;
        }

        try
        {
            keywordRecognizer = new KeywordRecognizer(keywords, ConfidenceLevel.Low);
            keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
            keywordRecognizer.Start();
            Debug.Log("Voice recognizer started successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Voice recognizer failed: " + e.Message);
        }
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (hintClips == null || hintClips.Length == 0)
        {
            Debug.LogWarning("No hint clips assigned!");
            return;
        }

        if (audioSource.isPlaying)
            audioSource.Stop();

        audioSource.PlayOneShot(hintClips[Random.Range(0, hintClips.Length)]);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (keywordRecognizer == null) return;

        if (pauseStatus) keywordRecognizer.Stop();
        else if (!keywordRecognizer.IsRunning) keywordRecognizer.Start();
    }

    void OnDestroy()
    {
        if (keywordRecognizer != null)
        {
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }
    }
}