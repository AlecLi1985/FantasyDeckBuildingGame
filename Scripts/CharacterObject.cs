using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuffInfo
{
    public string name;
    public bool delayLifetimeReduction;
    public int lifetime;
}

[SelectionBase]
public class CharacterObject : MonoBehaviour
{
    public static event Action OnPlayerExecuteTurn;
    public static event Action OnPlayerAttackComplete;
    public static event Action<CharacterObject> OnPlayerUseAbility;

    public static event Action<CharacterObject, int> OnAnimationHit;
    public static event Action<CharacterObject> OnCharacterDestroy;

    public Character characterDefinition;

    public List<BuffInfo> buffs = new List<BuffInfo>();

    public Vector3 startPositionOffset = Vector3.zero;
    public Transform enemyAttackPosition;

    public Vector3 characterStatsPositionOffset = Vector3.zero;

    public bool mirrorAnimations = false;
    public float attackAnimationSpeedMultiplier = 1f;
    public bool setManualRootMotion;
    public bool attackImmediately;

    [HideInInspector]
    public bool isExecutingTurn = false;
    [HideInInspector]
    public bool turnComplete = false;

    [HideInInspector]
    public int enemyIndex = -1;

    public GameObject[] effectObjects;
    public Transform effectObjectParent;
    [HideInInspector]
    public Transform effectObjectTargetTransform;
    public float effectSpeed = 10f;

    Animator animator;
    Vector3 startPosition = Vector3.zero;
    Quaternion startRotation = Quaternion.identity;
    Vector3 smoothDampVelocity = Vector3.zero;
    float smoothDampResetTime = .15f;
    bool animateToStart = false;
    bool animateToAttack = false;

    List<Transform> attackTargetTransforms = new List<Transform>();
    Transform effectTargetTransform;

    Vector3 myAttackPosition;
    public float minAttackDistance = 1f;

    bool matchTarget = false;

    public int currentHealth;
    public int currentBlock;
    public int currentStrength;
    public int currentDexterity;

    [HideInInspector]
    public int currentDamage = 0;
    [HideInInspector]
    public int currentDamagePerHit = 0;

    public bool isDead;

    int currentAttackPatternStep = 0;

    Card appliedCard;

    [HideInInspector]
    public CombatManager combatManager;

    // Start is called before the first frame update
    void Start()
    {
        combatManager = FindObjectOfType<CombatManager>();

        animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetFloat("attackSpeedMultiplier", attackAnimationSpeedMultiplier);
            animator.SetBool("mirror", mirrorAnimations);
        }

        currentHealth = characterDefinition.characterHealth;
        currentBlock = characterDefinition.characterBlock;

        CardObject.OnApplyCardEvent -= ApplyCardEffects;
        CardObject.OnApplyCardEvent += ApplyCardEffects;

        EffectObject.OnEffectObjectHit -= OnEffectHit;
        EffectObject.OnEffectObjectHit += OnEffectHit;
    }

    private void Update()
    {
        if (isDead == false)
        {
            if (animator.GetBool("attackComplete") == false)
            {
                if ((myAttackPosition - transform.position).sqrMagnitude < minAttackDistance)
                {
                    animator.SetBool("attack", true);
                }

                //player character only
                if (matchTarget)
                {
                    //animator.MatchTarget(startPosition, startRotation, AvatarTarget.Root, new MatchTargetWeightMask(Vector3.one, 1f), startMatchTargetTime, endMatchTargetTime);
                    transform.position = Vector3.SmoothDamp(transform.position, startPosition, ref smoothDampVelocity, smoothDampResetTime, 10000f);
                }

            }
        }
        else
        {
            if (characterDefinition.characterType == CharacterType.PLAYER)
            {
                animator.SetBool("attack", true);
            }
        }

    }

    public void UpdateCharacterObject()
    {
        //Debug.Log("current health " + currentHealth);

        if (currentHealth <= 0)
        {
            StartCoroutine(SetIsDead());
        }
    }

    IEnumerator SetIsDead()
    {
        yield return new WaitForSeconds(.1f);
        isDead = true;
    }

    public void ResetTurn()
    {
        isExecutingTurn = turnComplete = false;
        ReduceBuffLifetime();
    }

    public void ExecuteTurn(Transform recipient, CardObject usedCard)
    {
        attackTargetTransforms.Add(recipient);

        CharacterObject attackTargetObject = attackTargetTransforms[0].GetComponent<CharacterObject>();
        myAttackPosition = attackTargetObject.enemyAttackPosition.position;
        effectTargetTransform = attackTargetObject.effectObjectTargetTransform;

        if (characterDefinition.characterType != CharacterType.PLAYER)
        {
            isExecutingTurn = true;
            turnComplete = false;

            appliedCard = (characterDefinition as EnemyCharacter).attackPattern[currentAttackPatternStep];
            ApplyCardEffects(recipient, (characterDefinition as EnemyCharacter).attackPattern[currentAttackPatternStep]);
            currentAttackPatternStep = (currentAttackPatternStep + 1) % (characterDefinition as EnemyCharacter).attackPattern.Length;
        }
        else
        {
            isExecutingTurn = true;

            appliedCard = usedCard.cardDefinition;
            ApplyCardEffects(recipient, usedCard.cardDefinition);

            if (OnPlayerExecuteTurn != null)
            {
                OnPlayerExecuteTurn.Invoke();
            }
        }
    }

    public void ExecuteTurn(List<CharacterObject> enemyCharacters, CardObject usedCard)
    {
        foreach(CharacterObject enemy in enemyCharacters)
        {
            Transform recipient = enemy.transform;
            attackTargetTransforms.Add(recipient);

            isExecutingTurn = true;

            appliedCard = usedCard.cardDefinition;
            ApplyCardEffects(recipient, usedCard.cardDefinition);
        }

        if (OnPlayerExecuteTurn != null)
        {
            OnPlayerExecuteTurn.Invoke();
        }
    }

    public void CompleteTurn(Transform identity)
    {
        if (identity == transform)
        {
            if (characterDefinition.characterType != CharacterType.PLAYER)
            {
                isExecutingTurn = false;
                turnComplete = true;

                //Debug.Log("enemy turn complete");
            }
            else
            {
                isExecutingTurn = false;
                //Debug.Log("player turn complete");

                if (OnPlayerAttackComplete != null)
                {
                    OnPlayerAttackComplete.Invoke();
                }
            }

            attackTargetTransforms.Clear();
        }

    }

    public void ApplyCardEffects(Transform recipient, Card card)
    {
        CharacterObject targetObject;
        if (recipient.TryGetComponent(out targetObject))
        {
            card.Apply(targetObject, this);
            targetObject.UpdateCharacterObject();
        }

        if(card.cardType == CARDTYPE.ATTACK)
        {
            animator.SetInteger("animationID", card.animationID);
            animator.SetBool("attack", attackImmediately);
        }
        else if(card.cardType == CARDTYPE.ABILITY)
        {
            animator.SetInteger("animationID", card.animationID);
            animator.SetBool("useAbility", true);

            if(OnPlayerUseAbility != null)
            {
                OnPlayerUseAbility.Invoke(this);
            }
        }

    }

    //public void UseCard(Transform recipient, CardObject usedCard)
    //{
    //    if (usedCard != null)
    //    {
    //        usedCard.UseCard(recipient);
    //    }

    //    animator.SetInteger("animationID", usedCard.cardDefinition.animationID);
    //}

    void SetIsExecutingTurn(bool executing)
    {
        isExecutingTurn = executing;
    }

    void SetTurnComplete(bool complete)
    {
        turnComplete = complete;
    }

    public void OnEffectHit(Transform target)
    {
        if (target == attackTargetTransforms[0])
        {
            OnAttackTargetAnimationHit();
        }
    }

    public void OnAttackTargetAnimationHit()
    {
        if (attackTargetTransforms.Count > 0)
        {
            foreach(Transform attackTargetTransform in attackTargetTransforms)
            {
                Animator attackTargetAnimator = attackTargetTransform.GetComponent<Animator>();
                CharacterObject attackTargetObject = attackTargetTransform.GetComponent<CharacterObject>();

                if (attackTargetObject.isDead)
                {
                    attackTargetAnimator.SetInteger("animationID", 66);
                }
                else
                {
                    attackTargetAnimator.SetInteger("animationID", -1);
                }

                if (OnAnimationHit != null)
                {
                    OnAnimationHit.Invoke(attackTargetObject, currentDamagePerHit);
                }
            }

            SoundManager.instance.PlaySound("HitImpact1");

        }
    }

    public void AnimateJumpBack(int jump)
    {
        if (jump == 1)
        {
            matchTarget = true;
        }
        else
        {
            matchTarget = false;
        }
    }

    public void PlayAnimationSoundEffect()
    {
        SoundManager.instance.PlaySound("TwoHandedSwordHit1");
    }

    public void FireEffect(int effectIndex)
    {
        GameObject effectObjectInstance = Instantiate(effectObjects[effectIndex]);

        EffectObject effect = effectObjectInstance.GetComponent<EffectObject>();
        if (effect.parentToTransform)
        {
            effectObjectInstance.transform.SetParent(effectObjectParent, false);
        }
        else
        {
            effectObjectInstance.transform.position = effectObjectParent.position;
        }

        if (effect.isProjectile)
        {
            effect.owner = transform;
            effect.SetTargetTransform(effectTargetTransform);
        }
        else
        {
            effect.owner = transform;
        }
    }

    public void SetStartPositionAndRotation(Vector3 startPos, Quaternion startRot)
    {
        startPosition = startPos + startPositionOffset;
        startRotation = startRot;

        transform.position = startPosition;
        transform.rotation = startRotation;

    }

    public void ReduceBuffLifetime()
    {
        foreach (BuffInfo buffInfo in buffs)
        {
            if(characterDefinition.characterType == CharacterType.PLAYER)
            {
                if(buffInfo.delayLifetimeReduction)
                {
                    buffInfo.delayLifetimeReduction = false;
                }
                else
                {
                    if (buffInfo.lifetime > 0)
                    {
                        buffInfo.lifetime--;
                    }
                }
            }
            else
            {
                if (buffInfo.lifetime > 0)
                {
                    buffInfo.lifetime--;
                }
            }
            
        }
    }

    private void OnAnimatorMove()
    {
        if (setManualRootMotion)
        {
            if (animator)
            {
                if (isDead == false)
                {
                    if (animator.GetInteger("animationID") != 0 &&
                        animator.GetInteger("animationID") != 66 &&
                        animator.GetBool("attack") == false &&
                        animateToAttack == false)
                    {
                        Vector3 newPosition = transform.position;
                        newPosition.x -= animator.GetFloat("runSpeed") * Time.deltaTime;
                        transform.position = newPosition;
                    }
                    else if (animateToAttack)
                    {
                        transform.position = Vector3.SmoothDamp(transform.position, myAttackPosition, ref smoothDampVelocity, smoothDampResetTime, 4000f);
                    }
                    else if (animateToStart)
                    {
                        transform.position = Vector3.SmoothDamp(transform.position, startPosition, ref smoothDampVelocity, smoothDampResetTime, 2000f);
                    }
                }

            }
        }
        else
        {
            if (attackImmediately == false && isDead == false)
            {
                transform.position += animator.deltaPosition;
            }
        }
    }

    public void AnimateToStartPosition()
    {
        animateToStart = !animateToStart;
    }

    public void AnimateToAttackPosition()
    {
        if (isDead == false)
        {
            animateToAttack = !animateToAttack;
        }
    }

    public void OnDeathComplete(Transform identity)
    {
        if (identity == transform)
        {
            if (isDead)
            {
                CardObject.OnApplyCardEvent -= ApplyCardEffects;
                EffectObject.OnEffectObjectHit -= OnEffectHit;

                if (OnCharacterDestroy != null)
                {
                    OnCharacterDestroy.Invoke(this);
                }

                Destroy(gameObject);
            }

        }
    }

    private void OnDestroy()
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(myAttackPosition, 0.5f);

    }
}
