using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public Camera mainCamera;
    public ThirdPersonCamera thirdPersonCamera;
    public PlayerController playerController;
    public PlayerHealth.PlayerHealthUI playerHealthUI;
    public EnemySpawner enemySpawner;
    public Canvas menuCanvas;
    public Button playButton;
    public Button quitButton;
    private static MainMenuManager instance;
    private bool hasBeenPaused = false;

    private void Awake()
    {
        instance = this;
    }
    
    private void Start()
    {
        // Asegurarnos de que tenemos referencias a todos los componentes necesarios
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        if (thirdPersonCamera == null)
            thirdPersonCamera = FindFirstObjectByType<ThirdPersonCamera>();
            
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();
            
        if (playerHealthUI == null)
            playerHealthUI = FindFirstObjectByType<PlayerHealth>().healthUI;
        
        // Configurar estado inicial
        SetupInitialState();
        
        // Configurar eventos de botones
        playButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void Update() {
        if (hasBeenPaused && Input.GetKeyDown(KeyCode.Escape)) {
            StartGame();
        }
    }

    private void SetupInitialState()
    {
        mainCamera.gameObject.SetActive(true);
        thirdPersonCamera.gameObject.SetActive(false);
        
        playerController.enabled = false;
        playerHealthUI?.HideUI();
        
        // Mostrar el cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Mostrar el menú
        menuCanvas.gameObject.SetActive(true);
    }

    private void StartGame()
    {
        if (hasBeenPaused) {
            thirdPersonCamera.enabled = true;
            
            enemySpawner.PauseAllEnemies(false);
            
            hasBeenPaused = false;
        }
        else {
            mainCamera.gameObject.SetActive(false);
            thirdPersonCamera.gameObject.SetActive(true);
            
            enemySpawner.StartSpawning();
        }
        
        // Activar el control del jugador
        playerController.enabled = true;
        playerHealthUI.ShowUI();
        
        // Ocultar el menú
        menuCanvas.gameObject.SetActive(false);
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void ShowPauseMenu() {
        hasBeenPaused = true;
        enemySpawner.PauseAllEnemies(true);

        playerController.enabled = false;
        playerHealthUI.HideUI();

        thirdPersonCamera.enabled = false;
        
        // Mostrar el cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Mostrar el menú
        menuCanvas.gameObject.SetActive(true);
    }
    
    // Método para ser llamado desde DeathScreen
    public static void ReturnToMainMenu() {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // This runs after the scene is reloaded
        if (instance != null) {
            instance.SetupInitialState();
        }
        SceneManager.sceneLoaded -= OnSceneLoaded; // Remove the callback
    }
}