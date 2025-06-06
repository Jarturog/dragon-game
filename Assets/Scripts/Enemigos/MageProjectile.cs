using System;
using Unity.VisualScripting;
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
        // Override certain enemy behaviors for projectile
        moveForce = 0f; // Don't use force-based movement
        maxSpeed = speed;
        _direction = direction;
        _speed = speed;
        _destroyTime = Time.time + lifetime;
        attackDamage = damage;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        // Add rigidbody for collision detection only
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null) {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = false;
        rb.isKinematic = true; // Prevents physics interference
        
        transform.rotation = Quaternion.LookRotation(direction);
    }
    
    protected new void Start()
    {
        base.Start(); // Call the base Enemy Start method
        
        // Override certain enemy behaviors for projectile
        moveForce = 0f; // Don't use force-based movement
        maxSpeed = _speed;
        
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null) {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = false;
        rb.isKinematic = true; // Prevents physics interference
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
        }
        else if (other.tag.EndsWith("Enemy", StringComparison.InvariantCultureIgnoreCase) && !other.CompareTag("MageEnemy"))
        {
            other.GetComponent<Enemy>().TakeDamage(attackDamage);
            Debug.Log("Projectile hit enemy for " + attackDamage + " damage!");
        } else if (other.CompareTag("Projectile"))
        {
            Debug.Log("Projectile hit projectile!");
        }

        if (!other.CompareTag("MageEnemy")) {
            Destroy(gameObject);
            Debug.Log("Projectile hit something!");
        }
        
    }
}
