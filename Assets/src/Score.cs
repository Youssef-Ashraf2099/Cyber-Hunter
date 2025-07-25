using UnityEngine;
using TMPro; // Use TextMeshPro instead of UnityEngine.UI

public class Score : MonoBehaviour
{
    public TextMeshProUGUI scoreText; // Reference to the TMP UI component
    private int score = 0; // Variable to store the score

    public int CurrentScore => score;

    void Start()
    {
        // Initialize the score text
        UpdateScoreText();
    }

    // Method to increment the score
    public void IncrementScore()
    {
        score++;
        UpdateScoreText();

        // Notify GameEndManager about score change
        GameEndManager endManager = FindObjectOfType<GameEndManager>();
        if (endManager != null)
        {
            endManager.SetScore(score);
        }
    }

    // Update the score text in the UI
    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }
}
