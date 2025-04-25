using System;
using UnityEngine;

// Projectile class for the mage's attacks
public class MageProjectile : Enemy
{
    private Vector3 _direction;
    private float _speed;
    private float _destroyTime;
    
    // Override the abstract properties
    protected override float AttackDistance => 0.5f; // Only attack when very close
    protected override float FleeDistance => 0f; // Never flee
    
    public void Initialize(Vector3 direction, float speed, float lifetime, float damage)
    {
        _direction = direction;
        _speed = speed;
        _destroyTime = Time.time + lifetime;
        attackDamage = damage;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Set initial rotation to face direction
        transform.rotation = Quaternion.LookRotation(_direction);
    }
    
    protected new void Start()
    {
        base.Start(); // Call the base Enemy Start method
        
        // Override certain enemy behaviors for projectile
        moveForce = 0f; // Don't use force-based movement
        maxSpeed = _speed;
    }
    
    protected void FixedUpdate()
    {
        // Don't call base.FixedUpdate() to avoid standard enemy behavior
        
        // Move projectile in the direction
        transform.position += _direction * (_speed * Time.fixedDeltaTime);
        
        // Destroy after lifetime
        if (Time.time >= _destroyTime)
        {
            Destroy(gameObject);
        }
    }
    
    // Override UpdateEnemyState to always be in attack state
    protected override void UpdateEnemyState()
    {
        currentState = EnemyState.Attack;
    }
    
    // Override ExecuteCurrentState to do nothing (we handle movement ourselves)
    protected override void ExecuteCurrentState()
    {
        // No standard enemy movement/behavior
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Apply damage to player
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log("Projectile hit player for " + attackDamage + " damage!");
            }
            
            // Destroy projectile on hit
            Destroy(gameObject);
        }
        else if (!other.tag.EndsWith("Enemy", StringComparison.InvariantCultureIgnoreCase) && !other.CompareTag("Projectile"))
        {
            // Hit something else (like a wall) - destroy projectile
            Destroy(gameObject);
        }
    }
    
    // Override unnecessary methods
    public override void TakeDamage(float damage)
    {
        // Projectiles can't take damage
    }
}
