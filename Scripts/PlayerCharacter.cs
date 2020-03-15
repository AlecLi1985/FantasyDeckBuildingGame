using UnityEngine;

[CreateAssetMenu(menuName = "Character/Player Character")]
public class PlayerCharacter : Character
{
    [Header("Player Character Attributes")]
    public Card[] startingCards;
    public int[] startingCardsAmount;
}
