using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string endGameSceneName = "EndGame";
    [SerializeField] public int targetScore = 2;
    [SerializeField] private float endSceneDelay = 1f; // Delay after sound before loading scene

    [Header("Game End Sounds")]
    [SerializeField] private AudioClip[] endSounds;
    [SerializeField] private AudioSource audioSource;

    private int currentScore = 0;
    private bool gameEnded = false;
    private bool waitingForQuestionnaire = false;

    void Awake()
    {
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void SetScore(int score)
    {
        currentScore = score;
        CheckGameEnd();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        CheckGameEnd();
    }

   
    private void CheckGameEnd()
    {
        if (!gameEnded && currentScore >= targetScore)
        {
            TapHandler tapHandler = FindObjectOfType<TapHandler>();
            if (tapHandler != null && tapHandler.isQuestionnaireActive)
            {
                waitingForQuestionnaire = true;
                Debug.Log("Waiting for questionnaire to complete before ending game");
            }
            else
            {
                StartCoroutine(EndGameRoutine());
            }
        }
    }



    public void OnQuestionnaireCompleted()
    {
        if (waitingForQuestionnaire)
        {
            waitingForQuestionnaire = false;

            // Add a small delay to allow prefab destruction to complete smoothly
            StartCoroutine(DelayedEndGameRoutine(0.5f));
        }
    }

    private IEnumerator DelayedEndGameRoutine(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        StartCoroutine(EndGameRoutine());
    }


    private IEnumerator EndGameRoutine()
    {
        if (gameEnded) yield break;

        gameEnded = true;
        PlayRandomEndSound();

        // Wait for sound to play and any delay
        yield return new WaitForSeconds(endSceneDelay);

        LoadEndGameScene();
    }

    private void PlayRandomEndSound()
    {
        if (endSounds != null && endSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = endSounds[Random.Range(0, endSounds.Length)];
            audioSource.PlayOneShot(clip);
        }
    }

    private void LoadEndGameScene()
    {
        if (!string.IsNullOrEmpty(endGameSceneName))
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(endGameSceneName);
        }
        else
        {
            Debug.LogError("End game scene name not set!");
        }
    }
}