using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class IntList
{
    public List<int> intList;
}

[Serializable]
public class CharacterObjectArray
{
    public CharacterObject[] characterObjects;
}

[Serializable]
public class SceneIndexInfo
{
    public int sceneIndex;
    public string title;
    [TextArea]
    public string description;
}

[Serializable]
public class MapPointSerializable
{
    public float[] point;
    public float radius;

    public int mapPointType;

    public int mapPointCombatSceneIndex;
    public int mapPointCombatSceneArrayIndex;
    public string title;
    public string description;

    public int enemySpawnCount;

    public int enemyCharacterRangeMin;
    public int enemyCharacterRangeMax;
    public int eventType;

    public int[] neighbours;
    public bool allowMultipleIncomingConnection = false;
    public bool allowMultipleOutgoingConnection = false;

    public int incomingConnections = 0;
    public int outgoingConnections = 0;

    public int id;
    public int stepCount;

    public MapPointSerializable(MapPoint mapPoint)
    {
        point = new float[3];
        point[0] = mapPoint.point.x;
        point[1] = mapPoint.point.y;
        point[2] = mapPoint.point.z;

        radius = mapPoint.radius;

        mapPointType = (int)mapPoint.mapPointType;

        mapPointCombatSceneIndex = mapPoint.mapPointCombatSceneIndex;
        mapPointCombatSceneArrayIndex = mapPoint.mapPointCombatSceneArrayIndex;

        title = mapPoint.title;
        description = mapPoint.description;

        enemySpawnCount = mapPoint.enemySpawnCount;

        enemyCharacterRangeMin = mapPoint.enemyCharacterRangeMin;
        enemyCharacterRangeMax = mapPoint.enemyCharacterRangeMax;

        eventType = (int)mapPoint.eventType;

        neighbours = new int[mapPoint.neighbours.Count];
        for(int i = 0; i < neighbours.Length; i++)
        {
            neighbours[i] = mapPoint.neighbours[i];
        }
           
        allowMultipleIncomingConnection = mapPoint.allowMultipleIncomingConnection;
        allowMultipleOutgoingConnection = mapPoint.allowMultipleOutgoingConnection;

        incomingConnections = mapPoint.incomingConnections;
        outgoingConnections = mapPoint.outgoingConnections;

        id = mapPoint.id;
        stepCount = mapPoint.stepCount;
    }
}

[Serializable]
public class SaveFile
{
    public int currentPlayerCharacterIndex;
    public int currentPlayerMapPointIndex;
    public List<MapPointSerializable> mapPoints;

    public SaveFile(int currentPlayerIndex, int playerMapPointIndex, List<MapPointSerializable> points)
    {
        currentPlayerCharacterIndex = currentPlayerIndex;
        currentPlayerMapPointIndex = playerMapPointIndex;
        mapPoints = points;
    }
}

public class GameManager : MonoBehaviour
{
    public static event Action OnLoadGameError;
    public static event Action OnLoadGameSuccess;

    string websiteURL = "https://alecli1985.github.io";
    public AudioSource mainMenuMusic;

    public CharacterObject[] playerCharacterObjects;
    public List<CharacterObjectArray> enemyCharacterObjects;
    //public CharacterObject[] forestEnemyCharacterObjects;
    //public CharacterObject[] graveyardEnemyCharacterObjects;

    public CharacterObject[] eliteCharacterObjects;
    public CharacterObject[] minibossCharacterObjects;

    public SceneIndexInfo[] combatSceneIndices;
    public SceneIndexInfo miniBossSceneIndexInfo;
    public SceneIndexInfo eliteSceneIndexInfo;

    public List<Card> cardLibrary;
    public List<Buff> buffLibrary;


    public int currentPlayerCharacterIndex;
    public CharacterObject currentPlayerCharacterObject;
    public PlayerCharacter currentPlayerCharacter;
    
    public List<Card> playerCardDeck; //gets copied over to the currentCardDeck during combat and serialized when saving

    public MapPoint currentMapPoint;
    public List<MapPoint> currentGameMapPoints;
    public List<IntList> currentGameMapPointNeighbours;

    public bool gameInProgress = false;

    public static GameManager instance;

    MapManager mapManager;

    List<MapPoint> tempPoints;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        //we only have one player character so just copy their starting cards into the player deck
        //will eventually change this if there are multiple characters to play as
        playerCardDeck = new List<Card>();
        currentGameMapPoints = new List<MapPoint>();
        currentGameMapPointNeighbours = new List<IntList>();

        Debug.Log(Application.dataPath);
    }

    public void SelectCharacter(int id)
    {
        if(id >= playerCharacterObjects.Length)
        {
            //Debug.Log("character not in character array - id out of range");
        }
        else
        {
            currentPlayerCharacterIndex = id;
            currentPlayerCharacterObject = playerCharacterObjects[id];
            currentPlayerCharacter = currentPlayerCharacterObject.characterDefinition as PlayerCharacter;
            
        }
    }

    public void SetPlayerDeck()
    {
        if(currentPlayerCharacter != null)
        {
            for (int i = 0; i < currentPlayerCharacter.startingCards.Length; i++)
            {
                for (int j = 0; j < currentPlayerCharacter.startingCardsAmount[i]; j++)
                {
                    playerCardDeck.Add(currentPlayerCharacter.startingCards[i]);
                }
            }
        }
    }

    public void ResetMapPoints()
    {
        currentGameMapPoints.Clear();
        foreach (IntList neighbourList in currentGameMapPointNeighbours)
        {
            neighbourList.intList.Clear();
        }
        currentGameMapPointNeighbours.Clear();

        gameInProgress = false;
    }

    public void SetMapPoints(List<MapPoint> mapPoints, MapPoint currentPlayerMapPoint)
    {
        currentGameMapPoints.Clear();
        foreach(IntList neighbourList in currentGameMapPointNeighbours)
        {
            neighbourList.intList.Clear();
        }
        currentGameMapPointNeighbours.Clear();

        foreach (MapPoint mapPoint in mapPoints)
        {
            IntList neighboursList = new IntList();
            neighboursList.intList = new List<int>();
            foreach(int neighbour in mapPoint.neighbours)
            {
                neighboursList.intList.Add(neighbour);
            }

            currentGameMapPointNeighbours.Add(neighboursList);

            currentGameMapPoints.Add(mapPoint);
        }

        currentMapPoint = currentPlayerMapPoint;
    }

    public List<MapPoint> GetMapPoints()
    {
        return currentGameMapPoints;
    }

    public List<IntList> GetMapPointNeighbours()
    {
        return currentGameMapPointNeighbours;
    }

    public void SetEnemyEncounter(MapPoint mapPoint, MapManager mapManager)
    {
        int randomEnemyCount = 0;

        if(mapPoint.mapPointType == MapPointType.BATTLE)
        {
            //pick enemies based on combat scene
            int combatSceneIndex = Random.Range(0, combatSceneIndices.Length);

            //always 1 or 2 goblins on the first battle
            if (mapPoint.stepCount == 1)
            {
                randomEnemyCount = Random.Range(1, 3);

                mapPoint.enemySpawnCount = randomEnemyCount;
                mapPoint.enemyCharacterRangeMin = 0;
                mapPoint.enemyCharacterRangeMax = 0;

                mapPoint.mapPointCombatSceneIndex = combatSceneIndices[0].sceneIndex;
                mapPoint.mapPointCombatSceneArrayIndex = 0;
                mapPoint.title = combatSceneIndices[0].title;
                mapPoint.description = combatSceneIndices[0].description;
            }
            else
            {
                mapPoint.mapPointCombatSceneIndex = combatSceneIndices[combatSceneIndex].sceneIndex;
                mapPoint.mapPointCombatSceneArrayIndex = combatSceneIndex;
                mapPoint.title = combatSceneIndices[combatSceneIndex].title;
                mapPoint.description = combatSceneIndices[combatSceneIndex].description;

                randomEnemyCount = Random.Range(1, 5);

                mapPoint.enemySpawnCount = randomEnemyCount;

                //enemy count should affect composition, a low count should consist of harder enemies
                //high counts should consist of easier enemies, in general
                //float percent = (float)mapPoint.stepCount / (float)mapManager.maxMapPointsPerPath;
                //int minCharacterRange = Mathf.RoundToInt(percent * enemyCharacterObjects.Length) - 2;
                //int maxCharacterRange = Mathf.RoundToInt(percent * enemyCharacterObjects.Length) + 1;

                //mapPoint.enemyCharacterRangeMin = Mathf.Clamp(minCharacterRange, 0, enemyCharacterObjects.Length);
                //mapPoint.enemyCharacterRangeMax = Mathf.Clamp(maxCharacterRange, 0, enemyCharacterObjects.Length);

                mapPoint.enemyCharacterRangeMin = 0;
                mapPoint.enemyCharacterRangeMax = enemyCharacterObjects[combatSceneIndex].characterObjects.Length;
            }
        }
        else if(mapPoint.mapPointType == MapPointType.ELITEBATTLE)
        {
            mapPoint.mapPointCombatSceneIndex = eliteSceneIndexInfo.sceneIndex;
            mapPoint.mapPointCombatSceneArrayIndex = 0;
            mapPoint.title = eliteSceneIndexInfo.title;
            mapPoint.description = eliteSceneIndexInfo.description;

            randomEnemyCount = 1;

            mapPoint.enemySpawnCount = randomEnemyCount;

            mapPoint.enemyCharacterRangeMin = 0;
            mapPoint.enemyCharacterRangeMax = eliteCharacterObjects.Length;
        }
        else if(mapPoint.mapPointType == MapPointType.MINIBOSS)
        {
            mapPoint.mapPointCombatSceneIndex = miniBossSceneIndexInfo.sceneIndex;
            mapPoint.mapPointCombatSceneArrayIndex = 0;
            mapPoint.title = miniBossSceneIndexInfo.title;
            mapPoint.description = miniBossSceneIndexInfo.description;

            randomEnemyCount = 1;

            mapPoint.enemySpawnCount = randomEnemyCount;
            mapPoint.enemyCharacterRangeMin = 0;
            mapPoint.enemyCharacterRangeMax = minibossCharacterObjects.Length;
        }
    }

    public void StopMusic()
    {
        if(mainMenuMusic != null)
        {
            mainMenuMusic.Stop();
        }
    }

    public void SetMapManager(MapManager manager)
    {
        mapManager = manager;
    }

    public bool CheckSaveFileExists()
    {
        string path = Application.persistentDataPath + "/save.data";
        return File.Exists(path);
    }

    public void LoadGame()
    {
        if(CheckSaveFileExists())
        {
            string path = Application.persistentDataPath + "/save.data";

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SaveFile saveData = formatter.Deserialize(stream) as SaveFile;

            stream.Close();

            if (saveData != null)
            {
                currentPlayerCharacterIndex = saveData.currentPlayerCharacterIndex;
                currentPlayerCharacterObject = playerCharacterObjects[currentPlayerCharacterIndex];
                currentPlayerCharacter = currentPlayerCharacterObject.characterDefinition as PlayerCharacter;
                SetPlayerDeck();

                if (saveData.mapPoints.Count > 0)
                {
                    SetMapPoints(saveData.mapPoints, saveData.currentPlayerMapPointIndex);
                    gameInProgress = true;

                    if (OnLoadGameSuccess != null)
                    {
                        OnLoadGameSuccess.Invoke();
                    }
                }
            }
        }
        else
        {
            if(OnLoadGameError != null)
            {
                OnLoadGameError.Invoke();
            }
        }
    }

    public void SetMapPoints(List<MapPointSerializable> mapPoints, int currentPlayerMapPointIndex)
    {
        currentGameMapPoints.Clear();
        foreach (IntList neighbourList in currentGameMapPointNeighbours)
        {
            neighbourList.intList.Clear();
        }
        currentGameMapPointNeighbours.Clear();

        for (int i = 0; i < mapPoints.Count; i++)
        {
            MapPointSerializable mapPointSerializable = mapPoints[i];

            IntList neighboursList = new IntList();
            neighboursList.intList = new List<int>();

            foreach (int neighbour in mapPointSerializable.neighbours)
            {
                neighboursList.intList.Add(neighbour);
            }

            currentGameMapPointNeighbours.Add(neighboursList);

            MapPoint mapPoint = new MapPoint();
            mapPoint.neighbours = new List<int>();

            mapPoint.point.x = mapPointSerializable.point[0];
            mapPoint.point.y = mapPointSerializable.point[1];
            mapPoint.point.z = mapPointSerializable.point[2];

            mapPoint.radius = mapPointSerializable.radius;

            mapPoint.mapPointType = (MapPointType)mapPointSerializable.mapPointType;

            mapPoint.mapPointCombatSceneIndex = mapPointSerializable.mapPointCombatSceneIndex;
            mapPoint.mapPointCombatSceneArrayIndex = mapPointSerializable.mapPointCombatSceneArrayIndex;

            mapPoint.title = mapPointSerializable.title;
            mapPoint.description = mapPointSerializable.description;

            mapPoint.enemySpawnCount = mapPointSerializable.enemySpawnCount;

            mapPoint.enemyCharacterRangeMin = mapPointSerializable.enemyCharacterRangeMin;
            mapPoint.enemyCharacterRangeMax = mapPointSerializable.enemyCharacterRangeMax;

            mapPoint.eventType = (RandomEventType)mapPointSerializable.eventType;

            mapPoint.allowMultipleIncomingConnection = mapPointSerializable.allowMultipleIncomingConnection;
            mapPoint.allowMultipleOutgoingConnection = mapPointSerializable.allowMultipleOutgoingConnection;

            mapPoint.id = mapPointSerializable.id;
            mapPoint.stepCount = mapPointSerializable.stepCount;

            currentGameMapPoints.Add(mapPoint);
        }

        currentMapPoint = currentGameMapPoints[currentPlayerMapPointIndex];

        //make duplicates reference the same point instead of making them unique
        for (int i = 0; i < currentGameMapPoints.Count; i++)
        {
            for (int j = 0; j < currentGameMapPoints.Count; j++)
            {
                if (j != i)
                {
                    if (currentGameMapPoints[j].Equals (currentGameMapPoints[i]))
                    {
                        currentGameMapPoints[j] = currentGameMapPoints[i];
                    }
                }
            }
        }
    }



    public void Quit()
    {
#if (UNITY_STANDALONE) 
        Application.Quit();
#elif (UNITY_WEBGL)
        Application.OpenURL(websiteURL);
#endif
    }
}
