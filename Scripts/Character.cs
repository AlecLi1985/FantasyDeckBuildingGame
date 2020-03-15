using UnityEngine;

public enum CharacterType
{
    NONE,
    PLAYER,
    ENEMY,
    ELITE,
    MINIBOSS,
    BOSS
}

public abstract class Character : ScriptableObject
{
    [Header("Base Character Attributes")]
    public CharacterType characterType;
    public string characterName;
    public string characterFlavorText;

    public int characterHealth;
    public int characterBlock;
    public int characterStrength;
    public int characterDexterity;
}
