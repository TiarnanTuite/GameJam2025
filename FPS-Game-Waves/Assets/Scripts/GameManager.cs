using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject pauseMenu;
    public GameObject deathScreen;
    public GameObject gameHUD;

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";
    public string gameSceneName = "Game";

    private bool isPaused = false;
    private bool isDead = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Initialize UI states
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (deathScreen != null) deathScreen.SetActive(false);
        if (gameHUD != null) gameHUD.SetActive(true);

        ResumeGame();
    }

    void Update()
    {
        // Press ESC to pause/unpause
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && !isDead)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenu != null) pauseMenu.SetActive(true);
        if (gameHUD != null) gameHUD.SetActive(false);

        // Unlock cursor for menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (gameHUD != null) gameHUD.SetActive(true);

        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowDeathScreen()
    {
        isDead = true;
        Time.timeScale = 0f;

        if (deathScreen != null) deathScreen.SetActive(true);
        if (gameHUD != null) gameHUD.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        isDead = false;
        isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        isDead = false;
        isPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public bool IsPaused() => isPaused;
    public bool IsDead() => isDead;
}
