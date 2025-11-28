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

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Unsubscribe when destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-find UI references after scene loads
        FindUIReferences();

        // Initialize based on scene
        if (scene.name == gameSceneName)
        {
            InitializeGameScene();
        }
        else if (scene.name == mainMenuSceneName)
        {
            InitializeMainMenu();
        }
    }

    void FindUIReferences()
    {
        // Try to find UI elements by name
        GameObject canvas = GameObject.Find("Canvas");

        if (canvas != null)
        {
            Transform pauseTransform = canvas.transform.Find("PauseMenu");
            if (pauseTransform != null) pauseMenu = pauseTransform.gameObject;

            Transform deathTransform = canvas.transform.Find("DeathScreen");
            if (deathTransform != null) deathScreen = deathTransform.gameObject;

            Transform hudTransform = canvas.transform.Find("GameHUD");
            if (hudTransform != null) gameHUD = hudTransform.gameObject;
        }
    }

    void InitializeGameScene()
    {
        isDead = false;
        isPaused = false;

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (deathScreen != null) deathScreen.SetActive(false);
        if (gameHUD != null) gameHUD.SetActive(true);

        ResumeGame();
    }

    void InitializeMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Start()
    {
        FindUIReferences();
        InitializeGameScene();
    }

    void Update()
    {
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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (gameHUD != null) gameHUD.SetActive(true);

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