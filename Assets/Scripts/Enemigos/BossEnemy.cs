using System.Collections;
using UnityEngine;

public class BossEnemy : Enemy
{
    public static GameObject gameObjectToInstantiate;
    
    protected override float AttackDistance => 8f;
    protected override float FleeDistance => 0f; // Boss never flees
    
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
    private float lastAttack1Time;
    private float lastAttack2Time;
    private float lastAttack3Time;
    
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
        
        // Initialize spawn timers
        lastSlimeSpawnTime = Time.time;
        lastSkeletonSpawnTime = Time.time;
        
        // Initialize attack timers
        lastAttack1Time = -attack1Cooldown;
        lastAttack2Time = -attack2Cooldown;
        lastAttack3Time = -attack3Cooldown;
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
                SpawnMinion(SlimeEnemy.gameObjectToInstantiate); // Replace with skeleton prefab when available
                lastSkeletonSpawnTime = Time.time;
            }
        }
    }
    
    private void SpawnMinion(GameObject minionPrefab)
    {
        if (minionPrefab != null)
        {
            // Spawn minion near the boss
            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * 3f;
            spawnPosition.y = transform.position.y; // Keep same Y level as boss
            
            GameObject minion = Instantiate(minionPrefab, spawnPosition, Quaternion.identity);
            Enemy minionEnemy = minion.GetComponent<Enemy>();
            if (minionEnemy != null)
            {
                minionEnemy.enabled = true;
            }
            
            Debug.Log($"Boss spawned minion: {minion.name}");
        }
    }
    
    protected override void MoveTowardsTarget(Vector3 targetPosition)
    {
        // Null check for animator before using it
        if (_animator != null && !_estaCaminandoAnimacion)
        {
            _animator.SetTrigger("Caminar");
        }
        
        base.MoveTowardsTarget(targetPosition);
    }
    
    protected override void AttackPlayer()
    {
        // Determine which attacks are available based on phase
        switch (currentPhase)
        {
            case 1:
                // Phase 1: Only Attack 1
                if (Time.time >= lastAttack1Time + attack1Cooldown)
                {
                    PerformAttack1();
                    lastAttack1Time = Time.time;
                }
                break;
                
            case 2:
                // Phase 2: Attack 1 and 2, with slight preference for 2
                if (CanPerformAnyAttack())
                {
                    // 60% chance for attack 2, 40% chance for attack 1
                    if (Random.value < 0.6f && Time.time >= lastAttack2Time + attack2Cooldown)
                    {
                        PerformAttack2();
                        lastAttack2Time = Time.time;
                    }
                    else if (Time.time >= lastAttack1Time + attack1Cooldown)
                    {
                        PerformAttack1();
                        lastAttack1Time = Time.time;
                    }
                }
                break;
                
            case 3:
                // Phase 3: All attacks, with preference for 2 and more so for 3
                if (CanPerformAnyAttack())
                {
                    float attackRoll = Random.value;
                    
                    // 50% chance for attack 3, 30% for attack 2, 20% for attack 1
                    if (attackRoll < 0.5f && Time.time >= lastAttack3Time + attack3Cooldown)
                    {
                        PerformAttack3();
                        lastAttack3Time = Time.time;
                    }
                    else if (attackRoll < 0.8f && Time.time >= lastAttack2Time + attack2Cooldown)
                    {
                        PerformAttack2();
                        lastAttack2Time = Time.time;
                    }
                    else if (Time.time >= lastAttack1Time + attack1Cooldown)
                    {
                        PerformAttack1();
                        lastAttack1Time = Time.time;
                    }
                }
                break;
        }
    }
    
    private bool CanPerformAnyAttack()
    {
        return Time.time >= lastAttack1Time + attack1Cooldown ||
               Time.time >= lastAttack2Time + attack2Cooldown ||
               Time.time >= lastAttack3Time + attack3Cooldown;
    }
    
    private void PerformAttack1()
    {
        Debug.Log(gameObject.name + " performs Attack 1!");
        if (_animator != null)
        {
            _animator.SetTrigger("Atacar1");
        }
        
        // Apply damage to player
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
        
        AudioManager.Instance.PlaySFX("SlimeImpact");
    }
    
    private void PerformAttack2()
    {
        Debug.Log(gameObject.name + " performs Attack 2!");
        if (_animator != null)
        {
            _animator.SetTrigger("Atacar2");
        }
        
        // Apply slightly more damage for attack 2
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage * 1.2f);
        }
        
        AudioManager.Instance.PlaySFX("SlimeImpact");
    }
    
    private void PerformAttack3()
    {
        Debug.Log(gameObject.name + " performs Attack 3!");
        if (_animator != null)
        {
            _animator.SetTrigger("Atacar3");
        }
        
        // Apply more damage for attack 3 (strongest attack)
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage * 1.5f);
        }
        
        AudioManager.Instance.PlaySFX("SlimeImpact");
    }
    
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        
        // Additional boss-specific damage handling could go here
        // For example, special effects or screen shake
    }
}