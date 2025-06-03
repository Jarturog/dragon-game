using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemySpawner : MonoBehaviour 
{
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

    private HashSet<Enemy> currentEnemies;
    private int currentRound = 0;
    private bool isPaused = false;
    private CirclePointGenerator generadorPuntos;

    private void Start() {
        generadorPuntos = new CirclePointGenerator(transform.position);
    }

    public void StartSpawning()
    {
        StartCoroutine(SpawnRounds());
    }

    private IEnumerator SpawnRounds()
    {
        for (int roundIndex = 0; roundIndex < rounds.Length; roundIndex++) {
            currentRound = roundIndex;
            currentEnemies = new HashSet<Enemy>();

            bool cambios = true;
            // Intercalar spawns de diferentes tipos de enemigos
            for (int spawnIndex = 0; cambios; spawnIndex++) {
                cambios = false;
                for (int typeIndex = 0; typeIndex < rounds[roundIndex].enemyTypes.Length; typeIndex++) {
                    // Solo spawna si aún quedan enemigos de este tipo por spawnar
                    if (spawnIndex < rounds[roundIndex].enemyCounts[typeIndex]) {
                        cambios = true;
                        EnemyType enemyType = rounds[roundIndex].enemyTypes[typeIndex];
                        //Debug.Log("spawneando intervalo " + spawnInterval);
                        yield return new WaitForSeconds(spawnInterval);
                        //Debug.Log("esperados 5s");
                        while (isPaused) {
                            yield return new WaitForSeconds(1);
                            //Debug.Log("esperados 1s");
                        }

                        Enemy newEnemy = SpawnEnemy(enemyType, spawnIndex);
                        currentEnemies.Add(newEnemy);
                    }
                }
            }

            // Esperar a que todos los enemigos sean derrotados
            yield return StartCoroutine(WaitForEnemiesDefeated());

            // Esperar entre rondas
            yield return new WaitForSeconds(delayBetweenRounds);
        }

        GameObject.FindWithTag("skybox").GetComponent<Skybox>().material = Resources.Load<Material>("Materials/Skybox/skybox dia");
        GameObject.FindWithTag("Luna").SetActive(false);
    }

    private Enemy SpawnEnemy(EnemyType enemyType, int enemyId) {
        Vector3 position = generadorPuntos.GenerarPunto();//(spawnPoint != null) ? spawnPoint.position : transform.position;

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
                gbToInstantiate = Boss1Enemy.gameObjectToInstantiate;
                break;
        }
        
        GameObject enemyObject = Instantiate(gbToInstantiate, position, Quaternion.identity);
        enemyObject.name = $"{enemyType}_" + enemyId;

        Rigidbody rb = enemyObject.AddComponent<Rigidbody>();
        rb.isKinematic = false;

        Enemy enemyScript = enemyObject.GetComponent<Enemy>();
        enemyScript.enabled = true;
        
        Debug.Log($"Enemy created: {enemyObject.name}");
        return enemyScript;
    }

    private IEnumerator WaitForEnemiesDefeated()
    {
        while (currentEnemies.Count > 0)
        {
            // Remove any null enemies (defeated)
            currentEnemies.RemoveWhere(enemy => enemy == null);
            yield return null;
        }
    }

    public void PauseAllEnemies(bool pauseEnemies)
    {
        if (currentEnemies == null) {
            return;
        }
        
        isPaused = pauseEnemies;
        foreach (var enemy in currentEnemies)
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
    
    public class CirclePointGenerator
    {
        public float radius = 20f;          // Radio del círculo
        public float minDistance = 1f;     // Distancia mínima entre puntos

        private Vector3 center;            // Centro del círculo
        private HashSet<Vector3> puntos;   // Conjunto de puntos generados

        public CirclePointGenerator(Vector3 centro) 
        {
            puntos = new HashSet<Vector3>();
            puntos.Add(new Vector3(25, 10, -5));
            puntos.Add(new Vector3(25, 10, -4));
            puntos.Add(new Vector3(25, 10, -3));
            puntos.Add(new Vector3(25, 10, -2));
            puntos.Add(new Vector3(25, 10, -1));
            puntos.Add(new Vector3(25, 10, 0));
            puntos.Add(new Vector3(25, 10, 1));
            puntos.Add(new Vector3(25, 10, 2));
            puntos.Add(new Vector3(25, 10, 3));
            puntos.Add(new Vector3(25, 10, 4));
            puntos.Add(new Vector3(25, 10, 5));
            this.center = centro;
        }

        public Vector3 GenerarPunto() 
        {
            Vector3 newPoint = center;
            bool hayColision = true;
            int maxIntentos = 100; // Previene bucles infinitos
            int intentos = 0;
            
            while (hayColision && intentos < maxIntentos) 
            {
                // Genera un ángulo aleatorio
                float angle = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
                
                // Calcula un punto en la circunferencia (horizontalmente, en plano XZ)
                newPoint = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

                hayColision = false;
                foreach (var p in puntos) 
                {
                    if (Vector3.Distance(newPoint, p) <= minDistance) 
                    {
                        hayColision = true;
                        break;
                    }
                }
                
                intentos++;

            }
            
            // Si no pudimos encontrar un punto después de muchos intentos
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