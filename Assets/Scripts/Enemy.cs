using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health = 100f;
    public float speed = 3.5f;
    public Transform player;

    private HealthBar _healthBar;
    private Rigidbody rb;
    
    [Header("Enemy Settings")]
    public float moveForce = 50f;
    public float maxSpeed = 5f;
    public float attackDistance = 1.5f;
    public float attackCooldown = 1f;
    public float attackDamage = 10f;
    
    // State machine variables
    private enum EnemyState { Approach, Attack }
    private EnemyState currentState;
    private float lastAttackTime;

    void Start() {
        gameObject.SetActive(true);
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        
        // Get component
        rb = GetComponent<Rigidbody>();
        
        // Setup Rigidbody
        if (rb != null)
        {
            rb.freezeRotation = true;  // Prevents tipping over
            rb.useGravity = true;
            rb.isKinematic = false;
        }
        
        // Set initial state
        currentState = EnemyState.Approach;
        lastAttackTime = -attackCooldown; // Allow immediate attack if in range
        
        _healthBar = new HealthBar(transform);
    }

    void Update()
    {
        if (player == null)
            return;
            
        // Determine state based on distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // State transitions
        switch (currentState)
        {
            case EnemyState.Approach:
                if (distanceToPlayer <= attackDistance)
                {
                    currentState = EnemyState.Attack;
                }
                break;
                
            case EnemyState.Attack:
                if (distanceToPlayer > attackDistance)
                {
                    currentState = EnemyState.Approach;
                }
                else if (Time.time >= lastAttackTime + attackCooldown)
                {
                    AttackPlayer();
                    lastAttackTime = Time.time;
                }
                break;
        }
        
        _healthBar.UpdateHealthBarPosition();
    }
    
    void FixedUpdate()
    {
        if (player == null || rb == null)
            return;
            
        // Execute current state behavior
        switch (currentState)
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
        rb.AddForce(directionToPlayer * moveForce, ForceMode.Force);
        
        // Limit speed
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
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
        _healthBar.UpdateHealthBar(health);
        
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " ha sido derrotado!");
        _healthBar.Die();
        Destroy(gameObject);
    }
    
    class HealthBar {
        private GameObject healthBarPlane;
        private Material healthBarMaterial;
        private Transform parentTransform; // Store reference to enemy transform
    
        public HealthBar(Transform enemyTransform) {
            parentTransform = enemyTransform; // Store reference but don't parent
        
            healthBarPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            // Remove collider components to prevent physics issues
            Destroy(healthBarPlane.GetComponent<Collider>());
            Destroy(healthBarPlane.GetComponent<Rigidbody>());

            // Don't set parent
            healthBarPlane.transform.localScale = new Vector3(1f, 0.1f, 1f);
        
            // Position above the enemy
            healthBarPlane.transform.position = parentTransform.position + new Vector3(0, 2, 0);

            healthBarMaterial = new Material(Shader.Find("Unlit/Color"));
            healthBarMaterial.color = Color.green;
            healthBarPlane.GetComponent<Renderer>().material = healthBarMaterial;
        }

        public void UpdateHealthBar(float health)
        {
            if (healthBarMaterial != null)
            {
                healthBarMaterial.color = Color.Lerp(Color.red, Color.green, health / 100f);
                healthBarPlane.transform.localScale = new Vector3(health / 100f, 0.1f, 1f);
            }
        }

        public void UpdateHealthBarPosition()
        {
            if (healthBarPlane != null && Camera.main != null)
            {
                // Update position to follow the enemy
                healthBarPlane.transform.position = parentTransform.position + new Vector3(0, 2, 0);
        
                // Make the health bar always face the camera
                healthBarPlane.transform.rotation = Camera.main.transform.rotation;
        
                // Alternative approach - directly face the camera's position:
                // healthBarPlane.transform.LookAt(Camera.main.transform);
                // healthBarPlane.transform.rotation *= Quaternion.Euler(0, 180, 0); // Flip it to face camera
            }
        }

        public void Die() {
            Destroy(healthBarPlane);
        }
    }
    
}