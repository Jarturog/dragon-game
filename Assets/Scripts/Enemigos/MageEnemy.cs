using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MageEnemy : Enemy
{
    public static GameObject gameObjectToInstantiate;
    protected override float AttackDistance => 15f;
    protected override float FleeDistance => 7.5f;
    
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
    
    protected override void MoveTowardsTarget(Vector3 targetPosition)
    {
        if (!_estaCaminandoAnimacion) {
            _animator.SetTrigger("Caminar");
        }
        
        // Calculate movement direction (horizontal only)
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0;
        direction.Normalize();
        
        // Apply force to move
        rb.AddForce(direction * moveForce, ForceMode.Force);
    }
    
    // Override the AttackPlayer method to shoot projectiles instead
    protected override void AttackPlayer()
    {
        Debug.Log(gameObject.name + " shoots a magical projectile!");
    
        if (projectilePrefab != null)
        {
            _animator.SetTrigger("Atacar");
            AudioManager.Instance.PlaySFX("LanzarProyectil");
            
            // Calculate spawn position at 2/3 of mage's height
            float mageHeight = GetComponent<Collider>().bounds.size.y;
            Vector3 spawnPosition = transform.position + Vector3.up * (mageHeight * 2f/3f) + transform.forward * 1.5f;
        
            // Create projectile at the calculated position
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        
            // Calculate target position at 2/3 of player's height
            float playerHeight = player.GetComponent<Collider>().bounds.size.y;
            Vector3 targetPosition = player.position + Vector3.up * (playerHeight * 2f/3f);
        
            // Get direction to player's chest/head area
            Vector3 direction = (targetPosition - projectile.transform.position).normalized;
        
            // Add some random deviation to allow projectiles to miss
            float accuracy = 0.8f; // 80% accuracy
            if (Random.value > accuracy)
            {
                // Add random deviation to direction
                direction += new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));
                direction.Normalize();
            }
        
            // Check if the projectile already has MageProjectile component
            MageProjectile mageProjectile = projectile.GetComponent<MageProjectile>();
            if (mageProjectile == null)
            {
                mageProjectile = projectile.AddComponent<MageProjectile>();
            }
        
            mageProjectile.Initialize(direction, projectileSpeed, projectileLifetime, attackDamage);
        }
        else
        {
            Debug.LogError("Projectile prefab not assigned on MageEnemy!");
        }
    }

    public override void TakeDamage(float damage) {
        base.TakeDamage(damage);
        AudioManager.Instance.PlaySFX("HitMage");
    }

}