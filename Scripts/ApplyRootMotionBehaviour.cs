using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyRootMotionBehaviour : StateMachineBehaviour
{
    public bool setRootMotionOnEnter = true;
    public bool setRootMotionOnExit = true;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("usingRootMotion", setRootMotionOnEnter);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("usingRootMotion", setRootMotionOnExit);
    }


}
