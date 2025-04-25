using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MageEnemy : Enemy
{
    public static GameObject gameObjectToInstantiate;
    protected override float AttackDistance => 10f;
    protected override float FleeDistance => 4f;
    
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float projectileLifetime = 5f;
    
    private void Awake() {
        gameObjectToInstantiate = GameObject.FindWithTag("MageEnemy");
        if (gameObjectToInstantiate == null) {
            Debug.LogError("UEP: mage is null");
        }
        
        GetComponent<Enemy>().enabled = false;
    }
    
    // Override the AttackPlayer method to shoot projectiles instead
    protected override void AttackPlayer()
    {
        Debug.Log(gameObject.name + " shoots a magical projectile!");
        
        if (projectilePrefab != null)
        {
            // Create projectile
            GameObject projectile = Instantiate(projectilePrefab, transform.position + transform.forward * 1.5f, Quaternion.identity);
            
            // Get direction to player with some randomness for "miss" chance
            Vector3 direction = (player.position - projectile.transform.position).normalized;
            
            // Add some random deviation to allow projectiles to miss
            float accuracy = 0.8f; // 80% accuracy
            if (Random.value > accuracy)
            {
                // Add random deviation to direction
                direction += new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));
                direction.Normalize();
            }
            
            // Add projectile component and initialize
            MageProjectile mageProjectile = projectile.AddComponent<MageProjectile>();
            mageProjectile.Initialize(direction, projectileSpeed, projectileLifetime, attackDamage);
        }
        else
        {
            Debug.LogError("Projectile prefab not assigned on MageEnemy!");
        }
    }
}