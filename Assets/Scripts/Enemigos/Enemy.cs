using System.Collections;
using Unity.Mathematics.Geometry;
using UnityEngine;
using Math = System.Math;

public abstract class Enemy : MonoBehaviour
{
    public float maxHealth = 100f;
    public float health = 100f;
    protected EnemyHealthBar _healthBar;
    
    public Transform player;
    
    protected Rigidbody rb;
    
    [Header("Enemy Settings")]
    public float moveForce = 50f;
    public float maxSpeed = 5f;
    public float attackCooldown = 1f;
    public float attackDamage = 10f;
    private float _attackAnimationLength;
    protected abstract float AttackDistance { get; }
    protected abstract float FleeDistance { get; }
    
    private float _lastAttackTime;
    
    // State pattern for cleaner behavior management
    protected enum EnemyState
    {
        Approach,
        Attack,
        Flee,
        Idle,
        Dead
    }
    
    protected EnemyState currentState = EnemyState.Idle;

    protected Animator _animator;
    protected bool _estaCaminandoAnimacion, _estaAtacandoAnimacion;

    protected virtual void Start() {
        gameObject.SetActive(true);
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogWarning("Player was not found! Enemy behavior will be disabled.");
        }
        
        // Get component
        rb = GetComponent<Rigidbody>();
        
        // Setup Rigidbody
        if (rb != null)
        {
            rb.freezeRotation = true;  // Prevents tipping over
            rb.useGravity = true;
            rb.isKinematic = false;
            // Add drag for more natural movement
            rb.linearDamping = 1f;
        }
        
        _animator = GetComponentInChildren<Animator>();
        
        // Set initial state
        _lastAttackTime = -attackCooldown; // Allow immediate attack if in range
        
        _healthBar = gameObject.AddComponent<EnemyHealthBar>();
        _healthBar.Initialize();
        
        AnimationClip[] clips = _animator.runtimeAnimatorController.animationClips;
        _attackAnimationLength = 1f; // Default fallback

        foreach (AnimationClip clip in clips)
        {
            if (clip.name.Contains("Atacar"))
            {
                _attackAnimationLength = clip.length;
                break;
            }
        }
    }

    void FixedUpdate()
    {
        if (player == null || rb == null || currentState == EnemyState.Dead) {
            return;
        }

        // Determine state based on distance
        UpdateEnemyState();
    
        // Execute behavior based on current state
        ExecuteCurrentState();
    }

    protected virtual void UpdateEnemyState() 
    {
        if (currentState == EnemyState.Dead)
            return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
    
        // Determine state based on distance
        if (distanceToPlayer < FleeDistance)
        {
            currentState = EnemyState.Flee;
        }
        else if (distanceToPlayer <= AttackDistance)
        {
            currentState = EnemyState.Attack;
        }
        else
        {
            currentState = EnemyState.Approach;
        }
    }
    
    protected virtual void ExecuteCurrentState()
    {
        if (player == null) {
            player = GameObject.FindWithTag("Player").transform;
        }
        FacePlayer();
        
        switch (currentState)
        {
            case EnemyState.Approach:
                MoveTowardsTarget(player.position);
                break;
                
            case EnemyState.Flee:
                var position = transform.position;
                Vector3 fleePosition = position + (position - player.position).normalized * 10f;
                MoveTowardsTarget(fleePosition);
                break;
                
            case EnemyState.Attack:
                StopMoving();
                AttackPlayer();
                break;
                
            case EnemyState.Idle:
                StopMoving();
                break;
        }
        
        // Always limit speed after movement
        LimitSpeed();
    }
    
    protected virtual void MoveTowardsTarget(Vector3 targetPosition)
    {
        // Calculate movement direction (horizontal only)
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0;
        direction.Normalize();
        
        // Apply force to move
        rb.AddForce(direction * moveForce, ForceMode.Force);
    }

    protected void StopMoving()
    {
        // Apply force opposite to current velocity to slow down
        Vector3 opposingForce = -rb.linearVelocity * (moveForce * 0.8f);
        opposingForce.y = 0; // Don't affect vertical movement
        rb.AddForce(opposingForce, ForceMode.Force);
    }
    
    protected void LimitSpeed()
    {
        // Get horizontal velocity (ignore y)
        var linearVelocity = rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(linearVelocity.x, 0, linearVelocity.z);
        
        // If over max speed, cap it
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            // Preserve vertical velocity
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }
    }
    
    protected void FacePlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0; // Keep rotation on horizontal plane
        
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
    
    protected virtual void AttackPlayer()
    {
        if (Time.time < _lastAttackTime + attackCooldown || _estaAtacandoAnimacion)
        {
            return;
        }
    
        _lastAttackTime = Time.time;
        Debug.Log(gameObject.name + " starts attack animation!");

        _animator.SetTrigger("Atacar");
        StartCoroutine(CheckAttackAfterAnimation(_attackAnimationLength, attackDamage));
    }

    protected IEnumerator CheckAttackAfterAnimation(float animationDuration, float danyoAtaque)
    {
        // Wait for the attack animation to complete
        yield return new WaitForSeconds(Math.Max(animationDuration - 1f, 0.1f));
    
        // Check if player is still in attack range after animation
        if (player != null && Vector3.Distance(transform.position, player.position) <= AttackDistance)
        {
            // Apply damage only if still in range
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null) 
            {
                playerHealth.TakeDamage(danyoAtaque);
                Debug.Log(gameObject.name + " successfully hits player for " + danyoAtaque + " damage!");
            }
        }
        else
        {
            Debug.Log(gameObject.name + " attack missed - player out of range!");
        }
    }

    public virtual void TakeDamage(float damage)
    {
        if (currentState == EnemyState.Dead) {
            return;
        }
        
        health -= damage;
        Debug.Log(gameObject.name + " took " + damage + " damage. Remaining health: " + health);

        if (_healthBar != null)
        {
            _healthBar.UpdateHealthBar(health, maxHealth);
        }

        if (health <= 0)
        {
            currentState = EnemyState.Dead;  // Set state to dead
            _animator.SetTrigger("Morir");
        }
    }

    public IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log(gameObject.name + " has been defeated!");
        Destroy(gameObject);
    }

    public void setEstaCaminandoAnimacion(bool b) {
        _estaCaminandoAnimacion = b;
        Debug.Log("Enemigo está caminando: " + b);
    }

    public void setEstaAtacandoAnimacion(bool b) {
        _estaAtacandoAnimacion = b;
    }
}