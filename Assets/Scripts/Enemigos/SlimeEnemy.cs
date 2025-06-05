using System;
using UnityEngine;

public class SlimeEnemy : Enemy 
{
    [Header("Jump Settings")]
    public float jumpForce = 300f;
    public float jumpCooldown = 3f;
    private float _lastJumpTime;
    private bool _isJumping = false;
    
    protected override float AttackDistance => 2f;
    protected override float FleeDistance => 0f;
    
    public static GameObject gameObjectToInstantiate;


    private void Awake() {
        gameObjectToInstantiate = GameObject.FindWithTag("SlimeEnemy");
        if (gameObjectToInstantiate == null) {
            Debug.LogError("UEP: slime is null");
        }

        GetComponent<Enemy>().enabled = false;
    }

    // Override ExecuteCurrentState to implement jumping behavior
    protected override void ExecuteCurrentState()
    {
        FacePlayer();
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        switch (currentState)
        {
            case EnemyState.Approach:
                if (distanceToPlayer > 2f && !_isJumping && Time.time > _lastJumpTime + jumpCooldown)
                {
                    Jump();
                    MoveTowardsTarget(player.position);
                    AudioManager.Instance.PlaySFX("SlimeImpact");
                }
                break;
                
            case EnemyState.Attack:
                StopMoving();
                TryAttack();
                AudioManager.Instance.PlaySFX("SlimeImpact");
                break;
                
            default:
                base.ExecuteCurrentState();
                break;
        }
        
        // Always limit speed after movement
        LimitSpeed();
    }
    
    private void Jump()
    {
        Debug.Log(gameObject.name + " is jumping!");
        
        _animator.SetTrigger("Saltar");
        
        // Calculate jump direction towards player
        Vector3 jumpDirection = (player.position - transform.position).normalized;
        
        // Apply jump force
        rb.AddForce(jumpDirection * jumpForce * 0.5f + Vector3.up * jumpForce, ForceMode.Impulse);
        
        _isJumping = true;
        _lastJumpTime = Time.time;
        
        // Schedule to reset jumping state
        Invoke(nameof(ResetJumpState), 1.0f);
    }
    
    private void ResetJumpState()
    {
        _isJumping = false;
    }
    
    // Override to check if grounded before jumping again
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") && _isJumping)
        {
            // Small cooldown to prevent immediate re-jump
            Invoke(nameof(ResetJumpState), 0.2f);
        }
    }
    
}