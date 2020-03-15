using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using System;
using Random = UnityEngine.Random;

public class CombatManager : MonoBehaviour
{
    public static event Action OnFirstFrame;
    public static event Action<int> OnUpdateCardPoints;
    public static event Action OnAllCardPointsUsed;
    public static event Action<List<Card>> OnCardsDealt;
    public static event Action<Card> OnCardDiscarded;
    public static event Action OnDiscardEmptied;
    public static event Action<CharacterObject> OnTurnComplete;
    public static event Action OnEnemyTurn;
    public static event Action OnEnemyTurnComplete;
    public static event Action<int> OnEnemyRemoved;


    public static event Action OnHasWon;
    public static event Action OnPlayerDestroyed;

    public Transform playerSpawnPoint;
    public Transform enemySpawnPointParent;

    public GameObject mouseWorldObject;
    public float mouseWorldZ = 1f;
    public float cardSelectedMouseWorldZ = 10f;

    public LayerMask cardMask;
    public CardObject cardObject;
    public GameObject playerHandObject;
    public GameObject selectedCardPositionObject;
    public LayerMask characterMask;

    public PathCreator pathCreator;
    public GameObject splineStartPoint;
    public Vector3 splineStartPositionOffset;
    public GameObject splineMidPoint;
    public LineRenderer lineRenderer;

    public int maxCardPoints = 3;
    public int currentMaxCardPoints = 3;
    public int currentCardPoints;
    public List<Card> cardLibrary;
    public List<Buff> buffLibrary;

    public List<Card> playerCurrentCardDeck;
    public List<CardObject> playerCardHand;
    public List<Card> playerDiscardedCards;

    public CharacterObject playerCharacterObject;
    public List<CharacterObject> enemyCharacters;

    public int maxCardsInHand = 5;
    public int currentMaxCardsInHand = 5;

    public float cardSpreadX;
    public float cardSpacing;
    public float cardYOffset;
    public float cardArcAngle;
    public float cardArcRadius;

    GameManager gameManager;

    Transform[] enemySpawnPoints;

    PlayerCharacter playerCharacter;

    bool isPlayerTurn = true;
    bool handDealt = false;
 
    bool hasCardBeenSelected = false;
    CardObject selectedCard;

    Vector3 cardAngleVector = Vector3.zero;

    Vector3[] bezierPathPoints;
    BezierPath bezierPath;
    VertexPath vertexPath;

    bool hasWon = false;
    bool hasDied = false;

    AudioSource combatMusic;

    // Start is called before the first frame update
    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();

        enemyCharacters = new List<CharacterObject>();

        enemySpawnPoints = new Transform[enemySpawnPointParent.childCount];
        for(int i = 0; i < enemySpawnPointParent.childCount; i++)
        {
            enemySpawnPoints[i] = enemySpawnPointParent.GetChild(i);
        }

        playerCurrentCardDeck = new List<Card>();
        if(gameManager != null)
        {
            playerCharacterObject = Instantiate(gameManager.currentPlayerCharacterObject);
            playerCharacterObject.SetStartPositionAndRotation(playerSpawnPoint.position, Quaternion.LookRotation(playerSpawnPoint.forward, Vector3.up));

            playerCharacter = gameManager.currentPlayerCharacter;

            foreach (Card card in gameManager.playerCardDeck)
            {
                playerCurrentCardDeck.Add(card);
            }

            //where is this being defined?
            for(int i = 0; i < gameManager.currentMapPoint.enemySpawnCount; i++)
            {
                if(gameManager.currentMapPoint.mapPointType == MapPointType.BATTLE)
                {
                    int randomIndex = Random.Range(0, gameManager.enemyCharacterObjects[gameManager.currentMapPoint.mapPointCombatSceneArrayIndex].characterObjects.Length);
                    CharacterObject enemy = Instantiate(gameManager.enemyCharacterObjects[gameManager.currentMapPoint.mapPointCombatSceneArrayIndex].characterObjects[Random.Range(gameManager.currentMapPoint.enemyCharacterRangeMin, gameManager.currentMapPoint.enemyCharacterRangeMax)]);
                    enemy.enemyIndex = i;
                    enemy.SetStartPositionAndRotation(enemySpawnPoints[i].position, Quaternion.LookRotation(enemySpawnPoints[i].forward, Vector3.up));
                    enemyCharacters.Add(enemy);
                }
                else if(gameManager.currentMapPoint.mapPointType == MapPointType.ELITEBATTLE)
                {
                    Transform eliteSpawnPoint = GameObject.FindGameObjectWithTag("EliteSpawnPoint").transform;
                    CharacterObject enemy = Instantiate(gameManager.eliteCharacterObjects[Random.Range(gameManager.currentMapPoint.enemyCharacterRangeMin, gameManager.currentMapPoint.enemyCharacterRangeMax)]);
                    enemy.enemyIndex = i;
                    enemy.SetStartPositionAndRotation(eliteSpawnPoint.position, Quaternion.LookRotation(eliteSpawnPoint.forward, Vector3.up));
                    enemyCharacters.Add(enemy);
                }
                else if(gameManager.currentMapPoint.mapPointType == MapPointType.MINIBOSS)
                {
                    Transform eliteSpawnPoint = GameObject.FindGameObjectWithTag("EliteSpawnPoint").transform;
                    CharacterObject enemy = Instantiate(gameManager.minibossCharacterObjects[Random.Range(gameManager.currentMapPoint.enemyCharacterRangeMin, gameManager.currentMapPoint.enemyCharacterRangeMax)]);
                    enemy.enemyIndex = i;
                    enemy.SetStartPositionAndRotation(eliteSpawnPoint.position, Quaternion.LookRotation(eliteSpawnPoint.forward, Vector3.up));
                    enemyCharacters.Add(enemy);
                }
            }
        }
        else
        {

            int count = 0;
            CharacterObject[] charactersInScene = FindObjectsOfType<CharacterObject>();
            foreach (CharacterObject c in charactersInScene)
            {
                if (c.tag == "Player")
                {
                    playerCharacterObject = c;
                    playerCharacterObject.SetStartPositionAndRotation(playerSpawnPoint.position, Quaternion.LookRotation(playerSpawnPoint.forward, Vector3.up));
                    playerCharacter = c.characterDefinition as PlayerCharacter;
                }
                else
                {
                    if(c.characterDefinition.characterType == CharacterType.MINIBOSS ||
                        c.characterDefinition.characterType == CharacterType.ELITE)
                    {
                        Transform eliteSpawnPoint = GameObject.FindGameObjectWithTag("EliteSpawnPoint").transform;
                        c.SetStartPositionAndRotation(eliteSpawnPoint.position, Quaternion.LookRotation(eliteSpawnPoint.forward, Vector3.up));
                    }
                    else
                    {
                        c.SetStartPositionAndRotation(enemySpawnPoints[count].position, Quaternion.LookRotation(enemySpawnPoints[count].forward, Vector3.up));
                    }
                    c.enemyIndex = count;
                    enemyCharacters.Add(c);
                    count++;
                }
            }

            for (int i = 0; i < playerCharacter.startingCards.Length; i++)
            {
                for (int j = 0; j < playerCharacter.startingCardsAmount[i]; j++)
                {
                    playerCurrentCardDeck.Add(playerCharacter.startingCards[i]);
                }
            }

        }

        ShuffleCardList(playerCurrentCardDeck);

        playerCardHand = new List<CardObject>();
        playerDiscardedCards = new List<Card>();

        currentCardPoints = maxCardPoints;

        SetupBezierCurve();

        combatMusic = GetComponent<AudioSource>();

        CharacterObject.OnCharacterDestroy -= RemoveEnemy;
        CharacterObject.OnCharacterDestroy += RemoveEnemy;

    }

    void SetupBezierCurve()
    {
        bezierPathPoints = new Vector3[3];
        bezierPathPoints[0] = bezierPathPoints[1] = bezierPathPoints[2] = Vector3.zero;

        if (splineStartPoint != null)
        {
            bezierPathPoints[2] = splineStartPoint.transform.position;
        }
        if (splineMidPoint != null)
        {
            bezierPathPoints[1] = splineMidPoint.transform.position;
        }
        if (mouseWorldObject != null)
        {
            bezierPathPoints[0] = mouseWorldObject.transform.position;
        }

        if (pathCreator != null)
        {
            bezierPath = new BezierPath(bezierPathPoints);
            pathCreator.bezierPath = bezierPath;

            //vertexPath = new VertexPath(bezierPath, transform);

            if (lineRenderer != null)
            {
                lineRenderer.positionCount = pathCreator.path.NumPoints;
                for (int i = 0; i < lineRenderer.positionCount; i++)
                {
                    lineRenderer.SetPosition(i, pathCreator.path.localPoints[i]);
                }
            }

            pathCreator.pathUpdated -= UpdateLine;
            pathCreator.pathUpdated += UpdateLine;

        }

        lineRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (hasWon == false)
        {
            //if (Input.GetKeyDown(KeyCode.R))
            //{
            //    RemoveRandomCardFromHand();
            //}

            Vector3 mousePos = Input.mousePosition;
            if (selectedCard != null)
            {
                mousePos.z = cardSelectedMouseWorldZ;
            }
            else
            {
                mousePos.z = mouseWorldZ;
            }
            Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

            if (mouseWorldObject != null)
            {
                mouseWorldObject.transform.position = worldMousePos;
            }

 
            if (isPlayerTurn)
            {
                if (handDealt == false)
                {
                    if (cardObject != null && playerHandObject != null)
                    {
                        DealPlayerHand(currentMaxCardsInHand);
                        handDealt = true;
                    }

                    //Debug.Log("start player turn");
                }

                if (playerCharacterObject.isExecutingTurn == false)
                {
                    Ray mouseRay = Camera.main.ScreenPointToRay(mousePos);
                    RaycastHit mouseHit;

                    if (selectedCard == null)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            if (Physics.Raycast(mouseRay, out mouseHit, 100f, cardMask))
                            {
                                if(mouseHit.transform.TryGetComponent(out selectedCard))
                                {
                                    selectedCard.isSelected = true;
                                    hasCardBeenSelected = true;
                                }
                            }
                        }
                    }

                    if (Input.GetMouseButtonUp(0))
                    {
                        hasCardBeenSelected = false;
                    }

                    if (hasCardBeenSelected)
                    {
                        if (selectedCardPositionObject != null)
                        {
                            splineStartPoint.transform.position = selectedCard.GetStartPosition() + splineStartPositionOffset;

                            bezierPath.MovePoint(0, mouseWorldObject.transform.position);
                            bezierPath.MovePoint(bezierPath.NumPoints - 1, splineStartPoint.transform.position);

                            pathCreator.TriggerPathUpdate();

                            lineRenderer.enabled = true;

                            //selectedCard.transform.position = selectedCardPositionObject.transform.position;
                        }
                        //selectedCard.transform.position = mouseWorldObject.transform.position;
                    }
                    else
                    {
                        if (selectedCard != null)
                        {
                            lineRenderer.enabled = false;

                            if(selectedCard.cardDefinition.cardType == CARDTYPE.ATTACK)
                            {
                                if(selectedCard.cardDefinition.useOnGroup)
                                {
                                    UseCardEnemyGroup();
                                }
                                else
                                {
                                    if (Physics.Raycast(mouseRay, out mouseHit, 100f, characterMask))
                                    {
                                        UseCardSingleEnemy(mouseHit);
                                    }
                                    else
                                    {
                                        ResetCardPosition(selectedCard);
                                    }
                                }
                            }
                            else if(selectedCard.cardDefinition.cardType == CARDTYPE.ABILITY)
                            {
                                UseCardAbility();
                            }

                            selectedCard = null;
                        }
                    }

                    if (CheckHasWon())
                    {
                        ClearPlayerHand();
                        ReturnDiscardedToDeck();

                        hasWon = true;
                        if (OnHasWon != null)
                        {
                            OnHasWon.Invoke();
                        }
                    }
                }
            }
            else
            {
                ClearPlayerHand();

                bool allEnemyTurnsExecuted = true;

                if(playerCharacterObject.isDead)
                {
                    if (hasDied == false)
                    {
                        if (CheckHasDied())
                        {
                            if (OnPlayerDestroyed != null)
                            {
                                OnPlayerDestroyed.Invoke();
                            }
                        }
                    }
                }
                else
                {
                    //enemy turn
                    foreach (CharacterObject enemy in enemyCharacters)
                    {
                        if (enemy.isExecutingTurn == false && enemy.turnComplete == false && playerCharacterObject.currentHealth > 0)
                        {
                            enemy.ExecuteTurn(playerCharacterObject.transform, null);
                            allEnemyTurnsExecuted = false;
                        }
                        if (enemy.turnComplete == false)
                        {
                            allEnemyTurnsExecuted = false;
                            break;
                        }
                    }

                    if (allEnemyTurnsExecuted)
                    {
                        //Debug.Log("all enemy turns complete");

                        foreach (CharacterObject enemy in enemyCharacters)
                        {
                            enemy.ResetTurn();

                            if (OnTurnComplete != null)
                            {
                                OnTurnComplete.Invoke(enemy);
                            }
                        }

                        //ReturnDiscardedToDeck();

                        isPlayerTurn = true;
                        handDealt = false;
                        ResetCardPoints();

                        if (OnEnemyTurnComplete != null)
                        {
                            OnEnemyTurnComplete.Invoke();
                        }

                        if (OnTurnComplete != null)
                        {
                            OnTurnComplete.Invoke(playerCharacterObject);
                        }
                    }
                }
            }
            
        }

        //else
        //{
        //    if (hasDied == false)
        //    {
        //        if (CheckHasDied())
        //        {
        //            if (OnHasDied != null)
        //            {
        //                OnHasDied.Invoke();
        //            }
        //        }
        //    }

        //}
    }

    void UseCardSingleEnemy(RaycastHit mouseHit)
    {
        if (CheckCardCost(selectedCard))
        {
            playerCharacterObject.ExecuteTurn(mouseHit.transform, selectedCard);

            UpdateCurrentCardPoints(selectedCard);
            RemoveCardFromHand(selectedCard);
        }
        else
        {
            ResetCardPosition(selectedCard);
        }
    }

    void UseCardEnemyGroup()
    {
        if (CheckCardCost(selectedCard))
        {
            playerCharacterObject.ExecuteTurn(enemyCharacters, selectedCard);

            UpdateCurrentCardPoints(selectedCard);
            RemoveCardFromHand(selectedCard);
        }
        else
        {
            ResetCardPosition(selectedCard);
        }
    }

    void UseCardAbility()
    {
        if (CheckCardCost(selectedCard))
        {
            playerCharacterObject.ExecuteTurn(playerCharacterObject.transform, selectedCard);

            UpdateCurrentCardPoints(selectedCard);
            RemoveCardFromHand(selectedCard);
        }
        else
        {
            ResetCardPosition(selectedCard);
        }
    }

    public void EndPlayerTurn()
    {
        isPlayerTurn = false;

        playerCharacterObject.ReduceBuffLifetime();

        if(OnTurnComplete != null)
        {
            OnTurnComplete.Invoke(playerCharacterObject);
        }

        if(OnEnemyTurn != null)
        {
            OnEnemyTurn.Invoke();
        }
    }

    void ClearPlayerHand()
    {
        for(int i = playerCardHand.Count - 1; i >=0; i--)
        {
            RemoveCardFromHand(playerCardHand[i]);
        }
    }

    void DealPlayerHand(int numCards)
    {
        if(playerCurrentCardDeck.Count > numCards)
        {
            for (int i = 0; i < numCards; i++)
            {
                AddCardFromDeckToHand();
            }
        }
        else
        {
            int cardsToDraw = playerCurrentCardDeck.Count;
            int cardsToLeftDraw = numCards - playerCurrentCardDeck.Count;

            for (int i = 0; i < cardsToDraw; i++)
            {
                AddCardFromDeckToHand();
            }

            ReturnDiscardedToDeck();

            if(OnDiscardEmptied != null)
            {
                OnDiscardEmptied.Invoke();
            }

            ShuffleCardList(playerCurrentCardDeck);

            for(int i = 0; i < cardsToLeftDraw; i++)
            {
                AddCardFromDeckToHand();
            }
        }

        CalculateCardPositions(numCards);

        if(OnCardsDealt != null)
        {
            OnCardsDealt.Invoke(playerCurrentCardDeck);
        }
    }

    void AddCardFromDeckToHand()
    {
        CardObject cardObj = Instantiate(cardObject, playerHandObject.transform, false);

        int cardIndex = Random.Range(0, playerCurrentCardDeck.Count);
        cardObj.cardDefinition = playerCurrentCardDeck[cardIndex];
        playerCardHand.Add(cardObj);

        playerCurrentCardDeck.RemoveAt(cardIndex);
    }

    void ReturnDiscardedToDeck()
    {
        foreach(Card c in playerDiscardedCards)
        {
            playerCurrentCardDeck.Add(c);
        }

        playerDiscardedCards.Clear();
    }

    void ShuffleCardList(List<Card> cards)
    {
        System.Random random = new System.Random();
        int n = cards.Count;

        for(int i = cards.Count - 1; i > 1; i--)
        {
            int randomID = random.Next(i + 1);

            Card card = cards[randomID];
            cards[randomID] = cards[i];
            cards[i] = card;
        }

    }

    void CalculateCardPositions(int numCards)
    {
        float spaceX = (cardSpreadX + cardSpacing) / numCards;
        float startX = -(spaceX * numCards * 0.5f);
        float angleInc = cardArcAngle / numCards;
        float startAngle = -(angleInc * numCards * 0.5f);

        for (int i = 0; i < numCards; i++)
        {
            Vector3 cardPosition = Vector3.zero;
            cardPosition.x = startX + (i * spaceX);
            cardPosition.x += spaceX * 0.5f;
            cardPosition.z = i * .02f;

            cardAngleVector.x = Mathf.Sin((startAngle * Mathf.Deg2Rad) + (angleInc * Mathf.Deg2Rad * i) + (angleInc * Mathf.Deg2Rad * 0.5f));
            cardAngleVector.y = Mathf.Cos((startAngle * Mathf.Deg2Rad) + (angleInc * Mathf.Deg2Rad * i) + (angleInc * Mathf.Deg2Rad * 0.5f));

            if (playerCardHand[i].isOver == false)
            {
                playerCardHand[i].SetStartPosition(cardPosition + (cardAngleVector * cardArcRadius));
                playerCardHand[i].SetLocalStartPosition(cardPosition + (cardAngleVector * cardArcRadius));
            }

            Quaternion cardRotation = Quaternion.Euler(0, 0, startAngle + (angleInc * i) + (angleInc * 0.5f));
            playerCardHand[i].SetStartRotation(cardRotation);
            playerCardHand[i].transform.rotation = cardRotation;
        }
    }

    void RemoveRandomCardFromHand()
    {
        int index = Random.Range(0, playerCardHand.Count);

        CardObject cardObject = playerCardHand[index];
        playerCardHand.RemoveAt(index);

        playerDiscardedCards.Add(cardObject.cardDefinition);

        if (OnCardDiscarded != null)
        {
            OnCardDiscarded.Invoke(cardObject.cardDefinition);
        }

        CalculateCardPositions(playerCardHand.Count);

        Destroy(cardObject.gameObject);
    }

    void RemoveCardFromHand(CardObject cardObject)
    {
        playerCardHand.Remove(cardObject);
        playerDiscardedCards.Add(cardObject.cardDefinition);

        if(OnCardDiscarded != null)
        {
            OnCardDiscarded.Invoke(cardObject.cardDefinition);
        }

        CalculateCardPositions(playerCardHand.Count);

        Destroy(cardObject.gameObject);
    }

    void ResetCardPosition(CardObject cardObject)
    {
        //cardObject.ResetCardPosition(selectedCardPositionObject.transform.position);
        cardObject.MouseExitCard();
        cardObject.isSelected = false;
    }

    bool CheckCardCost(CardObject cardObject)
    {
        return cardObject.cardDefinition.cost <= currentCardPoints;
    }

    void ResetCardPoints()
    {
        currentCardPoints = currentMaxCardPoints;
        if (OnUpdateCardPoints != null)
        {
            OnUpdateCardPoints.Invoke(currentCardPoints);
        }
    }

    void UpdateCurrentCardPoints(CardObject cardObject)
    {
        currentCardPoints -= cardObject.cardDefinition.cost;
        if (OnUpdateCardPoints != null)
        {
            OnUpdateCardPoints.Invoke(currentCardPoints);
        }

        if (currentCardPoints == 0)
        {
            if (OnAllCardPointsUsed != null)
            {
                OnAllCardPointsUsed.Invoke();
            }
        }
    }

    void RemoveEnemy(CharacterObject characterObject)
    {
        if(OnEnemyRemoved != null)
        {
            if(characterObject.characterDefinition.characterType != CharacterType.PLAYER)
            {
                OnEnemyRemoved.Invoke(enemyCharacters.IndexOf(characterObject));
            }
        }

        enemyCharacters.Remove(characterObject);
    }

    void UpdateLine()
    {
        //pathCreator.EditorData.ResetBezierPath(Vector3.zero);

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = pathCreator.path.NumPoints;
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                lineRenderer.SetPosition(i, pathCreator.path.localPoints[i]);
            }
        }
    }

    bool CheckHasWon()
    {
        bool won = true;
        foreach (CharacterObject enemy in enemyCharacters)
        {
            if (enemy.isDead == false)
            {
                won = false;
                break;
            }
                
        }
        return won;
    }

    bool CheckHasDied()
    {
        if(playerCharacterObject == null)
        {
            hasDied = true;
        }

        return hasDied;
    }

    public void StopMusic()
    {
        if(combatMusic != null)
        {
            combatMusic.Stop();
        }
    }

    private void OnDestroy()
    {
        CharacterObject.OnCharacterDestroy -= RemoveEnemy;
        if(pathCreator != null)
        {
            pathCreator.pathUpdated -= UpdateLine;
        }
    }

    private void OnDrawGizmos()
    {
        int cardGizmos = 0;

        if(Application.isPlaying)
        {
            cardGizmos = playerCardHand.Count;
        }
        else
        {
            cardGizmos = maxCardsInHand;
        }

        Gizmos.color = Color.green;
        //Gizmos.DrawLine(playerHandObject.transform.position, playerHandObject.transform.position + cardAngleVector * cardArcRadius);

        float spaceX = (cardSpreadX + cardSpacing) / cardGizmos;
        float startX = -(spaceX * cardGizmos * 0.5f);
        float angleInc = cardArcAngle / cardGizmos;
        float startAngle = -(angleInc * cardGizmos * 0.5f);

        Vector3 angleVector = Vector3.zero;
        for (int i = 0; i < cardGizmos; i++)
        {
            angleVector.x = Mathf.Sin((startAngle * Mathf.Deg2Rad) + (angleInc * Mathf.Deg2Rad * i) + (angleInc * Mathf.Deg2Rad * 0.5f));
            angleVector.y = Mathf.Cos((startAngle * Mathf.Deg2Rad) + (angleInc * Mathf.Deg2Rad * i) + (angleInc * Mathf.Deg2Rad * 0.5f));

            Gizmos.DrawSphere(angleVector * cardArcRadius, 0.01f);
        }
    }
}
