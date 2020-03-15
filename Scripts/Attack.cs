using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Cards/Attack Card")]
public class Attack : Card
{
    [Header("Attack Card Attributes")]
    public int damage;
    public int damagePerHit;
    public Buff buffToInflict;

    public override void Apply(CharacterObject targetObject, CharacterObject selfObject)
    {
        int newDamage = damage;
        int newDamagePerHit = damagePerHit;

        //apply buffs/debuffs to self
        for (int i = 0; i < selfObject.buffs.Count; i++)
        {
            BuffInfo buffInfo = selfObject.buffs[i];
            
            if(buffInfo.lifetime > 0)
            {
                foreach (Buff buff in selfObject.combatManager.buffLibrary)
                {
                    if (buffInfo.name == buff.name)
                    {
                        if (buff.external == false)
                        {
                            ApplyBuff(buff, ref newDamage, ref newDamagePerHit, selfObject);
                        }
                    }
                }
            }
        }

        //apply buffs/debuffs to attack/target
        if (buffToInflict != null)
        {
            bool doesBuffExist = false;
            foreach (BuffInfo buffInfo in targetObject.buffs)
            {
                if (buffInfo.lifetime > 0)
                {
                    if (buffInfo.name == buffToInflict.name)
                    {
                        doesBuffExist = true;
                        buffInfo.delayLifetimeReduction = buffToInflict.delayLifetimeReductionUntilNextTurn;
                        buffInfo.lifetime++;

                        foreach (Buff buff in targetObject.combatManager.buffLibrary)
                        {
                            if (buffInfo.name == buff.name)
                            {
                                if (buff.external)
                                {
                                    ApplyBuff(buff, ref newDamage, ref newDamagePerHit, targetObject);
                                }
                            }
                        }

                    }
                }
            }

            if (doesBuffExist == false)
            {
                BuffInfo newBuff = new BuffInfo();
                newBuff.name = buffToInflict.name;
                newBuff.delayLifetimeReduction = buffToInflict.delayLifetimeReductionUntilNextTurn;
                newBuff.lifetime++;

                targetObject.buffs.Add(newBuff);
            }
        }
        else
        {
            foreach (BuffInfo buffInfo in targetObject.buffs)
            {
                if (buffInfo.lifetime > 0)
                {
                    foreach (Buff buff in targetObject.combatManager.buffLibrary)
                    {
                        if (buffInfo.name == buff.name)
                        {
                            if (buff.external)
                            {
                                ApplyBuff(buff, ref newDamage, ref newDamagePerHit, targetObject);
                            }
                        }
                    }
                }
            }
        }

        selfObject.currentDamage = newDamage;
        selfObject.currentDamagePerHit = newDamagePerHit;

        if (targetObject.currentBlock >= newDamage)
        {
            targetObject.currentBlock -= newDamage;
        }
        else
        {
            int remainder = newDamage - targetObject.currentBlock;
            targetObject.currentBlock = 0;
            targetObject.currentHealth -= remainder;
        }
    }

    void ApplyBuff(Buff appliedBuff, ref int newDamage, ref int newDamagePerHit, CharacterObject targetObject)
    {
        int oldDamage = newDamage;
        int oldDamagePerHit = newDamagePerHit;

        if (appliedBuff.external)
        {
            newDamage = Mathf.CeilToInt(oldDamage * appliedBuff.buffPercentage);
            newDamagePerHit = Mathf.CeilToInt(oldDamagePerHit * appliedBuff.buffPercentage);

            newDamage = oldDamage + newDamage;
            newDamagePerHit = oldDamagePerHit + newDamagePerHit;
        }
        else
        {
            newDamage = Mathf.FloorToInt(oldDamage * appliedBuff.buffPercentage);
            newDamagePerHit = Mathf.FloorToInt(oldDamagePerHit * appliedBuff.buffPercentage);
        }

        switch (appliedBuff.buffStat)
        {
            case BuffStat.CARDPOINTS:
                {
                    targetObject.combatManager.currentMaxCardPoints--;
                }
                break;
            case BuffStat.CARDSDRAWN:
                {

                }
                break;
        }
    }


}
