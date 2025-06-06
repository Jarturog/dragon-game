using UnityEngine;

public class NotificarAnimacionesEnemigo : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
    {
        Enemy enemy = animator.gameObject.GetComponentInParent<Enemy>();
        if (stateInfo.IsName("Armature|Caminar")) {
            enemy.setEstaCaminandoAnimacion(true);
        } else if (stateInfo.IsName("Armature|Atacar") || stateInfo.IsName("Armature|Atacar1") ||stateInfo.IsName("Armature|Atacar2") || stateInfo.IsName("Armature|Atacar3")) {
            enemy.setEstaAtacandoAnimacion(true);
        } else if (stateInfo.IsName("Armature|Morir")) {
            Debug.Log("Death animation started");
            float animationLength = stateInfo.length;
            enemy.StartCoroutine(enemy.DestroyAfterDelay(animationLength));
        }
        
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Enemy enemy = animator.gameObject.GetComponentInParent<Enemy>();
        if (stateInfo.IsName("Armature|Caminar")) {
            enemy.setEstaCaminandoAnimacion(false);
        } else if (stateInfo.IsName("Armature|Atacar") || stateInfo.IsName("Armature|Atacar1") ||stateInfo.IsName("Armature|Atacar2") || stateInfo.IsName("Armature|Atacar3")) {
            enemy.setEstaAtacandoAnimacion(false);
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
    
}
