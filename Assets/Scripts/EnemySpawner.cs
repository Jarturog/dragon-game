using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour 
{
    public GameObject enemyPrefab;     // The enemy prefab to spawn
    public int numberOfEnemies = 10;   // Total enemies to spawn
    public float spawnInterval = 1.0f; // Time between spawns in seconds
    public Transform spawnPoint;       // Location where enemies will spawn (optional)
   
    private void Start()
    {
        // Start the spawning coroutine
        StartCoroutine(SpawnEnemies());
    }
   
    private IEnumerator SpawnEnemies()
    {
        int enemiesSpawned = 0;
       
        while (enemiesSpawned < numberOfEnemies)
        {
            // Spawn an enemy
            SpawnEnemy();
           
            // Increment counter
            enemiesSpawned++;
           
            // Wait for the next spawn
            yield return new WaitForSeconds(spawnInterval);
        }
    }
   
    private void SpawnEnemy()
    {
        // If spawnPoint is assigned, spawn at that position
        // Otherwise spawn at the spawner's position
        Vector3 position = (spawnPoint != null) ? spawnPoint.position : transform.position;
       
        // Instantiate the enemy
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
       
        // You can add additional initialization here if needed
        Debug.Log("Enemy spawned: " + enemy.name);
    }
}