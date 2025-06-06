using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemySpawner : MonoBehaviour 
{
    [Header("End Game")]
    public EndGameSequenceManager endGameManager;
    
    [Header("Boss Animation")]
    private Transform bossOnStage;
    public float bossJumpDuration = 2f;
    public float bossAnimationCameraDuration = 3f;
    public Vector3 bossCameraOffset = new Vector3(0, 3f, -8f);
    
    [Serializable]
    public class RoundConfiguration
    {
        public EnemyType[] enemyTypes;
        public int[] enemyCounts;
    }

    public enum EnemyType
    {
        SlimeEnemy,
        MageEnemy,
        Boss1Enemy
    }

    [Header("Round Configurations")]
    public RoundConfiguration[] rounds = {
        new RoundConfiguration() { 
            enemyTypes = new[] { EnemyType.SlimeEnemy }, 
            enemyCounts = new[] { 3 } 
        },
        new RoundConfiguration() { 
            enemyTypes = new[] { EnemyType.SlimeEnemy, EnemyType.MageEnemy }, 
            enemyCounts = new[] { 3, 3 } 
        },
        new RoundConfiguration() { 
            enemyTypes = new[] { EnemyType.Boss1Enemy }, 
            enemyCounts = new[] { 1 } 
        }
    };

    [Header("Spawn Settings")]
    public float spawnInterval = 1.0f;
    public Transform spawnPoint;
    public float delayBetweenRounds = 3f;
    public float radius = 60f;          
    public float minDistance = 1f;     
    public const float minDistancePuntosEvitados = 15f;     

    // Store all enemies organized by rounds
    private List<List<Enemy>> allRoundEnemies;
    private HashSet<Enemy> currentActiveEnemies;
    private int currentRound = 0;
    private bool isPaused = false;
    private CirclePointGenerator generadorPuntos;
    
    // Referencias para la animación de la cámara
    private ThirdPersonCamera originalCamera;
    private PlayerController playerController;
    private Camera mainCamera;
    
    [Header("Sol")]
    private Material materialSol;
    float intensidadSol = 4f;
    byte incrementadorIntensidadSol = 3;
    Color colorSol = new Color(191f / 255f, 121f / 255f, 49f / 255f, 1f);

    private int roundIndex;

    private void Start() {
        materialSol = Resources.Load<Material>("Materials/MaterialSolEclipse");
        materialSol.SetColor("_EmissionColor", colorSol * Mathf.Pow(2.0F, intensidadSol));
        
        bossOnStage = GameObject.FindWithTag("PuntoJefe").transform;
            
        generadorPuntos = new CirclePointGenerator(transform.position, radius, minDistance);
        
        // Obtener referencias para la animación de cámara
        originalCamera = FindFirstObjectByType<ThirdPersonCamera>();
        playerController = FindFirstObjectByType<PlayerController>();
        mainCamera = Camera.main;
        
        // Spawn all enemies at the beginning
        SpawnAllEnemies();
    }

    private void SpawnAllEnemies()
    {
        allRoundEnemies = new List<List<Enemy>>();
        
        for (int roundIndex = 0; roundIndex < rounds.Length; roundIndex++)
        {
            List<Enemy> roundEnemies = new List<Enemy>();
            
            // Spawn all enemies for this round
            int enemyId = 0;
            for (int typeIndex = 0; typeIndex < rounds[roundIndex].enemyTypes.Length; typeIndex++)
            {
                EnemyType enemyType = rounds[roundIndex].enemyTypes[typeIndex];
                int count = rounds[roundIndex].enemyCounts[typeIndex];
                
                for (int i = 0; i < count; i++)
                {
                    Enemy newEnemy = SpawnEnemy(enemyType, enemyId, roundIndex);
                    roundEnemies.Add(newEnemy);
                    
                    // Disable the enemy script initially
                    newEnemy.enabled = false;
                    
                    enemyId++;
                }
            }
            
            allRoundEnemies.Add(roundEnemies);
        }
        
        Debug.Log($"Pre-spawned {allRoundEnemies.Count} rounds of enemies");
    }

    public void StartSpawning()
    {
        StartCoroutine(ActivateRounds());
    }

    private IEnumerator ActivateRounds() {
        materialSol.SetColor("_EmissionColor", colorSol * Mathf.Pow(2.0F, intensidadSol));
        
        for (roundIndex = 0; roundIndex < rounds.Length; roundIndex++)
        {
            currentRound = roundIndex;
            currentActiveEnemies = new HashSet<Enemy>();

            // Check if we need boss animation before the last round
            bool isLastRoundBossOnly = IsLastRoundBossOnly();
            
            if (isLastRoundBossOnly && IsLastRound())
            {
                // Execute boss jumping animation
                yield return StartCoroutine(BossJumpAnimation());
            }

            // Activate enemies for this round with intervals
            List<Enemy> roundEnemies = allRoundEnemies[roundIndex];
            
            for (int i = 0; i < roundEnemies.Count; i++)
            {
                Enemy enemy = roundEnemies[i];
                
                // Wait for spawn interval
                if (i > 0) // Don't wait before the first enemy
                {
                    yield return new WaitForSeconds(spawnInterval);
                }
                
                // Wait if paused
                while (isPaused)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                // Activate the enemy
                enemy.gameObject.SetActive(true);
                enemy.enabled = true;
                enemy.gameObject.GetComponent<Rigidbody>().freezeRotation = false;
                currentActiveEnemies.Add(enemy);
                
                Debug.Log($"Activated enemy: {enemy.name}");
            }

            // Wait for all enemies in this round to be defeated
            yield return StartCoroutine(WaitForEnemiesDefeated());
            
            materialSol.SetColor("_EmissionColor", colorSol * Mathf.Pow(2.0F, intensidadSol));
            intensidadSol += incrementadorIntensidadSol;
            if (incrementadorIntensidadSol > 1) {
                incrementadorIntensidadSol--;
            }
            
            // No delay after the last round
            if (roundIndex < rounds.Length - 1)
            {
                yield return new WaitForSeconds(delayBetweenRounds);
            }
        }

        // Start end game sequence
        if (endGameManager != null)
        {
            endGameManager.StartEndGameSequence();
        }
    }
    
    public bool IsLastRoundBossOnly()
    {
        int lastRoundIndex = rounds.Length - 1;
        if (lastRoundIndex < 0) return false;
        
        RoundConfiguration lastRound = rounds[lastRoundIndex];
        
        if (lastRound.enemyTypes.Length != 1) return false;
        
        return lastRound.enemyTypes[0] == EnemyType.Boss1Enemy && 
               lastRound.enemyCounts[0] == 1;
    }
    
    public bool IsLastRound() {
        if (rounds == null) {
            return false;
        }
        return (roundIndex == rounds.Length - 1);
    }
    
    private IEnumerator BossJumpAnimation()
    {
        // Find the boss enemy that was pre-spawned for this round
        List<Enemy> lastRoundEnemies = allRoundEnemies[allRoundEnemies.Count - 1];
        Enemy bossEnemy = lastRoundEnemies.Find(e => e.name.Contains("Boss1Enemy"));
        
        if (bossEnemy != null && bossOnStage != null)
        {
            // Move the boss to the stage position for the animation
            bossEnemy.transform.position = bossOnStage.position;
            bossOnStage = bossEnemy.transform; // Update reference
        }

        Debug.Log("Iniciando animación del jefe saltando del palco");

        // Save original camera state and controls
        Vector3 originalCameraPos = mainCamera.transform.position;
        
        // Disable player controls and original camera
        playerController.enabled = false;
        originalCamera.enabled = false;

        // Hide health UI if it exists
        PlayerHealth playerHealth = playerController.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.healthUI.HideUI();
        }
        
        var menu = FindFirstObjectByType<MainMenuManager>();
        menu.enabled = false;

        Vector3 bossStartPos = bossOnStage.position;
        Vector3 bossEndPos = bossStartPos + (spawnPoint.position - bossStartPos).normalized * 2f;
        bossEndPos.y = 0;

        Vector3 targetCameraPos = bossStartPos + bossCameraOffset;
        
        float elapsedTime = 0f;
        float maxDuration = Mathf.Max(bossAnimationCameraDuration, bossJumpDuration);

        IEnumerator PlayBossFallSoundDelayed()
        {
            yield return new WaitForSeconds(2f); // Wait for 1 second
            AudioManager.Instance.PlaySFX("BossFalling");
        }
        StartCoroutine(PlayBossFallSoundDelayed());

        while (elapsedTime < maxDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // Animate camera
            if (elapsedTime <= bossAnimationCameraDuration)
            {
                float cameraProgress = elapsedTime / bossAnimationCameraDuration;
                float smoothCameraProgress = Mathf.SmoothStep(0f, 1f, cameraProgress);

                Vector3 newCameraPos = Vector3.Lerp(originalCameraPos, targetCameraPos, smoothCameraProgress);
                mainCamera.transform.position = newCameraPos;
                
                mainCamera.transform.LookAt(bossOnStage.position);
            }
            
            // Animate boss jump
            if (elapsedTime <= bossJumpDuration)
            {
                float jumpProgress = elapsedTime / bossJumpDuration;
                float smoothJumpProgress = Mathf.SmoothStep(0f, 1f, jumpProgress);
                
                Vector3 currentPos = Vector3.Lerp(bossStartPos, bossEndPos, smoothJumpProgress);
                
                float jumpHeight = 10f;
                float heightOffset = jumpHeight * Mathf.Sin(jumpProgress * Mathf.PI);
                currentPos.y += heightOffset;
                
                bossOnStage.position = currentPos;
            }

            yield return null;
        }

        // Ensure boss is at final position
        bossOnStage.position = bossEndPos;

        // Restore controls and camera
        playerController.enabled = true;
        originalCamera.enabled = true;
        
        if (playerHealth != null)
        {
            playerHealth.healthUI.ShowUI();
        }
        
        menu.enabled = true;

        Debug.Log("Animación del jefe completada");
    }
    
    private Enemy SpawnEnemy(EnemyType enemyType, int enemyId, int roundIndex) 
    {
        Vector3 position;
    
        // Special positioning for boss enemy
        if (enemyType == EnemyType.Boss1Enemy)
        {
            position = bossOnStage.position;
        }
        else
        {
            position = generadorPuntos.GenerarPunto();
        }

        GameObject gbToInstantiate = null;
        switch (enemyType)
        {
            case EnemyType.SlimeEnemy:
                gbToInstantiate = SlimeEnemy.gameObjectToInstantiate;
                break;
            case EnemyType.MageEnemy:
                gbToInstantiate = MageEnemy.gameObjectToInstantiate;
                break;
            case EnemyType.Boss1Enemy:
                gbToInstantiate = BossEnemy.gameObjectToInstantiate;
                break;
        }
    
        GameObject enemyObject = Instantiate(gbToInstantiate, position, Quaternion.identity);
        enemyObject.name = $"Round{roundIndex}_{enemyType}_{enemyId}";
    
        Vector3 directionToSpawn = (spawnPoint.position - position).normalized;
        enemyObject.transform.rotation = Quaternion.LookRotation(directionToSpawn);

        Rigidbody rb = enemyObject.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.freezeRotation = true;

        Enemy enemyScript = enemyObject.GetComponent<Enemy>();
    
        Debug.Log($"Pre-spawned enemy: {enemyObject.name}");
        return enemyScript;
    }
    
    private IEnumerator WaitForEnemiesDefeated()
    {
        while (currentActiveEnemies.Count > 0)
        {
            // Remove any null enemies (defeated)
            currentActiveEnemies.RemoveWhere(enemy => enemy == null);
            yield return null;
        }
    }

    public void PauseAllEnemies(bool pauseEnemies)
    {
        if (currentActiveEnemies == null) {
            return;
        }
        
        isPaused = pauseEnemies;
        foreach (var enemy in currentActiveEnemies)
        {
            if (enemy != null)
            {
                enemy.enabled = !pauseEnemies;
                Rigidbody rb = enemy.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
    }
    
    public void PauseAllEnemies(bool pauseEnemies, GameObject[] bossMinions)
    {
        if (currentActiveEnemies == null) {
            return;
        }
        
        isPaused = pauseEnemies;
        foreach (var enemy in bossMinions)
        {
            if (enemy != null)
            {
                enemy.GetComponent<Enemy>().enabled = !pauseEnemies;
                Rigidbody rb = enemy.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }

        PauseAllEnemies(pauseEnemies);
    }
    
    // CirclePointGenerator class remains the same...
    private class CirclePointGenerator
    {

        private float radius;          
        private float minDistance;
        
        private Vector3 center;            
        private HashSet<Vector3> puntos;
        private HashSet<Vector3> puntosEvitados;

        public CirclePointGenerator(Vector3 centro, float radius, float minDistance) {
            this.radius = radius;
            this.minDistance = minDistance;
            GameObject[] puntosEvitadosArray = GameObject.FindGameObjectsWithTag("PuntoEvitado");
            puntos = new HashSet<Vector3>();
            puntosEvitados = new HashSet<Vector3>();
            foreach (GameObject punto in puntosEvitadosArray) {
                puntosEvitados.Add(punto.transform.position);
            }
            this.center = centro;
        }

        public Vector3 GenerarPunto() 
        {
            Vector3 newPoint = center;
            bool hayColision = true;
            int maxIntentos = 100; 
            int intentos = 0;
            
            while (hayColision && intentos < maxIntentos) 
            {
                float angle = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
                newPoint = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

                hayColision = false;
                
                foreach (var p in puntosEvitados) 
                {
                    if (Vector3.Distance(newPoint, p) <= minDistancePuntosEvitados) 
                    {
                        hayColision = true;
                        break;
                    }
                }

                if (!hayColision) {
                    foreach (var p in puntos) 
                    {
                        if (Vector3.Distance(newPoint, p) <= minDistance) 
                        {
                            hayColision = true;
                            break;
                        }
                    }
                }
                
                intentos++;
            }
            
            if (intentos >= maxIntentos) 
            {
                Debug.LogWarning("No se pudo encontrar un punto válido después de " + maxIntentos + " intentos");
                return newPoint;
            }
            
            puntos.Add(newPoint);
            Debug.Log("Nuevo punto generado: " + newPoint);
            return newPoint;
        }
    }
}