using System;
using UnityEngine;

public class DeathCompleteBehaviour : StateMachineBehaviour
{
    public bool fireOnEnter = false;
    public bool fireOnExit = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (fireOnEnter)
        {
            CharacterObject characterObject = animator.GetComponent<CharacterObject>();
            if(characterObject != null)
            {
                characterObject.OnDeathComplete(animator.transform);
            }
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(fireOnExit)
        {
            CharacterObject characterObject = animator.GetComponent<CharacterObject>();
            if (characterObject != null)
            {
                characterObject.OnDeathComplete(animator.transform);
            }
        }
    }


}
