using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public Toggle challengeModeToggle;
    //public void StartGame()
    //{
    //    // Replace "GameScene" with the name of your game scene
    //    SceneManager.LoadScene("game");
    //}
    public void PlayGame()
    {
        PlayerPrefs.SetInt("ChallengeModeEnabled", challengeModeToggle.isOn ? 1 : 0);
        SceneManager.LoadScene("game");
    }
    public void QuitGame()
    {
        // Quit the application
        Application.Quit();

        // For testing in the editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
