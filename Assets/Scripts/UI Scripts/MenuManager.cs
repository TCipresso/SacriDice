using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Menu Roots (enable/disable)")]
    public GameObject mainMenuRoot;
    public GameObject pauseMenuRoot;
    public GameObject gameOverMenuRoot;
    public GameObject shopMenuRoot;   // NEW: Shop screen root (Canvas or Panel)
    public GameObject hudRoot;        // optional gameplay HUD

    [Header("Behavior")]
    public KeyCode pauseKey = KeyCode.Escape;
    public bool startInMainMenu = true;

    bool isPaused;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (startInMainMenu) ShowMainMenu();
        else { HideAllMenus(); if (hudRoot) hudRoot.SetActive(true); Resume(); }
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (pauseMenuRoot != null && pauseMenuRoot.activeSelf) Resume();
            else if (!IsAnyMenuOpenExceptHUD()) Pause();
        }
    }

    // ---------------- Public API ----------------
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void Restart()
    {
        Resume();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuRoot) pauseMenuRoot.SetActive(false);
        if (shopMenuRoot) shopMenuRoot.SetActive(false); // ensure shop hides on resume
        if (mainMenuRoot) mainMenuRoot.SetActive(false);
        if (gameOverMenuRoot) gameOverMenuRoot.SetActive(false);
        if (hudRoot) hudRoot.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuRoot) pauseMenuRoot.SetActive(true);
        if (hudRoot) hudRoot.SetActive(false);
        if (mainMenuRoot) mainMenuRoot.SetActive(false);
        if (gameOverMenuRoot) gameOverMenuRoot.SetActive(false);
        if (shopMenuRoot) shopMenuRoot.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void GoToScene(int buildIndex)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(buildIndex);
    }

    public void StartGameInCurrentScene()
    {
        HideAllMenus();
        if (hudRoot) hudRoot.SetActive(true);
        Resume();
    }

    // ------------- New: Shop control -------------
    public void ShowShopMenu()
    {
        Time.timeScale = 0f; // like other menus
        isPaused = true;

        if (shopMenuRoot) shopMenuRoot.SetActive(true);

        if (hudRoot) hudRoot.SetActive(false);
        if (pauseMenuRoot) pauseMenuRoot.SetActive(false);
        if (mainMenuRoot) mainMenuRoot.SetActive(false);
        if (gameOverMenuRoot) gameOverMenuRoot.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HideShopMenu()
    {
        if (shopMenuRoot) shopMenuRoot.SetActive(false);
    }

    // ------------- Helpers -------------
    public void ShowMainMenu()
    {
        Time.timeScale = 0f;
        isPaused = true;

        if (mainMenuRoot) mainMenuRoot.SetActive(true);
        if (pauseMenuRoot) pauseMenuRoot.SetActive(false);
        if (gameOverMenuRoot) gameOverMenuRoot.SetActive(false);
        if (shopMenuRoot) shopMenuRoot.SetActive(false);
        if (hudRoot) hudRoot.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowGameOverMenu()
    {
        Time.timeScale = 0f;
        isPaused = true;

        if (gameOverMenuRoot) gameOverMenuRoot.SetActive(true);
        if (pauseMenuRoot) pauseMenuRoot.SetActive(false);
        if (mainMenuRoot) mainMenuRoot.SetActive(false);
        if (shopMenuRoot) shopMenuRoot.SetActive(false);
        if (hudRoot) hudRoot.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HideAllMenus()
    {
        if (mainMenuRoot) mainMenuRoot.SetActive(false);
        if (pauseMenuRoot) pauseMenuRoot.SetActive(false);
        if (gameOverMenuRoot) gameOverMenuRoot.SetActive(false);
        if (shopMenuRoot) shopMenuRoot.SetActive(false);
    }

    bool IsAnyMenuOpenExceptHUD()
    {
        return (mainMenuRoot && mainMenuRoot.activeSelf)
            || (gameOverMenuRoot && gameOverMenuRoot.activeSelf)
            || (pauseMenuRoot && pauseMenuRoot.activeSelf)
            || (shopMenuRoot && shopMenuRoot.activeSelf);
    }
}
