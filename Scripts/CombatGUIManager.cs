using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class CombatGUIManager : MonoBehaviour
{
    CombatManager combatManager;
    public GameObject mouseClickBlocker;
    public GameObject cardUIObject;

    public GameObject characterStatsObject;
    public Transform characterStatsParent;

    public GameObject buffInfoTextPanel;
    public TMP_Text buffInfoText;

    public GameObject cardPointsPanel;
    public TMP_Text cardPointsText;

    public GameObject endTurnButton;

    public GameObject gameOverPanel;
    public CanvasGroup gameOverPanelCanvasGroup;
    public float animateGameOverPanelFadeTime;

    public GameObject victoryPanel;
    public CanvasGroup victoryPanelCanvasGroup;
    public float animateVictoryPanelFadeTime;

    public GameObject backToMapButton;
    public GameObject backToMainMenuButton;

    public GameObject currentCardDeckButton;
    public TMP_Text currentCardDeckButtonText;
    public GameObject currentCardDeckGridPanelObject;
    public GameObject currentCardDeckGridContent;
    List<CardUIObject> currentCardDeckCardUIObjects;
    bool currentCardDeckButtonPressed = false;

    public GameObject discardedCardDeckButton;
    public TMP_Text discardedCardDeckButtonText;
    public GameObject discardedCardDeckGridPanelObject;
    public GameObject discardedCardDeckGridContent;
    List<CardUIObject> discardedCardDeckCardUIObjects;
    bool discardedCardDeckButtonPressed = false;

    GameObject playerCharacterStatsObject;
    CharacterStats playerCharacterStats;
    Vector3 playerCharacterStatsObjectPosition = Vector3.zero;

    List<GameObject> enemyCharacterStatsObjects;

    LevelLoader levelLoader;

    bool firstFrame = false;

    // Start is called before the first frame update
    void Start()
    {
        combatManager = FindObjectOfType<CombatManager>();
        levelLoader = FindObjectOfType<LevelLoader>();

        //HideBackToMapButton();

        currentCardDeckCardUIObjects = new List<CardUIObject>();
        discardedCardDeckCardUIObjects = new List<CardUIObject>();

        if(combatManager != null)
        {
            foreach(Card card in combatManager.playerCurrentCardDeck)
            {
                if (currentCardDeckGridContent != null)
                {
                    GameObject cardUIObjectInstance = Instantiate(cardUIObject, currentCardDeckGridContent.transform);
                    CardUIObject cardUI = cardUIObjectInstance.GetComponent<CardUIObject>();
                    cardUI.SetCardDefinition(card);
                    currentCardDeckCardUIObjects.Add(cardUI);
                }

                if (discardedCardDeckGridContent != null)
                {
                    GameObject cardUIObjectInstance = Instantiate(cardUIObject, discardedCardDeckGridContent.transform);
                    cardUIObjectInstance.SetActive(false);
                    CardUIObject cardUI = cardUIObjectInstance.GetComponent<CardUIObject>();
                    cardUI.SetCardDefinition(card);
                    discardedCardDeckCardUIObjects.Add(cardUI);
                }
            }

            int numCharacterStatsObjects = combatManager.enemyCharacters.Count;
            enemyCharacterStatsObjects = new List<GameObject>(numCharacterStatsObjects);

            for(int i = 0; i < numCharacterStatsObjects; i++)
            {
                enemyCharacterStatsObjects.Add(Instantiate(characterStatsObject, characterStatsParent));
                //SetCharacterStatsObject(enemyCharacterStatsObjects[i], combatManager.enemyCharacters[i]);
            }

            playerCharacterStatsObject = Instantiate(characterStatsObject, characterStatsParent);
            playerCharacterStatsObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
            playerCharacterStats = playerCharacterStatsObject.GetComponent<CharacterStats>();
            //SetCharacterStatsObject(playerCharacterStatsObject, combatManager.playerCharacterObject);
        }

        CombatManager.OnCardsDealt -= UpdateCurrentCardDeckList;
        CombatManager.OnCardsDealt += UpdateCurrentCardDeckList;
        CombatManager.OnCardDiscarded -= UpdateDiscardedCardDeckListAdded;
        CombatManager.OnCardDiscarded += UpdateDiscardedCardDeckListAdded;
        CombatManager.OnDiscardEmptied -= UpdateDiscardedCardDeckListEmptied;
        CombatManager.OnDiscardEmptied += UpdateDiscardedCardDeckListEmptied;

        CombatManager.OnUpdateCardPoints -= UpdateCardPointsText;
        CombatManager.OnUpdateCardPoints += UpdateCardPointsText;
        CombatManager.OnHasWon -= AnimateVictoryPanel;
        CombatManager.OnHasWon += AnimateVictoryPanel;
        CombatManager.OnPlayerDestroyed -= AnimateGameOverPanel;
        CombatManager.OnPlayerDestroyed += AnimateGameOverPanel;

        CombatManager.OnTurnComplete -= UpdateBuffStatsObjects;
        CombatManager.OnTurnComplete += UpdateBuffStatsObjects;

        CombatManager.OnEnemyTurn -= HideEndTurnButton;
        CombatManager.OnEnemyTurn += HideEndTurnButton;
        CombatManager.OnEnemyTurnComplete -= ShowEndTurnButton;
        CombatManager.OnEnemyTurnComplete += ShowEndTurnButton;

        CharacterObject.OnPlayerExecuteTurn -= HideEndTurnButton;
        CharacterObject.OnPlayerExecuteTurn += HideEndTurnButton;
        CharacterObject.OnPlayerUseAbility -= UpdateBlockStat;
        CharacterObject.OnPlayerUseAbility += UpdateBlockStat;

        CharacterObject.OnPlayerAttackComplete -= ShowEndTurnButton;
        CharacterObject.OnPlayerAttackComplete += ShowEndTurnButton;

        CharacterObject.OnAnimationHit -= UpdateStats;
        CharacterObject.OnAnimationHit += UpdateStats;

        CharacterObject.OnCharacterDestroy -= RemovePlayerStatsObject;
        CharacterObject.OnCharacterDestroy += RemovePlayerStatsObject;

        //remove stats ui for enemies from combat manager
        CombatManager.OnEnemyRemoved -= RemoveCharacterStatsObject;
        CombatManager.OnEnemyRemoved += RemoveCharacterStatsObject;

        BuffText.OnEnterBuffText -= UpdateBuffInfoTextPanel;
        BuffText.OnEnterBuffText += UpdateBuffInfoTextPanel;
        BuffText.OnExitBuffText -= HideBuffInfoPanel;
        BuffText.OnExitBuffText += HideBuffInfoPanel;

    }

    void SetCharacterStatsObjects()
    {
        for (int i = 0; i < enemyCharacterStatsObjects.Count; i++)
        {
            SetCharacterStatsObject(enemyCharacterStatsObjects[i], combatManager.enemyCharacters[i]);
        }

        SetCharacterStatsObject(playerCharacterStatsObject, combatManager.playerCharacterObject);
    }

    private void Update()
    {
        if(firstFrame == false)
        {
            firstFrame = true;
            SetCharacterStatsObjects();
        }

        if(playerCharacterStatsObject != null)
        {
            playerCharacterStatsObjectPosition = Camera.main.WorldToScreenPoint(playerCharacterStats.characterObjectTransform.position + playerCharacterStats.characterStatsPositionOffset);
            playerCharacterStatsObject.GetComponent<RectTransform>().transform.position = playerCharacterStatsObjectPosition;
        }

        for (int i = 0; i < enemyCharacterStatsObjects.Count; i++)
        {
            CharacterStats enemyStats = enemyCharacterStatsObjects[i].GetComponent<CharacterStats>();
            enemyCharacterStatsObjects[i].GetComponent<RectTransform>().transform.position = Camera.main.WorldToScreenPoint(enemyStats.characterObjectTransform.position + enemyStats.characterStatsPositionOffset);
        }

    }

    void SetCharacterStatsObject(GameObject statsObject, CharacterObject characterObject)
    {
        CharacterStats characterStats = statsObject.GetComponent<CharacterStats>();
        characterStats.characterNameText.text = characterObject.characterDefinition.characterName;
        characterStats.characterHealthText.text = characterObject.currentHealth.ToString();
        characterStats.characterHealthSlider.maxValue = characterObject.characterDefinition.characterHealth;
        characterStats.characterHealthSlider.value = characterObject.characterDefinition.characterHealth;

        if(characterObject.currentBlock > 0)
        {
            characterStats.characterBlockText.text = characterObject.currentBlock.ToString();
        }
        else
        {
            characterStats.characterBlockText.text = "";
        }

        characterStats.characterObjectTransform = characterObject.transform;
        characterStats.characterStatsPositionOffset = characterObject.characterStatsPositionOffset;
    }

    void HideGameHUD()
    {
        cardPointsPanel.SetActive(false);
        endTurnButton.SetActive(false);

    }

    void ShowGameHUD()
    {
        cardPointsPanel.SetActive(true);
        endTurnButton.SetActive(true);
    }

    void HideEndTurnButton()
    {
        endTurnButton.SetActive(false);
    }

    void ShowEndTurnButton()
    {
        endTurnButton.SetActive(true);
    }

    void HideVictoryPanel()
    {
        victoryPanel.SetActive(false);
    }

    void ShowVictoryPanel()
    {
        victoryPanel.SetActive(true);
    }

    void AnimateVictoryPanel()
    {
        HideGameHUD();

        currentCardDeckButton.SetActive(false);
        discardedCardDeckButton.SetActive(false);

        currentCardDeckGridPanelObject.SetActive(false);
        discardedCardDeckGridPanelObject.SetActive(false);

        HideCharacterStatsObjects();

        StopAllCoroutines();
        StartCoroutine(AnimateFadeInVictoryPanelRoutine(1f));
    }

    IEnumerator AnimateFadeInVictoryPanelRoutine(float fadeOutDelay)
    {
        float currentTime;
        for (currentTime = 0f; currentTime < animateVictoryPanelFadeTime; currentTime += Time.deltaTime)
        {
            float normalizedTime = (1f / animateVictoryPanelFadeTime) * currentTime;
            victoryPanelCanvasGroup.alpha = Mathf.Lerp(victoryPanelCanvasGroup.alpha, 1f, normalizedTime);

            yield return null;
        }

        victoryPanelCanvasGroup.alpha = 1f;

        StartCoroutine(AnimateFadeOutVictoryPanelRoutine(fadeOutDelay));
    }

    IEnumerator AnimateFadeOutVictoryPanelRoutine(float fadeOutDelay)
    {
        yield return new WaitForSeconds(fadeOutDelay);

        float currentTime;
        for (currentTime = 0f; currentTime < animateVictoryPanelFadeTime; currentTime += Time.deltaTime)
        {
            float normalizedTime = (1f / animateVictoryPanelFadeTime) * currentTime;
            victoryPanelCanvasGroup.alpha = Mathf.Lerp(victoryPanelCanvasGroup.alpha, 0f, normalizedTime);

            yield return null;
        }

        victoryPanelCanvasGroup.alpha = 0f;

        if (levelLoader != null)
        {
            levelLoader.SetSceneIndex(1);
            levelLoader.LoadNextLevel();
        }
        else
        {
            ShowBackToMapButton();
        }
    }

    void HideBackToMapButton()
    {
        backToMapButton.SetActive(false);
    }

    void ShowBackToMapButton()
    {
        backToMapButton.SetActive(true);
    }

    void HideBackToMainButton()
    {
        backToMainMenuButton.SetActive(false);
    }

    void ShowBackToMainMenuButton()
    {
        backToMainMenuButton.SetActive(true);
    }


    void AnimateGameOverPanel()
    {
        HideGameHUD();

        currentCardDeckButton.SetActive(false);
        discardedCardDeckButton.SetActive(false);

        currentCardDeckGridPanelObject.SetActive(false);
        discardedCardDeckGridPanelObject.SetActive(false);

        HideCharacterStatsObjects();

        StopAllCoroutines();
        StartCoroutine(AnimateFadeInGameOverPanelRoutine(1f));
    }

    IEnumerator AnimateFadeInGameOverPanelRoutine(float fadeOutDelay)
    {
        float currentTime;
        for (currentTime = 0f; currentTime < animateGameOverPanelFadeTime; currentTime += Time.deltaTime)
        {
            float normalizedTime = (1f / animateGameOverPanelFadeTime) * currentTime;
            gameOverPanelCanvasGroup.alpha = Mathf.Lerp(gameOverPanelCanvasGroup.alpha, 1f, normalizedTime);

            yield return null;
        }

        gameOverPanelCanvasGroup.alpha = 1f;

        StartCoroutine(AnimateFadeOutGameOverPanelRoutine(fadeOutDelay));
    }

    IEnumerator AnimateFadeOutGameOverPanelRoutine(float fadeOutDelay)
    {
        yield return new WaitForSeconds(fadeOutDelay);

        float currentTime;
        for (currentTime = 0f; currentTime < animateGameOverPanelFadeTime; currentTime += Time.deltaTime)
        {
            float normalizedTime = (1f / animateGameOverPanelFadeTime) * currentTime;
            gameOverPanelCanvasGroup.alpha = Mathf.Lerp(gameOverPanelCanvasGroup.alpha, 0f, normalizedTime);

            yield return null;
        }

        gameOverPanelCanvasGroup.alpha = 0f;

        if(levelLoader != null)
        {
            levelLoader.SetSceneIndex(1);
            levelLoader.LoadNextLevel();
        }
        else
        {
            ShowBackToMainMenuButton();
        }
    }

    void UpdateCardPointsText(int cardPoints)
    {
        if (cardPointsText != null)
        {
            cardPointsText.text = cardPoints.ToString();
        }
    }

    void UpdateCurrentCardDeckList(List<Card> currentCardDeck)
    {
        DisableCardUIObjects(currentCardDeckCardUIObjects);

        foreach(Card card in currentCardDeck)
        {
            foreach(CardUIObject cardUIObject in currentCardDeckCardUIObjects)
            {
                if(card == cardUIObject.cardDefinition)
                {
                    if(cardUIObject.gameObject.activeSelf == false)
                    {
                        cardUIObject.gameObject.SetActive(true);
                        break;
                    }
                    
                }
            }
        }
    }

    //update when emptying discarded deck
    void UpdateDiscardedCardDeckListEmptied()
    {
        DisableCardUIObjects(discardedCardDeckCardUIObjects);
    }

    //update when adding card to discarded deck
    void UpdateDiscardedCardDeckListAdded(Card card)
    {
        foreach (CardUIObject cardUIObject in discardedCardDeckCardUIObjects)
        {
            if (card == cardUIObject.cardDefinition)
            {
                if (cardUIObject.gameObject.activeSelf == false)
                {
                    cardUIObject.gameObject.SetActive(true);
                    cardUIObject.GetComponent<Animator>().SetTrigger("pulse");
                    break;
                }
            }
        }
    }

    void DisableCardUIObjects(List<CardUIObject> cardObjectList)
    {
        foreach (CardUIObject cardUIObject in cardObjectList)
        {
            cardUIObject.gameObject.SetActive(false);
        }
    }

    public void OnPressCurrentCardDeckButton()
    {
        currentCardDeckButtonPressed = !currentCardDeckButtonPressed;

        if(currentCardDeckButtonPressed)
        {
            currentCardDeckButtonText.text = "Return";
        }
        else
        {
            currentCardDeckButtonText.text = "Current Deck";
        }

        currentCardDeckGridPanelObject.SetActive(currentCardDeckButtonPressed);
        discardedCardDeckButton.SetActive(!currentCardDeckButtonPressed);

        mouseClickBlocker.SetActive(currentCardDeckButtonPressed);

        if(currentCardDeckButtonPressed)
        {
            HideCharacterStatsObjects();
        }
        else
        {
            ShowCharacterStatsObjects();
        }
    }

    public void OnPressDiscardedCardDeckButton()
    {
        discardedCardDeckButtonPressed = !discardedCardDeckButtonPressed;

        if (discardedCardDeckButtonPressed)
        {
            discardedCardDeckButtonText.text = "Return";
        }
        else
        {
            discardedCardDeckButtonText.text = "Discarded Deck";
        }

        discardedCardDeckGridPanelObject.SetActive(discardedCardDeckButtonPressed);
        currentCardDeckButton.SetActive(!discardedCardDeckButtonPressed);

        mouseClickBlocker.SetActive(discardedCardDeckButtonPressed);

        if (discardedCardDeckButtonPressed)
        {
            HideCharacterStatsObjects();
        }
        else
        {
            ShowCharacterStatsObjects();
        }
    }

    void UpdateStats(CharacterObject characterObject, int damage)
    {
        int block = 0;

        if (characterObject.characterDefinition.characterType == CharacterType.PLAYER)
        {
            if (playerCharacterStats.characterBlockText.text != "")
            {
                block = int.Parse(playerCharacterStats.characterBlockText.text);
            }
        }
        else
        {
            int id = combatManager.enemyCharacters.IndexOf(characterObject);
            CharacterStats enemyStats = enemyCharacterStatsObjects[id].GetComponent<CharacterStats>();
            if (enemyStats.characterBlockText.text != "")
            {
                block = int.Parse(enemyStats.characterBlockText.text);
            }
        }

        if (block >= damage)
        {
            UpdateBlockStatWithValue(characterObject, block - damage);
        }
        else
        {
            if(block == 0)
            {
                UpdateHealthStat(characterObject, damage);
            }
            else
            {
                int remainder = damage - block;
                UpdateBlockStatWithValue(characterObject, 0);
                UpdateHealthStat(characterObject, remainder);
            }
        }

        UpdateBuffStatsObjects(characterObject);
    }

    void UpdateHealthStat(CharacterObject characterObject, int damage)
    {
        if(characterObject.characterDefinition.characterType == CharacterType.PLAYER)
        {
            Slider playerHealthSlider = playerCharacterStats.characterHealthSlider;
            int currentHealth = 0;
            if (characterObject.currentHealth <= 0)
            {
                playerCharacterStatsObject.SetActive(false);
                playerCharacterStatsObject.GetComponent<CharacterStats>().isPermenantlyDisabled = true;
            }
            else
            {
                currentHealth = (int) playerHealthSlider.value;
                playerHealthSlider.value = Mathf.Clamp(currentHealth - damage, 0, characterObject.characterDefinition.characterHealth);
            }
            playerCharacterStats.characterHealthText.text = Mathf.Clamp(currentHealth - damage, 0, characterObject.characterDefinition.characterHealth).ToString();
        }
        else
        {
            int id = combatManager.enemyCharacters.IndexOf(characterObject);
            CharacterStats enemyStats = enemyCharacterStatsObjects[id].GetComponent<CharacterStats>();
            Slider enemyHealthSlider = enemyStats.characterHealthSlider;
            int currentHealth = 0;
            if(characterObject.currentHealth <= 0)
            {
                enemyCharacterStatsObjects[id].SetActive(false);
                enemyCharacterStatsObjects[id].GetComponent<CharacterStats>().isPermenantlyDisabled = true;

            }
            else
            {
                currentHealth = (int)enemyHealthSlider.value;
                enemyHealthSlider.value = Mathf.Clamp(currentHealth - damage, 0, characterObject.characterDefinition.characterHealth);
            }
            enemyStats.characterHealthText.text = Mathf.Clamp(currentHealth - damage, 0, characterObject.characterDefinition.characterHealth).ToString();
        }
    }

    void UpdateBlockStat(CharacterObject characterObject)
    {
        if (characterObject.characterDefinition.characterType == CharacterType.PLAYER)
        {
            playerCharacterStats.characterBlockText.text = characterObject.currentBlock.ToString();
        }
        else
        {
            int id = combatManager.enemyCharacters.IndexOf(characterObject);
            CharacterStats enemyStats = enemyCharacterStatsObjects[id].GetComponent<CharacterStats>();
            enemyStats.characterBlockText.text = characterObject.currentBlock.ToString();
        }
    }

    void UpdateBlockStatWithValue(CharacterObject characterObject, int value)
    {
        if (characterObject.characterDefinition.characterType == CharacterType.PLAYER)
        {
            if(value == 0)
            {
                playerCharacterStats.characterBlockText.text = "";
            }
            else
            {
                playerCharacterStats.characterBlockText.text = value.ToString();
            }
        }
        else
        {
            int id = combatManager.enemyCharacters.IndexOf(characterObject);
            CharacterStats enemyStats = enemyCharacterStatsObjects[id].GetComponent<CharacterStats>();
            if (value == 0)
            {
                enemyStats.characterBlockText.text = "";
            }
            else
            {
                enemyStats.characterBlockText.text = value.ToString();
            }
        }
    }

    public void UpdateBuffStatsObjects(CharacterObject characterObject)
    {
        if (characterObject.characterDefinition.characterType == CharacterType.PLAYER)
        {
            if(characterObject.buffs.Count > 0)
            {
                for (int i = 0; i < playerCharacterStats.buffTextParent.transform.childCount; i++)
                {
                    if (i < characterObject.buffs.Count)
                    {
                        if(characterObject.buffs[i].lifetime > 0)
                        {
                            playerCharacterStats.buffTextParent.transform.GetChild(i).gameObject.SetActive(true);
                            TMP_Text buffText = playerCharacterStats.buffTextParent.transform.GetChild(i).GetComponent<TMP_Text>();
                            buffText.text = characterObject.buffs[i].name;

                            BuffInfo buffInfo = playerCharacterStats.buffTextParent.transform.GetChild(i).GetComponent<BuffText>().buffInfo;
                            buffInfo.name = characterObject.buffs[i].name;
                        }
                        else
                        {
                            playerCharacterStats.buffTextParent.transform.GetChild(i).gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        playerCharacterStats.buffTextParent.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }
            }
        }
        else
        {
            int id = combatManager.enemyCharacters.IndexOf(characterObject);
            CharacterStats enemyStats = enemyCharacterStatsObjects[id].GetComponent<CharacterStats>();

            if (characterObject.buffs.Count > 0)
            {
                for (int i = 0; i < enemyStats.buffTextParent.transform.childCount; i++)
                {
                    if (i < characterObject.buffs.Count)
                    {
                        if (characterObject.buffs[i].lifetime > 0)
                        {
                            enemyStats.buffTextParent.transform.GetChild(i).gameObject.SetActive(true);
                            TMP_Text buffText = enemyStats.buffTextParent.transform.GetChild(i).GetComponent<TMP_Text>();
                            buffText.text = characterObject.buffs[i].name;

                            BuffInfo buffInfo = enemyStats.buffTextParent.transform.GetChild(i).GetComponent<BuffText>().buffInfo;
                            buffInfo.name = characterObject.buffs[i].name;
                        }
                        else
                        {
                            enemyStats.buffTextParent.transform.GetChild(i).gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        enemyStats.buffTextParent.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    void HideCharacterStatsObjects()
    {
        if(playerCharacterStatsObject != null)
        {
            playerCharacterStatsObject.SetActive(false);
        }

        for (int i = 0; i < enemyCharacterStatsObjects.Count; i++)
        {
            if(enemyCharacterStatsObjects[i] != null)
            {
                enemyCharacterStatsObjects[i].SetActive(false);
            }
        }
    }

    void ShowCharacterStatsObjects()
    {
        if (playerCharacterStatsObject != null)
        {
            playerCharacterStatsObject.SetActive(true);
        }

        for (int i = 0; i < enemyCharacterStatsObjects.Count; i++)
        {
            if (enemyCharacterStatsObjects[i] != null)
            {
                if (enemyCharacterStatsObjects[i].GetComponent<CharacterStats>().isPermenantlyDisabled == false)
                {
                    enemyCharacterStatsObjects[i].SetActive(true);
                }
            }
        }
    }

    void RemoveCharacterStatsObject(int index)
    {
        GameObject enemyStatsObject = enemyCharacterStatsObjects[index];
        enemyCharacterStatsObjects.RemoveAt(index);
        Destroy(enemyStatsObject);
    }

    void RemovePlayerStatsObject(CharacterObject characterObject)
    {
        if(characterObject.characterDefinition.characterType == CharacterType.PLAYER)
        {
            Destroy(playerCharacterStatsObject);
        }
    }

    void HideBuffInfoPanel()
    {
        buffInfoTextPanel.SetActive(false);
    }

    void ShowBuffInfoPanel()
    {
        buffInfoTextPanel.SetActive(true);
    }

    void UpdateBuffInfoTextPanel(Transform buffTextTransform)
    {
        BuffText buffText = buffTextTransform.GetComponent<BuffText>();
        foreach (Buff buff in combatManager.buffLibrary)
        {
            if(buff.name == buffText.buffInfo.name)
            {
                buffInfoText.text = buff.description;
                break;
            }
        }

        ShowBuffInfoPanel();
    }

    private void OnDestroy()
    {
        CombatManager.OnCardsDealt -= UpdateCurrentCardDeckList;
        CombatManager.OnCardDiscarded -= UpdateDiscardedCardDeckListAdded;
        CombatManager.OnDiscardEmptied -= UpdateDiscardedCardDeckListEmptied;

        CombatManager.OnUpdateCardPoints -= UpdateCardPointsText;
        CombatManager.OnHasWon -= AnimateVictoryPanel;
        CombatManager.OnPlayerDestroyed -= AnimateGameOverPanel;
        CombatManager.OnEnemyTurn -= HideEndTurnButton;
        CombatManager.OnEnemyTurnComplete -= ShowEndTurnButton;
        CombatManager.OnTurnComplete -= UpdateBuffStatsObjects;

        CombatManager.OnEnemyRemoved -= RemoveCharacterStatsObject;

        CharacterObject.OnPlayerExecuteTurn -= HideEndTurnButton;
        CharacterObject.OnPlayerAttackComplete -= ShowEndTurnButton;
        CharacterObject.OnPlayerUseAbility -= UpdateBlockStat;
        CharacterObject.OnAnimationHit -= UpdateStats;
        CharacterObject.OnCharacterDestroy -= RemovePlayerStatsObject;

        BuffText.OnEnterBuffText -= UpdateBuffInfoTextPanel;
        BuffText.OnExitBuffText -= HideBuffInfoPanel;

    }
}
