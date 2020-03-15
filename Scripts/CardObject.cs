using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class CardObject : MonoBehaviour
{
    public static event Action<Transform, Card> OnApplyCardEvent;

    public Card cardDefinition;

    public TMP_Text cardNameText;
    public TMP_Text cardDescriptionText;
    public TMP_Text cardCostText;

    public Animator cardAnimator;
    public float animateOverExitCardTime = 1f;
    public Vector3 overCardOffset;
    public float animateResetCardPositionTime = 1f;

    public bool isOver = false;
    public bool isSelected = false;

    Vector3 startPosition;
    Vector3 localStartPosition;

    Quaternion startRotation;
    Vector3 mouseWorldPosition;

    Vector3 cardRotationVector = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        isSelected = isOver = false;

        cardAnimator = GetComponentInChildren<Animator>();
        mouseWorldPosition = Vector3.zero;

        if (cardDefinition != null)
        {
            if (cardNameText != null)
            {
                cardNameText.text = cardDefinition.cardName;
            }
            if (cardDescriptionText != null)
            {
                cardDescriptionText.text = cardDefinition.description;
            }
            if (cardCostText != null)
            {
                cardCostText.text = cardDefinition.cost.ToString();
            }

        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    //public void UseCard(Transform recipient)
    //{
    //    if(OnApplyCardEvent != null)
    //    {
    //        OnApplyCardEvent.Invoke(recipient, cardDefinition);
    //    }
    //}

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }

    public void SetCardDefinition(Card definition)
    {
        cardDefinition = definition;
    }

    public void SetStartPosition(Vector3 position)
    {
        startPosition = transform.parent.position + position;
        localStartPosition = transform.localPosition;
    }

    public void SetLocalStartPosition(Vector3 position)
    {
        transform.localPosition = position;
        localStartPosition = position;
    }

    public Vector3 GetStartPosition()
    {
        return startPosition;
    }

    public void SetStartRotation(Quaternion rotation)
    {
        startRotation = rotation;
    }

    private void OnMouseEnter()
    {
        if (isSelected == false)
        {
            MouseOverCard();
            isOver = true;
        }

    }

    private void OnMouseExit()
    {
        if (isSelected == false)
        {
            MouseExitCard();
            isOver = false;
        }
    }

    public void MouseOverCard()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateMouseOverCard());
    }

    IEnumerator AnimateMouseOverCard()
    {
        //localStartPosition = transform.localPosition;

        float currentTime;
        for (currentTime = 0f; currentTime < animateOverExitCardTime; currentTime += Time.deltaTime)
        {
            float normalizedTime = (1f / animateOverExitCardTime) * currentTime;
            transform.localPosition = Vector3.Lerp(transform.localPosition, localStartPosition + overCardOffset, normalizedTime);

            yield return null;
        }

        transform.localPosition = localStartPosition + overCardOffset;
    }

    public void MouseExitCard()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateMouseExitCard());
    }

    IEnumerator AnimateMouseExitCard()
    {
        float currentTime;
        for (currentTime = 0f; currentTime < animateOverExitCardTime; currentTime += Time.deltaTime)
        {
            float normalizedTime = (1f / animateOverExitCardTime) * currentTime;
            transform.localPosition = Vector3.Lerp(transform.localPosition, localStartPosition, normalizedTime);

            yield return null;
        }

        transform.localPosition = localStartPosition;
    }


    public void ResetCardPosition(Vector3 fromPosition)
    {
        //Debug.Log("reset card position");
        StopAllCoroutines();
        StartCoroutine(AnimateResetCardPosition(fromPosition, Random.value > 0.7f));
    }

    IEnumerator AnimateResetCardPosition(Vector3 fromPosition, bool doSpin)
    {

        float currentResetCardPositionTime;
        for (currentResetCardPositionTime = 0f; currentResetCardPositionTime < animateResetCardPositionTime; currentResetCardPositionTime += Time.deltaTime)
        {
            float normalizedTime = (1f / animateResetCardPositionTime) * currentResetCardPositionTime;
            transform.localPosition = Vector3.Lerp(transform.TransformVector(fromPosition), localStartPosition, normalizedTime);

            if (doSpin)
            {

            }

            yield return null;
        }

        //SetSelected(false);
        transform.position = startPosition;
        //isOver = false;
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(startPosition, 0.3f);
    }

}
