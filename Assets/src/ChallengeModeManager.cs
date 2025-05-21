using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChallengeModeManager : MonoBehaviour
{
    [Header("Challenge Settings")]
    public int targetScore = 7;
    public float timeLimitSeconds = 300f; // 5 minutes

    [Header("Scene Names")]
    public string winSceneName = "EndGame";
    public string loseSceneName = "LostScene";

    private float timer;
    private bool challengeActive = true;
    private Score scoreManager;
    private bool gameEnded = false;

    [Header("UI Reference")]
    public TextMeshProUGUI timerText;

    [Header("Audio")]
    public AudioClip loseAudioClip;
    public AudioSource audioSource;
    public float loseAudioDelay = 0.5f; // Optional: extra delay after audio

    void Start()
    {
       // PlayerPrefs.SetInt("ChallengeModeEnabled", 1);
        // Only run if challenge mode is enabled
        if (PlayerPrefs.GetInt("ChallengeModeEnabled", 0) != 1)
        {
            this.enabled = false;
            if (timerText != null)
                timerText.gameObject.SetActive(false);
            return;
        }

        timer = timeLimitSeconds;
        scoreManager = FindObjectOfType<Score>();
        UpdateTimerUI();
    }

    void Update()
    {
        if (!challengeActive || gameEnded) return;

        timer -= Time.deltaTime;
        UpdateTimerUI();

        // Check for win
        if (scoreManager != null && scoreManager.CurrentScore >= targetScore)
        {
            EndChallenge(true);
        }
        // Check for lose
        else if (timer <= 0f)
        {
            EndChallenge(false);
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timer / 60f);
            int seconds = Mathf.FloorToInt(timer % 60f);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }

    private void EndChallenge(bool won)
    {
        gameEnded = true;
        challengeActive = false;
        PlayerPrefs.SetString("ChallengeResult", won ? "Win" : "Lose");
        PlayerPrefs.SetInt("FinalScore", scoreManager != null ? scoreManager.CurrentScore : 0);

        if (won)
        {
            SceneManager.LoadScene(winSceneName);
        }
        else
        {
            if (audioSource != null && loseAudioClip != null)
            {
                audioSource.PlayOneShot(loseAudioClip);
                StartCoroutine(LoadLoseSceneAfterAudio());
            }
            else
            {
                SceneManager.LoadScene(loseSceneName);
            }
        }
    }

    private System.Collections.IEnumerator LoadLoseSceneAfterAudio()
    {
        float waitTime = loseAudioClip != null ? loseAudioClip.length + loseAudioDelay : 1f;
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(loseSceneName);
    }
    public float GetTimeRemaining() => Mathf.Max(0, timer);
}

