using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class submiter : MonoBehaviour
{
    [System.Serializable]
    public class QuestionData
    {
        public string questionText;
        public string googleFormEntryID; // e.g., "entry.1234567890"
    }

    [Header("UI References")]
    public Text questionText;
    public InputField answerInputField;
    public Button submitButton;

    [Header("Questions")]
    public List<QuestionData> questions = new List<QuestionData>();

    [Header("Google Form")]
    [Tooltip("Paste the Google Form 'formResponse' URL here")]
    public string googleFormUrl;

    private int currentQuestionIndex = 0;
    private List<string> playerAnswers = new List<string>();

    void Start()
    {
        if (questions.Count == 0)
        {
            Debug.LogError("No questions assigned!");
            return;
        }

        playerAnswers = new List<string>(new string[questions.Count]);
        submitButton.onClick.AddListener(OnSubmitAnswer);
        ShowCurrentQuestion();
    }

    void ShowCurrentQuestion()
    {
        if (currentQuestionIndex < questions.Count)
        {
            questionText.text = questions[currentQuestionIndex].questionText;
            answerInputField.text = "";
            answerInputField.ActivateInputField();
        }
    }

    void OnSubmitAnswer()
    {
        string answer = answerInputField.text.Trim();
        if (string.IsNullOrEmpty(answer))
        {
            Debug.LogWarning("Answer is empty!");
            return;
        }

        playerAnswers[currentQuestionIndex] = answer;
        currentQuestionIndex++;

        if (currentQuestionIndex < questions.Count)
        {
            ShowCurrentQuestion();
        }
        else
        {
            // All questions answered, upload to Google Form
            submitButton.interactable = false;
            StartCoroutine(UploadAnswersAndProceed());
        }
    }

    IEnumerator UploadAnswersAndProceed()
    {
        WWWForm form = new WWWForm();
        for (int i = 0; i < questions.Count; i++)
        {
            form.AddField(questions[i].googleFormEntryID, playerAnswers[i]);
        }

        using (UnityWebRequest www = UnityWebRequest.Post(googleFormUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to submit answers: " + www.error);
            }
            else
            {
                Debug.Log("Answers submitted successfully!");
            }
        }

        // Proceed to EndGame scene
        SceneManager.LoadScene("EndGame");
    }
}
