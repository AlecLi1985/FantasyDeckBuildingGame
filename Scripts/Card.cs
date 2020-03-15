using UnityEngine;
using System.Collections.Generic;

public enum CARDTYPE
{
    NONE,
    ATTACK,
    ABILITY,
    EFFECT,
}

public abstract class Card : ScriptableObject
{
    [Header("Base Card Attributes")]
    public string cardName;
    public string description;
    public string flavorText;
    public CARDTYPE cardType;
    public int cost;
    [Tooltip("Is this card applied only to its owner?  Abiliites set this to true.")]
    public bool useSelfOnly;
    [Tooltip("Is the card applied to a group or just one thing?")]
    public bool useOnGroup;
    public int animationID;

    public abstract void Apply(CharacterObject targetObject, CharacterObject selfObject);

}



