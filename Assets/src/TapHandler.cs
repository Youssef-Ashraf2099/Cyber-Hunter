using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Networking;
public class TapHandler : MonoBehaviour
{
    [System.Serializable]
    public class InteractivePrefab
    {
        [Tooltip("The prefab that will be interacted with")]
        public GameObject prefab;

        [Tooltip("Sound to play when this prefab is tapped")]
        public AudioClip interactionSound;

        [Tooltip("Questionnaire panel for this prefab")]
        public GameObject questionnairePanel;

        [Tooltip("Text component to display the question")]
        public Text questionText;

        [Tooltip("Input field for player's answer")]
        public InputField answerInputField;

        [Tooltip("Question data including correct answer")]
        public Question questionData;
    }

    [Header("Configuration")]
    [Tooltip("List of interactive prefabs and their settings")]
    public List<InteractivePrefab> interactivePrefabs = new List<InteractivePrefab>();

    [Tooltip("Reference to the score manager")]
    public Score scoreManager;

    [Header("Audio")]
    [Tooltip("Main audio source for playing sounds")]
    public AudioSource audioSource;

    public bool isQuestionnaireActive = false;
    private InteractivePrefab currentActivePrefab;

    
    [Header("Game End")]
    public GameEndManager gameEndManager;

    public AICompanionBehavior aiCompanion; // Assign in Inspector


    void Start()
    {
        // Create audio source if not assigned
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Initialize all questionnaire panels
        InitializeQuestionnaires();
    }

    private void InitializeQuestionnaires()
    {
        foreach (var prefabData in interactivePrefabs)
        {
            if (prefabData.questionnairePanel != null)
            {
                prefabData.questionnairePanel.SetActive(false);

                // Verify button setup
                Button submitButton = prefabData.questionnairePanel.GetComponentInChildren<Button>();
                if (submitButton != null)
                {
                    submitButton.onClick.AddListener(OnQuestionnaireSubmit);
                    Debug.Log($"Submit button found and listener added for {prefabData.prefab.name}");
                }
                else
                {
                    Debug.LogError($"No submit button found in questionnaire panel for {prefabData.prefab.name}");
                }
            }

            // Ensure the prefab has a collider
            if (prefabData.prefab.GetComponent<Collider>() == null)
            {
                Debug.LogError($"Prefab {prefabData.prefab.name} is missing a Collider component!");
            }
        }
    }

    void Update()
    {
        if (isQuestionnaireActive) return;

        HandleInput();
    }

    private void HandleInput()
    {
        bool inputDetected = false;
        Vector2 inputPosition = Vector2.zero;

        // Handle both new and old input systems
#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        var touch = Touchscreen.current;

        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            inputDetected = true;
            inputPosition = mouse.position.ReadValue();
        }
        else if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
        {
            inputDetected = true;
            inputPosition = touch.primaryTouch.position.ReadValue();
        }
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            inputDetected = true;
            inputPosition = Input.GetTouch(0).position;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            inputDetected = true;
            inputPosition = Input.mousePosition;
        }
#endif

        if (inputDetected)
        {
            CheckForPrefabTap(inputPosition);
        }
    }

    private void CheckForPrefabTap(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            foreach (var prefabData in interactivePrefabs)
            {
                if (hit.transform.gameObject == prefabData.prefab)
                {
                    ProcessPrefabTap(prefabData);
                    break;
                }
            }
        }
    }

    //private void SendAnswerToGoogleForm(string entryID, string answer)
    //{
    //    StartCoroutine(PostToGoogleForm(entryID, answer));
    //}

    //private IEnumerator PostToGoogleForm(string entryID, string answer)
    //{
    //    string formURL = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSebFjYhjmr6Uefx95lP0z3NY_nGIYqBkwScwqUIwSIyKT0jOw/formResponse";
    //    WWWForm form = new WWWForm();

    //    if (string.IsNullOrEmpty(entryID))
    //    {
    //        Debug.LogError("Google Form Entry ID is missing!");
    //        yield break;
    //    }

    //    form.AddField(entryID, answer);

    //    UnityWebRequest www = UnityWebRequest.Post(formURL, form);
    //    yield return www.SendWebRequest();

    //    if (www.result != UnityWebRequest.Result.Success)
    //    {
    //        Debug.LogError("Form submission failed: " + www.error);
    //    }
    //    else
    //    {
    //        Debug.Log("Form submitted successfully.");
    //    }
    //}

    private void ProcessPrefabTap(InteractivePrefab prefabData)
    {
        if (isQuestionnaireActive || currentActivePrefab != null) return;

        currentActivePrefab = prefabData;
        PlayInteractionSound(prefabData);

        if (prefabData.questionnairePanel != null)
        {
            // Wait for user to submit, score will be handled after
            ShowQuestionnaire(prefabData);
        }
        else
        {
            // No questionnaire? Process immediately
            if (scoreManager != null)
                scoreManager.IncrementScore();

            StartCoroutine(DestroyPrefabAfterSound(prefabData));
            currentActivePrefab = null;
        }
    }


    private void PlayInteractionSound(InteractivePrefab prefabData)
    {
        if (audioSource != null && prefabData.interactionSound != null)
        {
            audioSource.PlayOneShot(prefabData.interactionSound);
        }
    }

    private void ShowQuestionnaire(InteractivePrefab prefabData)
    {
        if (prefabData.questionnairePanel != null && prefabData.questionData != null)
        {
            isQuestionnaireActive = true;
            prefabData.questionnairePanel.SetActive(true);
            Time.timeScale = 0;

            // Set up questionnaire content
            if (prefabData.questionText != null)
            {
                prefabData.questionText.text = prefabData.questionData.questionText;
            }

            if (prefabData.answerInputField != null)
            {
                prefabData.answerInputField.text = "";
                prefabData.answerInputField.ActivateInputField();
            }
        }
    }


    public void OnQuestionnaireSubmit()
    {
        if (currentActivePrefab == null) return;

        string playerAnswer = currentActivePrefab.answerInputField != null ?
            currentActivePrefab.answerInputField.text.Trim() : "";

        if (string.IsNullOrEmpty(playerAnswer))
        {
            Debug.LogWarning("Player submitted an empty answer!");
            return;
        }

        currentActivePrefab.questionnairePanel.SetActive(false);
        Time.timeScale = 1;
        isQuestionnaireActive = false;



       // SendAnswerToGoogleForm(currentActivePrefab.questionData.googleFormEntryID, playerAnswer);
        // Notify GameEndManager FIRST (before destroying prefab)
        if (gameEndManager != null)
        {
            gameEndManager.OnQuestionnaireCompleted();
        }

        // Increment score LAST (triggers CheckGameEnd)
        if (scoreManager != null)
        {
            scoreManager.IncrementScore();
        }

        // Destroy the prefab after the sound
        StartCoroutine(DestroyPrefabAfterSound(currentActivePrefab));

        currentActivePrefab = null;

    }



    // Helper to get score from Score manager (add a public getter in Score.cs)
    private int GetScoreFromScoreManager()
    {
        // Add a public property in Score.cs: public int CurrentScore => score;
        return scoreManager.CurrentScore;
    }


    private IEnumerator DestroyPrefabAfterSound(InteractivePrefab prefabData)
    {
        if (prefabData.interactionSound != null)
        {
            yield return new WaitForSecondsRealtime(prefabData.interactionSound.length);
        }
        else
        {
            yield return null;
        }

        if (prefabData.prefab != null)
        {
            Destroy(prefabData.prefab);
        }
    }
}


[System.Serializable]
public class Question
{
    public string questionText;
    public string correctAnswer;
    public string[] options;

    [Tooltip("Google Form Entry ID for this question (e.g., entry.1234567890)")]
    public string googleFormEntryID;
}
