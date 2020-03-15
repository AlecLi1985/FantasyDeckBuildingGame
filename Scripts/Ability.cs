using UnityEngine;
using System;
using System.Collections.Generic;

public enum AbilityType
{
    NONE,
    ADDSTAT
}

public enum StatType
{
    NONE,
    HEALTH,
    BLOCK,
    DAMAGE,
    CARDSDRAWN
}

[Serializable]
public class AbilityStatInfo
{
    public AbilityType abilityType;
    public StatType statType;
    [Tooltip("Apply a specific number")]
    public int statAmount;
    [Tooltip("Apply a percentage of some other value"), Range(0f,1f)]
    public float statPercentage;
    [Tooltip("This stat is used with statPercentage to get the amount")]
    public StatType statPercentageValue;
}

[CreateAssetMenu(menuName = "Cards/Ability Card")]
public class Ability : Card
{
    [Header("Ability Card attributes")]
    public AbilityStatInfo[] abilityStatInfos;

    public override void Apply(CharacterObject targetObject, CharacterObject selfObject)
    {
        foreach(AbilityStatInfo abilityStatInfo in abilityStatInfos)
        {
            if(abilityStatInfo.abilityType == AbilityType.ADDSTAT)
            {
                ApplyAddStat(selfObject, abilityStatInfo);
            }
        }
    }

    void ApplyAddStat(CharacterObject selfObject, AbilityStatInfo abilityStatInfo)
    {
        switch(abilityStatInfo.statType)
        {
            case StatType.DAMAGE:
                {
                    if (abilityStatInfo.statAmount > 0)
                    {
                        
                    }
                    else
                    {
                        
                    }
                }
                break;
            case StatType.BLOCK:
                {
                    if(abilityStatInfo.statAmount > 0)
                    {
                        selfObject.currentBlock += abilityStatInfo.statAmount;
                    }
                    else
                    {
                        ApplyAddStatPercentage(selfObject, abilityStatInfo.statPercentageValue, abilityStatInfo.statPercentage);
                    }
                }
                break;
            case StatType.CARDSDRAWN:
                {

                }
                break;
        }
    }

    void ApplyAddStatPercentage(CharacterObject selfObject, StatType statPercentageValue, float statPercentage)
    {
        switch (statPercentageValue)
        {
            case StatType.HEALTH:
                {

                }
                break;
            case StatType.DAMAGE:
                {

                }
                break;
            case StatType.BLOCK:
                {
                    selfObject.currentBlock += Mathf.RoundToInt(selfObject.currentBlock * statPercentage);
                }
                break;
            case StatType.CARDSDRAWN:
                {

                }
                break;
        }
    }

}
