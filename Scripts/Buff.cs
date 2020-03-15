using UnityEngine;
using System.Collections;

public enum BuffStat
{
    NONE,
    HEALTH,
    BLOCK,
    DAMAGE,
    CARDSDRAWN,
    CARDPOINTS
}

[CreateAssetMenu(menuName = "Buff")]
public class Buff : ScriptableObject
{
    public string buffName;
    [TextArea]
    public string description;
    public BuffStat buffStat;
    [Tooltip("True = affects input (Weakened), False = affects output (Crippled)")]
    public bool external;
    [Tooltip("Set this buff once only when inflicted")]
    public bool setOnce;
    [Tooltip("Delay reducing lifetime until after next turn, only applies for enemies")]
    public bool delayLifetimeReductionUntilNextTurn;

    public int buffAmount;
    [Range(0f,1f)]
    public float buffPercentage;
    [HideInInspector]
    public int lifetime;

    public void CalculateBuff()
    {
        
    }
}
