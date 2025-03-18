using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public int numberOfEnemies = 10; // Total enemies to spawn
    public float spawnInterval = 1.0f; // Time between spawns in seconds
    public Transform spawnPoint; // Location where enemies will spawn (optional)
    private HashSet<Enemy> enemies;
    private bool isPaused = false;
    private Coroutine spawnCoroutine;

    public void StartSpawning()
    {
        // Start the spawning coroutine
        enemies = new HashSet<Enemy>(numberOfEnemies);
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        int enemiesSpawned = 0;
        while (enemiesSpawned < numberOfEnemies)
        {
            if (!isPaused)
            {
                // Increment counter
                enemiesSpawned++;
                // Spawn an enemy
                enemies.Add(SpawnEnemy(enemiesSpawned));
                // Wait for the next spawn only if not paused
                yield return new WaitForSeconds(spawnInterval);
            }
            else
            {
                // Just yield for a second while paused
                yield return new WaitForSeconds(1);
            }
        }
    }

    private Enemy SpawnEnemy(int enemyId)
    {
        // If spawnPoint is assigned, spawn at that position
        // Otherwise spawn at the spawner's position
        Vector3 position = (spawnPoint != null) ? spawnPoint.position : transform.position;

        // Create a new GameObject for the enemy
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "EnemyCapsule_" + enemyId;
        enemy.transform.position = position;

        // Add rigidbody for physics
        Rigidbody rb = enemy.AddComponent<Rigidbody>();
        rb.isKinematic = false;

        // Add enemy behavior script
        Enemy enemyScript = enemy.AddComponent<Enemy>();
        Debug.Log("Enemy capsule created: " + enemy.name);

        return enemyScript;
    }

    public void PauseAllEnemies(bool pauseEnemies) {
        isPaused = pauseEnemies;

        foreach (var enemy in enemies.Where(enemy => enemy != null)) {
            enemy.enabled = !pauseEnemies;
            if (pauseEnemies) {
                Rigidbody rb = enemy.GetComponent<Rigidbody>();
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}