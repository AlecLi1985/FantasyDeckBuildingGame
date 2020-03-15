using UnityEngine;
using TMPro;

public class CardUIObject : MonoBehaviour
{
    public Card cardDefinition;

    public TMP_Text cardTitleText;
    public TMP_Text cardDescriptionText;
    public TMP_Text cardCostText;

    // Start is called before the first frame update
    void Start()
    {
        if(cardDefinition != null)
        {
            if(cardTitleText != null)
            {
                cardTitleText.text = cardDefinition.cardName;
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

    public void SetCardDefinition(Card card)
    {
        cardDefinition = card;

        if (cardTitleText != null)
        {
            cardTitleText.text = cardDefinition.cardName;
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
