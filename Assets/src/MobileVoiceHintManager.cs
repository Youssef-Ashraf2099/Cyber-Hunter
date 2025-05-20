using UnityEngine;
using KKSpeech;
using System.Collections;
using System.Collections.Generic;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class MobileVoiceHintManager : MonoBehaviour
{
    [Tooltip("Voice commands that trigger hints (case-insensitive)")]
    [SerializeField] private string[] keywords = { "guide me", "help", "decrypt" };

    [Tooltip("AudioSource to play hint clips")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Pre-recorded audio hints about treasure locations")]
    public AudioClip[] hintClips;

    [Tooltip("Time in seconds to wait before retrying speech recognition after an error")]
    [SerializeField] private float retryDelay = 2f;

    [Tooltip("Debug mode - logs detailed information about speech recognition")]
    [SerializeField] private bool debugMode = true;

    private bool isListening = false;
    private bool permissionGranted = false;
    private SpeechRecognizerListener listener;
    private bool isInitialized = false;
    private Coroutine retryCoroutine;

    void Start()
    {
        // Set up audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                LogMessage("No AudioSource found - created one automatically.", LogType.Warning);
            }
        }

        // Request microphone permission
        RequestMicrophonePermission();
    }

    private void RequestMicrophonePermission()
    {
        LogMessage("Requesting microphone permission...");

#if PLATFORM_ANDROID
        // Android-specific permission handling
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
            // Check if permission dialog is showing by checking again immediately
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                // We need to wait for the response, so start a coroutine
                StartCoroutine(CheckPermissionAfterRequest());
                return;
            }
        }
#endif

        // For iOS or if Android permission already granted
        StartCoroutine(RequestMicrophoneAuthorizationAsync());
    }

#if PLATFORM_ANDROID
    private IEnumerator CheckPermissionAfterRequest()
    {
        float timeElapsed = 0;
        float timeOut = 10f; // 10 seconds timeout

        // Wait for the user to respond to the system permission dialog
        while (!Permission.HasUserAuthorizedPermission(Permission.Microphone) && timeElapsed < timeOut)
        {
            yield return new WaitForSeconds(0.5f);
            timeElapsed += 0.5f;
        }

        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            LogMessage("Android microphone permission granted");
            permissionGranted = true;
            StartCoroutine(RequestMicrophoneAuthorizationAsync());
        }
        else
        {
            LogMessage("Android microphone permission denied or timed out", LogType.Error);
            permissionGranted = false;
        }
    }
#endif

    private IEnumerator RequestMicrophoneAuthorizationAsync()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);

        if (Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            LogMessage("Unity microphone authorization granted");
            permissionGranted = true;
            InitializeSpeechRecognizer();
        }
        else
        {
            LogMessage("Unity microphone authorization denied", LogType.Error);
            permissionGranted = false;
        }
    }

    private void InitializeSpeechRecognizer()
    {
        if (isInitialized)
            return;

        if (!SpeechRecognizer.ExistsOnDevice())
        {
            LogMessage("Speech recognition is not supported on this device.", LogType.Warning);
            return;
        }

        // Find or create the listener
        listener = FindObjectOfType<SpeechRecognizerListener>();
        if (listener == null)
        {
            GameObject listenerObj = new GameObject("SpeechRecognizerListener");
            listener = listenerObj.AddComponent<SpeechRecognizerListener>();
            LogMessage("Created new SpeechRecognizerListener object");
        }

        // Make sure to unsubscribe first to avoid duplicate events
        UnsubscribeFromEvents();

        // Subscribe to events
        listener.onFinalResults.AddListener(OnFinalResults);
        listener.onPartialResults.AddListener(OnPartialResults);
        listener.onErrorDuringRecording.AddListener(OnSpeechError);
        listener.onErrorOnStartRecording.AddListener(OnStartRecordingError);
        listener.onAvailabilityChanged.AddListener(OnAvailabilityChanged);
        listener.onReadyForSpeech.AddListener(OnReadyForSpeech);
        listener.onEndOfSpeech.AddListener(OnEndOfSpeech);
        listener.onAuthorizationStatusFetched.AddListener(OnAuthorizationStatusFetched);

        LogMessage("Initialized speech recognizer and subscribed to events");
        isInitialized = true;

        // Check the authorization status explicitly
        AuthorizationStatus status = SpeechRecognizer.GetAuthorizationStatus();
        OnAuthorizationStatusFetched(status);
    }

    private void OnAuthorizationStatusFetched(AuthorizationStatus status)
    {
        LogMessage("Speech recognizer authorization status: " + status);

        switch (status)
        {
            case AuthorizationStatus.Authorized:
                permissionGranted = true;
                StartListening();
                break;
            case AuthorizationStatus.Denied:
            case AuthorizationStatus.Restricted:
                permissionGranted = false;
                LogMessage("Speech recognition permission denied or restricted", LogType.Warning);
                break;
            case AuthorizationStatus.NotDetermined:
                // Request authorization explicitly
                SpeechRecognizer.RequestAccess();
                break;
        }
    }

    private void OnReadyForSpeech()
    {
        LogMessage("Ready for speech");
    }

    private void OnEndOfSpeech()
    {
        LogMessage("End of speech detected");

        // Auto-restart listening after a brief pause
        if (permissionGranted && isInitialized)
        {
            CancelRetryCoroutine();
            retryCoroutine = StartCoroutine(RetryListeningAfterDelay(1f));
        }
    }

    public void StartListening()
    {
        if (!permissionGranted)
        {
            LogMessage("Cannot start listening - permission not granted", LogType.Warning);
            return;
        }

        if (!isInitialized)
        {
            LogMessage("Cannot start listening - speech recognizer not initialized", LogType.Warning);
            return;
        }

        if (isListening || SpeechRecognizer.IsRecording())
        {
            LogMessage("Already listening");
            return;
        }

        try
        {
            // Create simplified options without the problematic fields
            // This is the key fix - remove the fields causing the NoSuchFieldError
            SpeechRecognizer.StartRecording(true); // Just use the simple version with boolean parameter

            isListening = true;
            LogMessage("Started listening for voice commands");
        }
        catch (System.Exception e)
        {
            LogMessage("Error starting speech recognition: " + e.Message, LogType.Error);

            // Try to restart after delay
            CancelRetryCoroutine();
            retryCoroutine = StartCoroutine(RetryListeningAfterDelay(retryDelay));
        }
    }

    private void StopListening()
    {
        if (isListening || SpeechRecognizer.IsRecording())
        {
            SpeechRecognizer.StopIfRecording();
            isListening = false;
            LogMessage("Stopped listening for voice commands");
        }
    }

    private void OnPartialResults(string recognizedText)
    {
        if (string.IsNullOrEmpty(recognizedText) || !debugMode)
            return;

        LogMessage("Partial recognized: " + recognizedText);
    }

    private void OnFinalResults(string recognizedText)
    {
        if (string.IsNullOrEmpty(recognizedText))
            return;

        LogMessage("Final recognized: " + recognizedText);

        string lowerText = recognizedText.ToLowerInvariant();
        bool keywordFound = false;

        foreach (var keyword in keywords)
        {
            if (lowerText.Contains(keyword.ToLowerInvariant()))
            {
                GiveHint();
                keywordFound = true;
                break;
            }
        }

        // Restart listening after getting results
        CancelRetryCoroutine();
        retryCoroutine = StartCoroutine(RetryListeningAfterDelay(keywordFound ? 2f : 0.5f));
    }

    private void OnSpeechError(string error)
    {
        LogMessage("Speech recognition error: " + error, LogType.Warning);
        isListening = false;

        // Try to restart after delay
        CancelRetryCoroutine();
        retryCoroutine = StartCoroutine(RetryListeningAfterDelay(retryDelay));
    }

    private void OnStartRecordingError(string error)
    {
        LogMessage("Failed to start recording: " + error, LogType.Error);
        isListening = false;

        // Try to restart after delay
        CancelRetryCoroutine();
        retryCoroutine = StartCoroutine(RetryListeningAfterDelay(retryDelay * 2));
    }

    private void OnAvailabilityChanged(bool available)
    {
        LogMessage("Speech recognition available: " + available);

        if (available)
        {
            if (permissionGranted && isInitialized && !isListening)
            {
                CancelRetryCoroutine();
                retryCoroutine = StartCoroutine(RetryListeningAfterDelay(1f));
            }
        }
        else
        {
            StopListening();
        }
    }

    private IEnumerator RetryListeningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (permissionGranted && isInitialized && !isListening && !SpeechRecognizer.IsRecording())
        {
            StartListening();
        }
    }

    private void CancelRetryCoroutine()
    {
        if (retryCoroutine != null)
        {
            StopCoroutine(retryCoroutine);
            retryCoroutine = null;
        }
    }

    public void GiveHint()
    {
        if (hintClips == null || hintClips.Length == 0)
        {
            LogMessage("No hint clips assigned!", LogType.Warning);
            return;
        }

        if (audioSource.isPlaying)
            audioSource.Stop();

        int randomIndex = Random.Range(0, hintClips.Length);
        audioSource.PlayOneShot(hintClips[randomIndex]);
        LogMessage("Playing hint clip: " + hintClips[randomIndex].name);
    }

    private void UnsubscribeFromEvents()
    {
        if (listener == null)
            return;

        listener.onFinalResults.RemoveListener(OnFinalResults);
        listener.onPartialResults.RemoveListener(OnPartialResults);
        listener.onErrorDuringRecording.RemoveListener(OnSpeechError);
        listener.onErrorOnStartRecording.RemoveListener(OnStartRecordingError);
        listener.onAvailabilityChanged.RemoveListener(OnAvailabilityChanged);
        listener.onReadyForSpeech.RemoveListener(OnReadyForSpeech);
        listener.onEndOfSpeech.RemoveListener(OnEndOfSpeech);
        listener.onAuthorizationStatusFetched.RemoveListener(OnAuthorizationStatusFetched);
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
        StopListening();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            StopListening();
        }
        else if (permissionGranted && isInitialized)
        {
            CancelRetryCoroutine();
            retryCoroutine = StartCoroutine(RetryListeningAfterDelay(1f));
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            StopListening();
        }
        else if (permissionGranted && isInitialized)
        {
            CancelRetryCoroutine();
            retryCoroutine = StartCoroutine(RetryListeningAfterDelay(1f));
        }
    }

    // Helper method for consistent logging
    private void LogMessage(string message, LogType logType = LogType.Log)
    {
        if (!debugMode && logType == LogType.Log)
            return;

        switch (logType)
        {
            case LogType.Warning:
                Debug.LogWarning("[VoiceHint] " + message);
                break;
            case LogType.Error:
                Debug.LogError("[VoiceHint] " + message);
                break;
            default:
                Debug.Log("[VoiceHint] " + message);
                break;
        }
    }
}