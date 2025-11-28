using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject tutorialPanel;
    public GameObject creditsPanel;

    [Header("Settings")]
    public string gameSceneName = "Main";

    void Start()
    {
        // Show main menu, hide others
        ShowMainMenu();

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Ensure time is running
        Time.timeScale = 1f;
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void ShowTutorial()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void ShowCredits()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}