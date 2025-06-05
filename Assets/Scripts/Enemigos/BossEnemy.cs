using UnityEngine;

public class BossEnemy: Enemy
{
    protected override float AttackDistance => 2f;
    protected override float FleeDistance => 0f;
    
    public static GameObject gameObjectToInstantiate;

    private void Awake() {
        gameObjectToInstantiate = GameObject.FindWithTag("Boss1Enemy");
        if (gameObjectToInstantiate == null) {
            Debug.LogError("UEP: boss1 is null");
        }

        GetComponent<Enemy>().enabled = false;
    }

    // Override ExecuteCurrentState to implement jumping behavior
    protected override void ExecuteCurrentState()
    {
        FacePlayer();
        
        switch (currentState)
        {
            case EnemyState.Approach:
                MoveTowardsTarget(player.position);
                break;
                
            case EnemyState.Attack:
                StopMoving();
                TryAttack();
                break;
                
            default:
                base.ExecuteCurrentState();
                break;
        }
        
        // Always limit speed after movement
        LimitSpeed();
    }

}
