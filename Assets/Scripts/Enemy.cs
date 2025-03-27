using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public float maxHealth = 100f;
    public float health = 100f;
    private EnemyHealthBar _healthBar;
    
    public Transform player;
    
    private Rigidbody _rb;
    
    [Header("Enemy Settings")]
    public float moveForce = 50f;
    public float maxSpeed = 5f;
    public float attackDistance = 1.5f;
    public float attackCooldown = 1f;
    public float attackDamage = 10f;
    
    // State machine variables
    private enum EnemyState { Approach, Attack }
    private EnemyState _currentState;
    private float _lastAttackTime;

    protected void Start() {
        gameObject.SetActive(true);
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.Log("Player was not found");
        }
        
        // Get component
        _rb = GetComponent<Rigidbody>();
        
        // Setup Rigidbody
        if (_rb != null)
        {
            _rb.freezeRotation = true;  // Prevents tipping over
            _rb.useGravity = true;
            _rb.isKinematic = false;
        }
        
        // Set initial state
        _currentState = EnemyState.Approach;
        _lastAttackTime = -attackCooldown; // Allow immediate attack if in range
        
        _healthBar = gameObject.AddComponent<EnemyHealthBar>();
        _healthBar.Initialize();
    }

    void Update()
    {
        if (player == null)
            return;
            
        // Determine state based on distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // State transitions
        switch (_currentState)
        {
            case EnemyState.Approach:
                if (distanceToPlayer <= attackDistance)
                {
                    _currentState = EnemyState.Attack;
                }
                break;
                
            case EnemyState.Attack:
                if (distanceToPlayer > attackDistance)
                {
                    _currentState = EnemyState.Approach;
                }
                else if (Time.time >= _lastAttackTime + attackCooldown)
                {
                    AttackPlayer();
                    _lastAttackTime = Time.time;
                }
                break;
        }
    }
    
    void FixedUpdate()
    {
        if (player == null || _rb == null)
            return;
            
        // Execute current state behavior
        switch (_currentState)
        {
            case EnemyState.Approach:
                ApproachPlayer();
                break;
                
            case EnemyState.Attack:
                // Attack handling is in Update for timing
                FacePlayer();
                break;
        }
    }
    
    private void ApproachPlayer()
    {
        // Calculate direction to player
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0; // Keep movement on the horizontal plane
        directionToPlayer.Normalize();
        
        // Apply force to move the enemy
        _rb.AddForce(directionToPlayer * moveForce, ForceMode.Force);
        
        // Limit speed
        Vector3 linearVelocity = _rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(linearVelocity.x, 0, linearVelocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            _rb.linearVelocity = new Vector3(horizontalVelocity.x, _rb.linearVelocity.y, horizontalVelocity.z);
        }
        
        // Rotate to face player
        FacePlayer();
    }
    
    private void FacePlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0; // Keep rotation on horizontal plane
        
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
    
    private void AttackPlayer()
    {
        Debug.Log(gameObject.name + " attacks player for " + attackDamage + " damage!");
    
        // Get the PlayerHealth component and apply damage
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null) 
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log(gameObject.name + " recibió " + damage + " puntos de daño. Salud restante: " + health);
    
        if (_healthBar != null)
        {
            _healthBar.UpdateHealthBar(health, maxHealth);
        }
    
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " ha sido derrotado!");
        Destroy(gameObject);
    }
}