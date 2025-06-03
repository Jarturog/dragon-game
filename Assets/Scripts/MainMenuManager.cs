using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenuManager : MonoBehaviour
{
    [Header("Camera References")]
    // Remove mainCamera reference - we'll only use thirdPersonCamera
    public ThirdPersonCamera thirdPersonCamera;
    
    [Header("Player References")]
    public PlayerController playerController;
    public PlayerHealth playerHealthUI;
    
    [Header("Game Systems")]
    public EnemySpawner enemySpawner;
    public VideoPlayer videoPlayer;
    
    [Header("UI References")]
    public Canvas menuCanvas;
    public Button playButton;
    public Button quitButton;
    
    [Header("Video Skip Settings")]
    public float skipHoldTime = 2f;
    private float escHoldTimer = 0f;
    private bool isHoldingEsc = false;
    
    private static MainMenuManager instance;
    private bool hasBeenPaused = false;
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        instance = this;
    }

    private void Start() 
    {
        InitializeComponents();
        SetupEvents();
        SetupInitialState();
    }

    private void Update() 
    {
        HandleInput();
    }
    
    private void OnDestroy()
    {
        CleanupEvents();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeComponents()
    {
        // Find missing components
        if (thirdPersonCamera == null)
            thirdPersonCamera = FindFirstObjectByType<ThirdPersonCamera>();

        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        if (playerHealthUI == null)
            playerHealthUI = FindFirstObjectByType<PlayerHealth>();
    }
    
    private void SetupEvents()
    {
        // Button events
        if (playButton != null)
            playButton.onClick.AddListener(StartGame);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        
        // Video events
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoFinished;
        
        // Scene events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void CleanupEvents()
    {
        // Button events
        if (playButton != null)
            playButton.onClick.RemoveListener(StartGame);
        
        if (quitButton != null)
            quitButton.onClick.RemoveListener(QuitGame);
        
        // Video events
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
        
        // Scene events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    #endregion
    
    #region Game State Management
    
    private void SetupInitialState()
    {
        SetCameraState(enableMovement: false);
        SetPlayerState(enabled: false);
        SetUIState(showMenu: true, showCursor: true);
    }
    
    private void SetCameraState(bool enableMovement)
    {
        if (thirdPersonCamera != null)
        {
            // Always keep the third person camera active
            thirdPersonCamera.gameObject.SetActive(true);
            thirdPersonCamera.enabled = enableMovement;
            
            // Control cursor based on camera movement state
            if (enableMovement)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                thirdPersonCamera.UnlockCursor();
            }
        }
    }
    
    private void SetPlayerState(bool enabled)
    {
        if (playerController != null)
            playerController.enabled = enabled;
        
        if (playerHealthUI?.healthUI != null)
        {
            if (enabled)
                playerHealthUI.healthUI.ShowUI();
            else
                playerHealthUI.healthUI.HideUI();
        }
        else {
            Debug.Log("playerHealthUI sin definir");
        }
    }
    
    private void SetUIState(bool showMenu, bool showCursor)
    {
        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(showMenu);
        
        if (showCursor)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
    
    #endregion
    
    #region Input Handling
    
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (hasBeenPaused)
            {
                ResumeGame();
            }
            else if (videoPlayer != null && videoPlayer.isPlaying)
            {
                // Start ESC hold timer for video skip
                isHoldingEsc = true;
                escHoldTimer = 0f;
            }
            else
            {
                // Normal gameplay - pause the game
                ShowPauseMenu();
            }
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (isHoldingEsc && videoPlayer != null && videoPlayer.isPlaying)
            {
                // ESC released before skipHoldTime - pause video and show menu
                videoPlayer.Pause();
                ShowPauseMenu();
            }
    
            isHoldingEsc = false;
            escHoldTimer = 0f;
        }

        // Handle ESC hold timer during video
        if (isHoldingEsc && videoPlayer != null && videoPlayer.isPlaying)
        {
            escHoldTimer += Time.deltaTime;
    
            if (escHoldTimer >= skipHoldTime)
            {
                // Skip video after holding ESC for skipHoldTime seconds
                if (videoPlayer != null && videoPlayer.isPlaying)
                {
                    videoPlayer.Stop();
                    InitializeGameplay();
                }
                isHoldingEsc = false;
                escHoldTimer = 0f;
            }
        }
    }
    
    #endregion
    
    #region Game Flow
    
    void StartGame()
    {
        if (hasBeenPaused)
        {
            ResumeGame();
        }
        else
        {
            StartNewGame();
        }
    }
    
    private void StartNewGame()
    {
        // Hide player health UI during video
        if (playerHealthUI != null)
            playerHealthUI.gameObject.SetActive(false);
        
        // Start intro video
        if (videoPlayer != null)
            videoPlayer.Play();
        
        SetUIState(showMenu: false, showCursor: false);
    }
    
    private void ResumeGame()
    {
        hasBeenPaused = false;
    
        // Check if we were in video mode
        if (videoPlayer != null && videoPlayer.isPaused)
        {
            // Resume video playback
            videoPlayer.Play();
            SetUIState(showMenu: false, showCursor: false);
        }
        else
        {
            // Resume normal gameplay
            SetCameraState(enableMovement: true);
            SetPlayerState(enabled: true);
            SetUIState(showMenu: false, showCursor: false);
        
            if (enemySpawner != null)
                enemySpawner.PauseAllEnemies(false);
        }
    }
    
    public void ShowPauseMenu()
    {
        hasBeenPaused = true;
        
        // Keep third person camera active but disable movement
        SetCameraState(enableMovement: false);
        SetPlayerState(enabled: false);
        SetUIState(showMenu: true, showCursor: true);
        
        if (enemySpawner != null)
            enemySpawner.PauseAllEnemies(true);
    }
    
    private void InitializeGameplay()
    {
        if (playerHealthUI != null)
            playerHealthUI.gameObject.SetActive(true);
        if (playerHealthUI.healthUI != null)
        {
            playerHealthUI.healthUI.ShowUI();
        }
    
        SetCameraState(enableMovement: true);
        SetPlayerState(enabled: true); // ← Esta línea faltaba
    
        if (enemySpawner != null)
            enemySpawner.StartSpawning();
    }
    
    private void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnVideoFinished(VideoPlayer vp)
    {
        isHoldingEsc = false;
        escHoldTimer = 0f;
        InitializeGameplay();
    }
    
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (instance != null)
        {
            instance.SetupInitialState();
        }
    }
    
    #endregion
    
    #region Static Methods
    
    public static void ReturnToMainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    #endregion
}