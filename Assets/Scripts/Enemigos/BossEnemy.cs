using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossEnemy : Enemy
{
    public static GameObject gameObjectToInstantiate;
    
    protected override float AttackDistance => 8f;
    protected override float FleeDistance => 0f; // Boss never flees
    
    [Header("Boss UI")]
    private bool isSoloBoss = false;
    private BossHealthUI bossUI;
    
    [Header("Boss Phase Settings")]
    private int currentPhase = 1;
    private bool hasEnteredPhase2 = false;
    private bool hasEnteredPhase3 = false;
    
    [Header("Minion Spawning")]
    public float slimeSpawnInterval = 10f;
    public float skeletonSpawnInterval = 10f;
    private float lastSlimeSpawnTime;
    private float lastSkeletonSpawnTime;
    
    [Header("Attack Settings")]
    public float attack1Cooldown = 2f;
    public float attack2Cooldown = 3f;
    public float attack3Cooldown = 4f;
    private float lastAttack1Time, _attack1AnimationLength;
    private float lastAttack2Time, _attack2AnimationLength;
    private float lastAttack3Time, _attack3AnimationLength;
    private byte lastAttack = 1;
    
    private void Awake()
    {
        gameObjectToInstantiate = GameObject.FindWithTag("Boss1Enemy");
        if (gameObjectToInstantiate == null)
        {
            Debug.LogError("UEP: boss is null");
        }
        
        // Initialize animator here to avoid null reference
        _animator = GetComponentInChildren<Animator>();
        if (_animator == null)
        {
            Debug.LogError("BossEnemy: Animator component not found!");
        }
        
        GetComponent<Enemy>().enabled = false;
    }
    
    protected override void Start()
    {
        base.Start();
        
        CheckIfSoloBoss();
        
        // Initialize spawn timers
        lastSlimeSpawnTime = Time.time;
        lastSkeletonSpawnTime = Time.time;
        
        // Initialize attack timers
        lastAttack1Time = -attack1Cooldown;
        lastAttack2Time = -attack2Cooldown;
        lastAttack3Time = -attack3Cooldown;
        
        AnimationClip[] clips = _animator.runtimeAnimatorController.animationClips;
        _attack1AnimationLength = 1f; // Default fallback
        _attack2AnimationLength = 1f; // Default fallback
        _attack3AnimationLength = 1f; // Default fallback

        foreach (AnimationClip clip in clips)
        {
            if (clip.name.Contains("Atacar1"))
            {
                _attack1AnimationLength = clip.length;
            } else if (clip.name.Contains("Atacar2"))
            {
                _attack2AnimationLength = clip.length;
            } else if (clip.name.Contains("Atacar3"))
            {
                _attack3AnimationLength = clip.length;
            }
        }

        if (player == null) {
            player = GameObject.FindWithTag("Player").transform;
        }
    }
    
    private void CheckIfSoloBoss()
    {
        // Find the EnemySpawner to check if this is the last round with only one boss
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            // Check if we're in the last round and it's boss-only
            bool isLastRoundBossOnly = spawner.IsLastRoundBossOnly(); // We'll need to make this method public
            if (isLastRoundBossOnly)
            {
                isSoloBoss = true;
                // Hide the regular enemy health bar
                if (_healthBar != null)
                {
                    _healthBar.canvas.gameObject.SetActive(false);
                }
                
                // Create boss UI
                PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
                if (playerHealth != null && playerHealth.uiCanvas != null)
                {
                    bossUI = new BossHealthUI(playerHealth.uiCanvas, this);
                }
            }
        }
    }
    
    protected override void ExecuteCurrentState()
    {
        // Update phase based on health
        UpdatePhase();
        
        // Handle minion spawning
        HandleMinionSpawning();
        
        // Execute base behavior
        base.ExecuteCurrentState();
    }
    
    private void UpdatePhase()
    {
        float healthPercent = (health / maxHealth) * 100f;
        
        if (healthPercent <= 40f && !hasEnteredPhase3)
        {
            currentPhase = 3;
            hasEnteredPhase3 = true;
            slimeSpawnInterval = 5f; // Spawn slimes every 5s in phase 3
            Debug.Log("Boss entered Phase 3!");
            
            // Play rise animation when entering phase 3
            if (_animator != null)
            {
                _animator.SetTrigger("Levantarse");
            }
        }
        else if (healthPercent <= 80f && !hasEnteredPhase2)
        {
            currentPhase = 2;
            hasEnteredPhase2 = true;
            Debug.Log("Boss entered Phase 2!");
            
            // Play rise animation when entering phase 2
            if (_animator != null)
            {
                _animator.SetTrigger("Levantarse");
            }
        }
    }
    
    private void HandleMinionSpawning()
    {
        if (currentPhase >= 2) // Phase 2 and 3
        {
            // Spawn slimes
            if (Time.time >= lastSlimeSpawnTime + slimeSpawnInterval)
            {
                SpawnMinion(SlimeEnemy.gameObjectToInstantiate);
                lastSlimeSpawnTime = Time.time;
            }
        }
        
        if (currentPhase >= 3) // Phase 3 only
        {
            // Spawn skeletons every 10s
            if (Time.time >= lastSkeletonSpawnTime + skeletonSpawnInterval)
            {
                // Assuming you have a SkeletonEnemy class similar to SlimeEnemy
                // If not, you can use SlimeEnemy.gameObjectToInstantiate as placeholder
                SpawnMinion(MageEnemy.gameObjectToInstantiate); // Replace with skeleton prefab when available
                lastSkeletonSpawnTime = Time.time;
            }
        }
    }
    
    private void SpawnMinion(GameObject minionPrefab) 
    {
        if (minionPrefab == null) {
            return;
        }

        // Generate a random angle, but exclude the front arc (boss's forward direction)
        float bossForwardAngle = transform.eulerAngles.y;
        float excludeAngleRange = 60f; // Exclude 60 degrees in front (±30 degrees from forward)
        
        // Generate random angle excluding the front arc
        float randomAngle;
        do 
        {
            randomAngle = Random.Range(0f, 360f);
        } 
        while (Mathf.Abs(Mathf.DeltaAngle(randomAngle, bossForwardAngle)) < excludeAngleRange / 2f);
        
        // Generate random distance between 2 and 3 meters
        float spawnDistance = Random.Range(2f, 3f);
        
        // Calculate spawn position
        Vector3 spawnDirection = new Vector3(
            Mathf.Sin(randomAngle * Mathf.Deg2Rad), 
            0, 
            Mathf.Cos(randomAngle * Mathf.Deg2Rad)
        );
        Vector3 spawnPosition = transform.position + spawnDirection * spawnDistance;
        spawnPosition.y = transform.position.y; // Keep same Y level as boss
        
        GameObject minion = Instantiate(minionPrefab, spawnPosition, Quaternion.identity);
        
        // Configure Rigidbody (similar to EnemySpawner.SpawnEnemy)
        Rigidbody rb = minion.GetComponent<Rigidbody>();
        if (rb == null) 
        {
            rb = minion.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;
        rb.freezeRotation = false; // Allow rotation initially
        rb.useGravity = true;
        rb.linearDamping = 1f;
        
        // Set up the enemy component
        Enemy minionEnemy = minion.GetComponent<Enemy>();
        if (minionEnemy != null) 
        {
            minionEnemy.enabled = true;
            
            // Make sure the enemy faces the spawn point (player direction)
            GameObject spawnPoint = GameObject.FindWithTag("Player");
            if (spawnPoint != null) 
            {
                Vector3 directionToPlayer = (spawnPoint.transform.position - spawnPosition).normalized;
                minion.transform.rotation = Quaternion.LookRotation(directionToPlayer);
            }
        }
        
        Debug.Log($"Boss spawned minion: {minion.name} at distance: {spawnDistance:F1}m");
    }
    
    protected override void MoveTowardsTarget(Vector3 targetPosition)
    {
        base.MoveTowardsTarget(targetPosition);
        
        // Null check for animator before using it
        Debug.Log("Jefe mueve hacia target. estacaminando: "+ _estaCaminandoAnimacion);
        if (_animator != null && !_estaCaminandoAnimacion)
        {
            Debug.Log("Jefe triggerea caminar");
            _animator.SetTrigger("Caminar");
        }
    }
    
    protected override void AttackPlayer()
    {
        if (!CanPerformAnyAttack() || _estaAtacandoAnimacion)
        {
            return;
        }
        
        // Determine which attacks are available based on phase
        switch (currentPhase)
        {
            case 1:
                // Phase 1: Only Attack 1
                if (Time.time >= lastAttack1Time + attack1Cooldown)
                {
                    PerformAttack(1);
                    lastAttack1Time = Time.time;
                }
                break;
                
            case 2:
                // Phase 2: Attack 1 and 2, with slight preference for 2
                    // 60% chance for attack 2, 40% chance for attack 1
                    if (Random.value < 0.6f && Time.time >= lastAttack2Time + attack2Cooldown)
                    {
                        PerformAttack(2);
                        lastAttack2Time = Time.time;
                    }
                    else if (Time.time >= lastAttack1Time + attack1Cooldown)
                    {
                        PerformAttack(1);
                        lastAttack1Time = Time.time;
                    }
                break;
                
            case 3:
                // Phase 3: All attacks, with preference for 2 and more so for 3
                    float attackRoll = Random.value;
                    
                    // 50% chance for attack 3, 30% for attack 2, 20% for attack 1
                    if (attackRoll < 0.5f && Time.time >= lastAttack3Time + attack3Cooldown)
                    {
                        PerformAttack(3);
                        lastAttack3Time = Time.time;
                    }
                    else if (attackRoll < 0.8f && Time.time >= lastAttack2Time + attack2Cooldown)
                    {
                        PerformAttack(2);
                        lastAttack2Time = Time.time;
                    }
                    else if (Time.time >= lastAttack1Time + attack1Cooldown)
                    {
                        PerformAttack(1);
                        lastAttack1Time = Time.time;
                    }
                break;
        }
    }
    
    private bool CanPerformAnyAttack()
    {
        return lastAttack == 1 && Time.time >= lastAttack1Time + attack1Cooldown ||
            lastAttack == 2 && Time.time >= lastAttack2Time + attack2Cooldown ||
            lastAttack == 3 && Time.time >= lastAttack3Time + attack3Cooldown;
    }

    private void PerformAttack(byte ataque)
    {
        Debug.Log(gameObject.name + " performs Attack "+ataque+"!");
        if (_animator != null)
        {
            _animator.SetTrigger("Atacar" + ataque);
        }

        float attackAnimationLength;
        if (ataque == 1) {
            attackAnimationLength = _attack1AnimationLength;
        } else if (ataque == 2) {
            attackAnimationLength = _attack2AnimationLength;
        } else if (ataque == 3) {
            attackAnimationLength = _attack3AnimationLength;
        }
        else {
            throw new Exception("Ataque " + ataque + " no implementado");
        }
        
        float multiplicador = 1f;
        if (ataque == 2) {
            multiplicador = 1.2f;
        }
        else if (ataque == 3) {
            multiplicador = 1.5f;
        }
        
        StartCoroutine(CheckAttackAfterAnimation(attackAnimationLength, attackDamage * multiplicador));

        lastAttack = ataque;
        AudioManager.Instance.PlaySFX("BossAtaque"+ataque);
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        AudioManager.Instance.PlaySFX("HitBoss");
    
        // Update boss UI if it exists
        if (isSoloBoss && bossUI != null)
        {
            bossUI.UpdateBossHealthBar(health / maxHealth);
        
            // Hide boss UI when dead
            if (health <= 0)
            {
                bossUI.HideBossHealthBar();
            }
        }
    }
}