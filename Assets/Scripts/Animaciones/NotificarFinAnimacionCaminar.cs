using UnityEngine;

public class NotificarFinAnimacionCaminar : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        if (stateInfo.IsName("Armature|Caminar") || stateInfo.IsName("Bufanda|Caminar")) {
            player.setEstaCaminandoAnimacion(true);
        } else if (stateInfo.IsName("Armature|Correr") || stateInfo.IsName("Bufanda|Correr")) {
            player.setEstaCorriendoAnimacion(true);
        } else if (stateInfo.IsName("Armature|Idle") || stateInfo.IsName("Bufanda|Idle")) {
            player.setEstaIdleAnimacion(true);
        } else if (stateInfo.IsName("Armature|Saltar") || stateInfo.IsName("Bufanda|Saltar")) {
            player.setEstaSaltandoAnimacion(false);
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
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        if (stateInfo.IsName("Armature|Caminar") || stateInfo.IsName("Bufanda|Caminar")) {
            player.setEstaCaminandoAnimacion(false);
        } else if (stateInfo.IsName("Armature|Correr") || stateInfo.IsName("Bufanda|Correr")) {
            player.setEstaCorriendoAnimacion(false);
        } else if (stateInfo.IsName("Armature|Idle") || stateInfo.IsName("Bufanda|Idle")) {
            player.setEstaIdleAnimacion(false);
        } else if (stateInfo.IsName("Armature|Saltar") || stateInfo.IsName("Bufanda|Saltar")) {
            player.setEstaSaltandoAnimacion(false);
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
