using UnityEngine;

[CreateAssetMenu(menuName = "Character/Enemy Character")]
public class EnemyCharacter : Character
{
    [Header("Enemy Character Attributes")]
    public Card[] attackPattern;
}
