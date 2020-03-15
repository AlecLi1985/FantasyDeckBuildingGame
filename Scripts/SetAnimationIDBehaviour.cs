using System;
using UnityEngine;

public class SetAnimationIDBehaviour : StateMachineBehaviour
{
    public int animationID;
    public bool onEnter = true;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(onEnter)
        {
            animator.SetInteger("animationID", animationID);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (onEnter == false)
        {
            animator.SetInteger("animationID", animationID);
        }
    }


}
