using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Cards/Status Card")]
public class Status : Card
{
    [Header("Status variables")]
    public bool isPlayable = false;
    public bool removeAtEndOfTurn = true;
    public int damage = 0;

    public override void Apply(CharacterObject targetObject, CharacterObject selfObject)
    {
        targetObject.currentHealth -= damage;
    }


}
