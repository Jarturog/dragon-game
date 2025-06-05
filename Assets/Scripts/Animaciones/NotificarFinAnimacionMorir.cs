using UnityEngine;

public class NotificarFinAnimacionMorir : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
    {
        Debug.Log("Death animation started");
        // Get the animation length and schedule destruction
        float animationLength = stateInfo.length;
        Enemy enemy = animator.gameObject.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            Debug.Log("Count down to death started");
            enemy.StartCoroutine(enemy.DestroyAfterDelay(animationLength));
        }
    }
}
