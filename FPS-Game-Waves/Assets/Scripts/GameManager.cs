using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject pauseMenu;
    public GameObject deathScreen;
    public GameObject gameHUD;

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";
    public string gameSceneName = "Main";

    private bool isPaused = false;
    private bool isDead = false;
    private GameObject playerObject;

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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindUIReferences();
        FindPlayer();

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

        // Reconnect buttons after scene load
        ReconnectButtons();
    }

    void ReconnectButtons()
    {
        // Reconnect Pause Menu buttons
        if (pauseMenu != null)
        {
            ConnectButton(pauseMenu, "ResumeButton", ResumeGame);
            ConnectButton(pauseMenu, "RestartButton", RestartGame);
            ConnectButton(pauseMenu, "MainMenuButton", LoadMainMenu);
            ConnectButton(pauseMenu, "QuitButton", QuitGame);
        }

        // Reconnect Death Screen buttons
        if (deathScreen != null)
        {
            ConnectButton(deathScreen, "RestartButton", RestartGame);
            ConnectButton(deathScreen, "MainMenuButton", LoadMainMenu);
            ConnectButton(deathScreen, "QuitButton", QuitGame);
        }
    }

    void ConnectButton(GameObject parent, string buttonName, UnityEngine.Events.UnityAction action)
    {
        Transform buttonTransform = parent.transform.Find(buttonName);
        if (buttonTransform != null)
        {
            UnityEngine.UI.Button button = buttonTransform.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                // Clear existing listeners and add new one
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(action);
            }
        }
    }

    void FindPlayer()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            Debug.Log("Found Player");
        }
        else
        {
            Debug.LogError("Player with 'Player' tag not found!");
        }
    }

    void SetPlayerEnabled(bool enabled)
    {
        if (playerObject != null)
        {
            MonoBehaviour[] scripts = playerObject.GetComponentsInChildren<MonoBehaviour>();
            foreach (var script in scripts)
            {
                if (script is PlayerHealth || script is HUDController)
                    continue;

                script.enabled = enabled;
            }
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

        StartCoroutine(DelayedHealthUpdate());
    }

    System.Collections.IEnumerator DelayedHealthUpdate()
    {
        yield return new WaitForSeconds(0.1f);
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        HUDController hud = FindFirstObjectByType<HUDController>();
        if (playerHealth != null && hud != null)
        {
            hud.UpdateHealth(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
        }
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
        FindPlayer();
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

        SetPlayerEnabled(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (gameHUD != null) gameHUD.SetActive(true);

        SetPlayerEnabled(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowDeathScreen()
    {
        isDead = true;
        Time.timeScale = 0f;

        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
            UpdateDeathStats();
        }

        if (gameHUD != null) gameHUD.SetActive(false);

        SetPlayerEnabled(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void UpdateDeathStats()
    {
        if (deathScreen != null)
        {
            Transform statsTransform = deathScreen.transform.Find("StatsText");
            if (statsTransform != null)
            {
                TextMeshProUGUI statsText = statsTransform.GetComponent<TextMeshProUGUI>();
                if (statsText != null)
                {
                    HUDController hud = FindFirstObjectByType<HUDController>();
                    if (hud != null)
                    {
                        int kills = hud.GetKillCount();
                        statsText.text = $"ENEMIES ELIMINATED: {kills}";
                    }
                }
            }
            else
            {
                Debug.LogWarning("StatsText not found on DeathScreen!");
            }
        }
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