using System;
using UnityEngine;

public class SetBoolParameterBehaviour : StateMachineBehaviour
{
    public string boolParameterName;
    public bool setOnEnter = false;
    public bool setOnEnterValue = false;
    public bool setOnExit = false;
    public bool setOnExitValue = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(setOnEnter)
        {
            animator.SetBool(boolParameterName, setOnEnterValue);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (setOnExit)
        {
            animator.SetBool(boolParameterName, setOnExitValue);
        }
    }


}
