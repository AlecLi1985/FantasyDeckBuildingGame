using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using PathCreation;
using UnityEngine;
using Cinemachine;
using Random = UnityEngine.Random;

public class MapManager : MonoBehaviour
{
    public static event Action OnGenerateMapSuccess;
    public static event Action<MapPoint, bool, bool> CanMoveToMapMarkerEvent;
    public static event Action<MapPoint, bool, bool> AnimatePlayerToMapMarkerComplete;
    public static event Action OnSaveGame;

    [Header("Game Manager to be assigned at runtime")]
    public GameManager gameManager;

    public CanvasGroup mapScreenUICanvasGroup;
    public GameObject pauseScreenUI;

    [Header("Start of real MapManager variables")]
    public LevelLoader levelLoader;
    public GameObject mouseClickBlocker;

    public GameObject playerObject;
    public Vector3 playerObjectOffset = Vector3.zero;
    public float playerAnimateTime = 2f;

    public CinemachineBrain brain;
    public CinemachineVirtualCamera mapCamera;
    public CinemachineTargetGroup startEndPointGroup;
    public float targetWeights = 1f;
    public float targetRadii = 10f;
    public CinemachineVirtualCamera focusCamera;
    public CinemachineVirtualCamera focusCamera2;

    public PathCreator pathCreator;
    public GameObject worldPathObject;
    public Transform worldPathObjectParent;

    public GameObject worldMapMarkerObject;
    public Transform worldMapMarkerObjectParent;
    public Material[] worldMapMarkerMaterials;

    public float displayPointRadius = 1f;
    public float pathDeviationRadius = 6f;

    [Range(0, 1)] [Tooltip("0 - PDS Map Generation : 1 - Slay the Spire Map Generation")]
    public int mapGenerationMode = 0;

    [Header("Map Generation Paramaters : PDS map")]
    public int minimumMapPoints = 30;
    public int maximumMapPoints = 50;
    [Range(1, 6)]
    public int maxNeighoursPerMapPoint = 3;

    [Range(0, 1f)]
    public float percentageEnemyPoints = 0f;
    [Range(0, 1f)]
    [Tooltip("Percentage based on points left after enemy points have been set")]
    public float percentageRandomPoints = 0f;
    [Tooltip("Perecentage to drop elite chance after an elite has just been set to prevent a long chain of elites towards the end")]
    public float elitePercentDropAfterSet = .15f;

    [Range(0, 1f)]
    public float randomEncounterBattle = 0f;
    [Range(0, 1f)]
    public float randomEncounterTreasure = 0f;
    [Range(0, 1f)]
    public float randomEncounterPositive = 0f;
    [Range(0, 1f)]
    public float randomEncounterNegative = 0f;

    public float startPointBoundaryDistance = 25f;
    public float endPointBoundaryDistance = 25f;

    public float minRadius = 1;
    public float maxRadius = 1;

    public Vector3 regionSize = Vector3.one;
    public int rejectionSamples = 30;
    [Range(1f, 2f)]
    public float searchRadiusMultiplier = 1.1f;

    [Header("Map Generation Paramaters : Slay the Spire map")]
    [Range(1, 10)]
    [Tooltip("The number of paths that equates to number of starting points, paths can merge with other paths at any point")]
    public int numberOfPaths;
    [Range(10f, 20f)]
    [Tooltip("How many points will there be in a run, should be standard and consistent")]
    public int maxMapPointsPerPath;
    [Range(0f, 1f)]
    [Tooltip("Percentage of all points that will be an enemy")]
    public float enemyPercentage;
    [Range(0f, 1f)]
    [Tooltip("Percentage that will be a random point after enemy points have been defined")]
    public float randomPercentage;
    [Range(0f, 1f)]
    [Tooltip("Where along the path will the mini boss appear")]
    public float miniBossPlacement;
    [Range(0f, 1f)]
    [Tooltip("Where along the path will the treasure appear")]
    public float treasurePlacement;
    [Range(0f, 1f)]
    [Tooltip("How likely adjacent paths points are to merge if they are the same type")]
    public float mergePathPointsPercentage;

    public float mapPointPathSpacing = 30f;
    public float mapPointPlacementSpacing = 20f;
    public float mapPointPlacementRandomRadius = 10f;
    public float startPointZOffset = 50f;
    public float endPointZOffset = 50f;

    public bool drawDebug = true;

    List<MapPoint> points = new List<MapPoint>();
    List<MapPoint> mapPoints = new List<MapPoint>();
    MapPoint[,] mapPointsArray;

    List<MapPoint> tempPoints = new List<MapPoint>();
    List<MapPoint> tempNeighbourPoints = new List<MapPoint>();

    public MapPoint playerCurrentMapPoint;

    MapPoint startPoint;
    MapPoint endPoint;
    MapPoint northPoint;
    MapPoint southPoint;
    Transform startPointTransform;
    Transform endPointTransform;
    Transform northernMostPoint;
    Transform southernMostPoint;

    public WorldMapMarker focusedMapMarker;
    Vector3 playerSmoothDampVelocity = Vector3.zero;

    bool mapValid = false;

    bool gamePaused = false;

    AudioSource mapMusic;

    //drop percentage chance of elite appearing again after an elite has already been set
    float elitePercentDrop = 0f;

    int duplicateCount = 0;

    //public Terrain terrain;
    //public GameObject terrainEdges;
    //public TerrainData terrainData;

    //public float raiseTerrainHeight = 3f;
    //public float flattenTerrainHeight = 2f;
    //public int samplePositionArea = 5;
    //float[,] startTerrainHeights;
    //float[,] setHeights;

    //BezierPath bezierPath;
    //Vector3[] bezierPathPoints;

    //VertexPath vertexPath;
    //Vector3 samplePathPosition = Vector3.zero;
    //float sampleDistanceIncrement = 0f;
    //public float pathDeviationRadius = 6f;
    //public int pathSampleAmount = 10;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if(gameManager != null)
        {
            gameManager.SetMapManager(this);

            if(gameManager.gameInProgress == false)
            {
                switch (mapGenerationMode)
                {
                    case 0:
                        GeneratePDSMapRoutine();
                        break;
                    case 1:
                        GenerateSTSMapRoutine();
                        break;
                }

                gameManager.SetMapPoints(mapPoints, playerCurrentMapPoint);

                gameManager.gameInProgress = true;
            }
            else
            {
                ClearLists();

                List<MapPoint> currentGameMapPoints = gameManager.GetMapPoints();
                List<IntList> currentGameMapPointNeighbours = gameManager.GetMapPointNeighbours();

                for (int i = 0; i < currentGameMapPoints.Count; i++)
                {
                    MapPoint mapPoint = currentGameMapPoints[i];

                    foreach(int neighbour in currentGameMapPointNeighbours[i].intList)
                    {
                        mapPoint.neighbours.Add(neighbour);
                    }

                    mapPoint.incomingConnections = 0;
                    mapPoint.outgoingConnections = 0;

                    mapPoints.Add(mapPoint);
                }

                duplicateCount = 0;

                foreach (MapPoint mapPoint in mapPoints)
                {
                    RemoveDuplicatePoints(mapPoint.neighbours);
                }

                //Debug.Log(duplicateCount + " duplicates removed");

                playerCurrentMapPoint = gameManager.currentMapPoint;
                PlacePlayerAtMapPoint(gameManager.currentMapPoint);

                GenerateMapFromPoints(mapPoints);
            }
        }

        if (mouseClickBlocker != null)
        {
            mouseClickBlocker.SetActive(false);
        }

        mapMusic = GetComponent<AudioSource>();

        brain = Camera.main.GetComponent<CinemachineBrain>();
        mapCamera = GameObject.FindGameObjectWithTag("MapCamera").GetComponent<CinemachineVirtualCamera>();
        startEndPointGroup = GameObject.FindGameObjectWithTag("StartEndTargetGroup").GetComponent<CinemachineTargetGroup>();
        focusCamera = GameObject.FindGameObjectWithTag("FocusCamera").GetComponent<CinemachineVirtualCamera>();
        focusCamera2 = GameObject.FindGameObjectWithTag("FocusCamera2").GetComponent<CinemachineVirtualCamera>();

        GameManager.OnLoadGameSuccess -= LoadLevel;
        GameManager.OnLoadGameSuccess += LoadLevel;

    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            gamePaused = !gamePaused;

            if(gamePaused)
            {
                mapScreenUICanvasGroup.alpha = 0;
                mapScreenUICanvasGroup.interactable = false;
                pauseScreenUI.SetActive(true);

                mouseClickBlocker.SetActive(true);

            }
            else
            {
                mapScreenUICanvasGroup.alpha = 1;
                mapScreenUICanvasGroup.interactable = true;
                pauseScreenUI.SetActive(false);

                mouseClickBlocker.SetActive(false);
            }
        }
    }

    void GenerateSTSMapRoutine()
    {
        ClearLists();
        GenerateSTSMapPoints();

        FindNorthAndSouthPoints();

        foreach (MapPoint mapPoint in mapPoints)
        {
            DefineMapPoint(mapPoint);

            GameObject mapMarkerObject = Instantiate(worldMapMarkerObject, mapPoint.point + worldMapMarkerObjectParent.transform.position, Quaternion.identity, worldMapMarkerObjectParent);

            WorldMapMarker mapMarker = mapMarkerObject.GetComponent<WorldMapMarker>();
            mapMarker.mapPoint = mapPoint;

            if (mapMarker.mapPoint.mapPointType == MapPointType.START)
            {
                startPointTransform = mapMarker.transform;
            }
            else if (mapMarker.mapPoint.mapPointType == MapPointType.END)
            {
                endPointTransform = mapMarker.transform;
            }

            if (mapPoint.point == northPoint.point)
            {
                northernMostPoint = mapMarker.transform;
            }

            if (mapPoint.point == southPoint.point)
            {
                southernMostPoint = mapMarker.transform;
            }

            mapMarker.SetMapMarkerMaterial();
        }

        GeneratePaths();

        if (startPointTransform != null &&
            endPointTransform != null &&
            northernMostPoint != null &&
            southernMostPoint != null)
        {
            Transform[] targets = new Transform[4];
            targets[0] = startPointTransform;
            targets[1] = endPointTransform;
            targets[2] = northernMostPoint;
            targets[3] = southernMostPoint;

            float[] weights = new float[4];
            weights[0] = targetWeights;
            weights[1] = targetWeights;
            weights[2] = targetWeights;
            weights[3] = targetWeights;

            float[] radius = new float[4];
            radius[0] = targetRadii;
            radius[1] = targetRadii;
            radius[2] = targetRadii;
            radius[3] = targetRadii;

            SetTargetGroup(targets, weights, radius, startEndPointGroup);
        }

        FocusCameraOnWholeMap();

        playerCurrentMapPoint = startPoint;

        WorldMapMarker.OnWorldMapMarkerPressed -= MapMarkerFocused;
        WorldMapMarker.OnWorldMapMarkerPressed += MapMarkerFocused;

        PlacePlayerAtMapMarker(startPoint);

        if (OnGenerateMapSuccess != null)
        {
            OnGenerateMapSuccess.Invoke();
        }
    }

    void GenerateSTSMapRoutineOnValidate()
    {
        ClearLists();
        GenerateSTSMapPoints();
    }

    void ClearLists()
    {
        foreach (MapPoint point in points)
        {
            if(point.neighbours != null && point.neighbours.Count > 0)
            {
                point.neighbours.Clear();
            }
        }
        points.Clear();

        foreach (MapPoint point in mapPoints)
        {
            if (point.neighbours != null && point.neighbours.Count > 0)
            {
                point.neighbours.Clear();
            }
        }
        mapPoints.Clear();

        if(mapPointsArray != null)
        {
            Array.Clear(mapPointsArray, 0, mapPointsArray.Length);
        }

        tempPoints.Clear();
        tempNeighbourPoints.Clear();
    }

    void GenerateSTSMapPoints()
    {
        Vector3 pointPosition = Vector3.zero;

        mapPointsArray = new MapPoint[numberOfPaths,maxMapPointsPerPath];

        int count = 0;
        for (int i = 0; i < numberOfPaths; i++)
        {
            int treasureIndex = Mathf.RoundToInt(maxMapPointsPerPath * treasurePlacement);
            int miniBossIndex = Mathf.RoundToInt(maxMapPointsPerPath * miniBossPlacement);

            int numEnemyPoints = Mathf.RoundToInt(maxMapPointsPerPath * enemyPercentage);
            int numRandomPoints = Mathf.RoundToInt((maxMapPointsPerPath - numEnemyPoints) * randomPercentage);

            for (int e = 0; e < numEnemyPoints; e++)
            {
                MapPoint enemyPoint = new MapPoint();
                enemyPoint.neighbours = new List<int>();

                enemyPoint.mapPointType = MapPointType.BATTLE;
                tempPoints.Add(enemyPoint);
            }
            for (int r = 0; r < numRandomPoints; r++)
            {
                MapPoint randomPoint = new MapPoint();
                randomPoint.neighbours = new List<int>();

                randomPoint.mapPointType = MapPointType.RANDOM;
                tempPoints.Add(randomPoint);
            }

            elitePercentDrop = 0f;

            List<MapPoint> path = new List<MapPoint>();
            for (int p = 0; p < maxMapPointsPerPath; p++)
            {
                if (p == 0)
                {
                    if(i == 0)
                    {
                        MapPoint point = new MapPoint();
                        point.stepCount = 0;
                        point.neighbours = new List<int>();
                        path.Add(point);
                    }
                    else
                    {
                        path.Add(mapPoints[0]);
                    }

                    path[p].mapPointType = MapPointType.START;
                    startPoint = path[p];
                }
                else if (p == 1)
                {
                    MapPoint point = new MapPoint();
                    point.stepCount = p;

                    point.neighbours = new List<int>();

                    path.Add(point);

                    path[p].mapPointType = MapPointType.BATTLE;
                }
                else if (p == treasureIndex)
                {
                    MapPoint point = new MapPoint();
                    point.stepCount = p;

                    point.neighbours = new List<int>();

                    path.Add(point);

                    path[p].mapPointType = MapPointType.TREASURE;
                }
                else if (p == miniBossIndex)
                {
                    MapPoint point = new MapPoint();
                    point.stepCount = p;

                    point.neighbours = new List<int>();

                    path.Add(point);

                    path[p].mapPointType = MapPointType.MINIBOSS;
                }
                else if (p == maxMapPointsPerPath - 1)
                {
                    if (i == 0)
                    {
                        MapPoint point = new MapPoint();
                        point.stepCount = p;

                        point.neighbours = new List<int>();
                        path.Add(point);
                    }
                    else
                    {
                        path.Add(mapPoints[maxMapPointsPerPath - 1]);
                    }

                    path[p].mapPointType = MapPointType.END;
                    endPoint = path[p];
                }
                else if (p == maxMapPointsPerPath - 2)
                {
                    MapPoint point = new MapPoint();
                    point.stepCount = p;

                    point.neighbours = new List<int>();

                    path.Add(point);

                    path[p].mapPointType = MapPointType.SAVE;
                }
                else
                {
                    int randomIndex = Random.Range(0, tempPoints.Count);

                    MapPoint point = tempPoints[randomIndex];
                    point.neighbours = new List<int>();
                    point.stepCount = p;

                    if(p > Mathf.RoundToInt(maxMapPointsPerPath * 0.5f))
                    {
                        //increase likelihood of elite points closer to the end
                        float divider = maxMapPointsPerPath - (p - Mathf.RoundToInt(maxMapPointsPerPath * .25f));
                        float eliteChance = 1f - (divider / maxMapPointsPerPath) - elitePercentDrop;
                        if(Random.value < eliteChance)
                        {
                            point.mapPointType = MapPointType.ELITEBATTLE;
                            elitePercentDrop += elitePercentDropAfterSet;
                        }
                    }

                    path.Add(point);
                    tempPoints.RemoveAt(randomIndex);
                }

                if (p == 0)
                {
                    pointPosition.x = (numberOfPaths * mapPointPathSpacing) * 0.4f;
                    pointPosition.z = -startPointZOffset;
                }
                else if(p == maxMapPointsPerPath - 1)
                {
                    pointPosition.x = (numberOfPaths * mapPointPathSpacing) * 0.4f;
                    pointPosition.z = (maxMapPointsPerPath * mapPointPlacementSpacing) + endPointZOffset;
                }
                else
                {
                    pointPosition.x = i * mapPointPathSpacing;
                    pointPosition.z = p * mapPointPlacementSpacing;

                    pointPosition += Random.onUnitSphere * Random.Range(mapPointPlacementRandomRadius * .5f, mapPointPlacementRandomRadius);
                    pointPosition.y = 0f;
                }

                path[p].point = pointPosition;
                path[p].id = count;
                count++;
            }

            //set random point as shop and random point as save
            int shopIndex = -1;
            while(shopIndex == -1)
            {
                shopIndex = Random.Range(maxMapPointsPerPath / 4, maxMapPointsPerPath - (maxMapPointsPerPath / 4));
                if(path[shopIndex].mapPointType == MapPointType.MINIBOSS || 
                    path[shopIndex].mapPointType == MapPointType.TREASURE)
                {
                    shopIndex = -1;
                }
            }
            
            int saveIndex = -1;
            while (saveIndex == -1)
            {
                saveIndex = Random.Range(maxMapPointsPerPath / 4, maxMapPointsPerPath - (maxMapPointsPerPath / 4));
                if (path[saveIndex].mapPointType == MapPointType.MINIBOSS || 
                    path[saveIndex].mapPointType == MapPointType.TREASURE)
                {
                    saveIndex = -1;
                }
            }

            path[shopIndex].mapPointType = MapPointType.SHOP;
            path[saveIndex].mapPointType = MapPointType.SAVE;

            mapPoints.AddRange(path);

            for (int p = 0; p < path.Count; p++)
            {
                if (p < path.Count - 1)
                {
                    path[p].neighbours.Add(path[p + 1].id);
                }
            }

        }

        count = 0;
        for(int i = 0; i < numberOfPaths; i++)
        {
            for(int p = 0; p < maxMapPointsPerPath; p++)
            {
                mapPointsArray[i, p] = mapPoints[count];
                count++;
            }
        }

        //for (int i = 0; i < numberOfPaths; i++)
        //{
        //    for (int i2 = 0; i2 < numberOfPaths; i2++)
        //    {
        //        mapPointsArray[i, 0].neighbours.Add(mapPointsArray[i2, 1]);
        //    }
        //}

        //merge certain points of the paths
        for (int p = 2; p < maxMapPointsPerPath; p++)
        {
            tempPoints.Clear();
            tempNeighbourPoints.Clear();

            if (Random.value < mergePathPointsPercentage)
            {
                int mergePointCount = 0;
                int currentRowPoint = 0;
                bool canMerge = false;

                pointPosition = Vector3.zero;

                for(int i = 0; i < numberOfPaths; i++)
                {
                    if (mapPointsArray[currentRowPoint, p].mapPointType == mapPointsArray[i, p].mapPointType)
                    {
                        mergePointCount++;
                    }
                    else
                    {
                        if (mergePointCount > 1 && (mergePointCount >= 2 || mergePointCount <= 4))
                        {
                            canMerge = true;
                        }
                        else
                        {
                            currentRowPoint = i;
                            mergePointCount = 0;
                        }
                    }

                    if(mergePointCount == 4)
                    {
                        canMerge = true;
                    }

                    if(canMerge)
                    {
                        if(mapPointsArray[currentRowPoint, p].mapPointType == MapPointType.END)
                        {
                            pointPosition.x = (numberOfPaths * mapPointPathSpacing) * 0.4f;
                            pointPosition.z = (maxMapPointsPerPath * mapPointPlacementSpacing) + endPointZOffset;
                        }
                        else
                        {
                            float pointMinX = mapPointsArray[currentRowPoint, p].point.x;
                            float pointMaxX = mapPointsArray[i, p].point.x;

                            pointPosition.x = pointMinX + ((pointMaxX - pointMinX) * 0.5f);
                            pointPosition.z = mapPointsArray[currentRowPoint, p].point.z;

                            pointPosition += Random.onUnitSphere * Random.Range(mapPointPlacementRandomRadius * .5f, mapPointPlacementRandomRadius);
                            pointPosition.y = 0f;
                        }

                        mapPointsArray[currentRowPoint, p].point = pointPosition;

                        mapPointsArray[currentRowPoint, p].neighbours.Clear();

                        for (int j = currentRowPoint; j < i; j++)
                        {
                            
                            //if(p < maxMapPointsPerPath - 1)
                            //{
                            //    mapPointsArray[currentRowPoint, p].neighbours.Add(mapPointsArray[j, p + 1]);
                            //}

                            mapPointsArray[j, p] = mapPointsArray[currentRowPoint, p];


                            mapPointsArray[j, p - 1].neighbours.Clear();
                            mapPointsArray[j, p - 1].neighbours.Add(mapPointsArray[j, p].id);
                        }


                        currentRowPoint = i;
                        mergePointCount = 0;
                        canMerge = false;
                    }
                }
            }
        }

        count = 0;
        for (int i = 0; i < numberOfPaths; i++)
        {
            for (int p = 0; p < maxMapPointsPerPath; p++)
            {
                if (p < maxMapPointsPerPath - 1)
                {
                    //enforce map points preceeding the end node to only have one neighbour, 
                    if(mapPointsArray[i, p].stepCount == maxMapPointsPerPath - 2)
                    {
                        mapPointsArray[i, p].neighbours.Clear();
                    }

                    if (mapPointsArray[i, p].neighbours.Contains(mapPointsArray[i, p + 1].id) == false)
                    {
                        mapPointsArray[i, p].neighbours.Add(mapPointsArray[i, p + 1].id);
                    }
                }

                mapPoints[count] = mapPointsArray[i, p];
                count++;
            }
        }

        //foreach (MapPoint point in mapPoints)
        //{
        //    RemoveDuplicatePoints(point.neighbours);
        //}
    }

    //should really have a way to express the condition as the paramater
    void GeneratePaths(bool alwaysSingleConnections = false)
    {
        if (pathCreator != null)
        {
            tempPoints.Clear();

            if (mapPoints != null)
            {
                for (int i = 0; i < mapPoints.Count; i++)
                {
                    MapPoint mapPoint = mapPoints[i];
                    tempPoints.Add(mapPoint);

                    if(mapPoint.mapPointType == MapPointType.START)
                    {
                        int x = 0;
                    }

                    if (mapPoint.neighbours != null)
                    {
                        foreach (int neighbour in mapPoint.neighbours)
                        {
                            bool canConnect = false;
                            MapPoint neighbourPoint = GetMapPointByID(mapPoints, neighbour);

                            if (alwaysSingleConnections)
                            {
                                canConnect = tempPoints.Contains(neighbourPoint) == false; 
                            }
                            else
                            {
                                canConnect = mapPoint.outgoingConnections < mapPoint.neighbours.Count;
                            }

                            if (canConnect)
                            {
                                neighbourPoint.incomingConnections++;
                                mapPoint.outgoingConnections++;

                                Vector3[] bezierPathPoints = new Vector3[5];

                                Vector3 randomVectorOne = Random.onUnitSphere;
                                randomVectorOne.y = 0f;
                                Vector3 randomVectorTwo = Random.onUnitSphere;
                                randomVectorTwo.y = 0f;
                                Vector3 randomVectorThree = Random.onUnitSphere;
                                randomVectorThree.y = 0f;

                                bezierPathPoints[0] = mapPoint.point;
                                bezierPathPoints[1] = mapPoint.point + ((neighbourPoint.point - mapPoint.point) * 0.25f + randomVectorOne * pathDeviationRadius);
                                bezierPathPoints[2] = mapPoint.point + ((neighbourPoint.point - mapPoint.point) * 0.5f + randomVectorTwo * pathDeviationRadius);
                                bezierPathPoints[3] = mapPoint.point + ((neighbourPoint.point - mapPoint.point) * 0.75f + randomVectorThree * pathDeviationRadius);
                                bezierPathPoints[4] = neighbourPoint.point;

                                BezierPath bezierPath = new BezierPath(bezierPathPoints);
                                bezierPath.ControlPointMode = BezierPath.ControlMode.Automatic;
                                bezierPath.GlobalNormalsAngle = 90f;
                                pathCreator.bezierPath = bezierPath;

                                VertexPath vertexPath = new VertexPath(bezierPath, pathCreator.transform);
                                vertexPath = pathCreator.EditorData.GetVertexPath(pathCreator.transform);

                                GameObject pathObject = Instantiate(worldPathObject, worldPathObjectParent);
                                pathObject.transform.LookAt(Vector3.down);
                                LineRenderer path = pathObject.GetComponent<LineRenderer>();
                                if (path != null)
                                {
                                    path.positionCount = vertexPath.NumPoints;
                                    for (int k = 0; k < vertexPath.NumPoints; k++)
                                    {
                                        path.SetPosition(k, vertexPath.localPoints[k] + (Vector3.up * .05f));
                                    }
                                }

                                //visitedPoints.Add(neighbour);
                            }
                        }
                    }
                }
            }
        }

       // Debug.Log("generate paths complete");
    }

    public void RegenerateMap()
    {
        ClearMapObjects();

        switch (mapGenerationMode)
        {
            case 0:
                GeneratePDSMapRoutine();
                break;
            case 1:
                GenerateSTSMapRoutine();
                break;
        }

        if(gameManager != null)
        {
            gameManager.SetMapPoints(mapPoints, playerCurrentMapPoint);
        }
    }

    void ClearMapObjects()
    {
        for (int i = worldMapMarkerObjectParent.childCount - 1; i >= 0; i--)
        {
            Destroy(worldMapMarkerObjectParent.GetChild(i).gameObject);
        }
        for (int i = worldPathObjectParent.childCount - 1; i >= 0; i--)
        {
            Destroy(worldPathObjectParent.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// GUI and Camera
    /// </summary>

    void MapMarkerFocused(MapPoint mapPoint, WorldMapMarker worldMapMarker)
    {
        FocusCameraOnMapMarker(mapPoint, worldMapMarker);

        if(CanMoveToMapMarkerEvent != null)
        {
            CanMoveToMapMarkerEvent.Invoke(mapPoint, CheckCanMoveToMapMarker(), mapPoint == playerCurrentMapPoint);
        }
    }

    public void FocusCameraOnWholeMap()
    {
        mapCamera.m_Priority = 1;
        focusCamera.m_Priority = 0;

        //focusedMapMarker = null;
    }

    void FocusCameraOnMapMarker(MapPoint mapPoint, WorldMapMarker worldMapMarker)
    {
        mapCamera.m_Priority = 0;

        if (focusCamera.m_Priority == 0)
        {
            focusCamera.m_LookAt = worldMapMarker.transform;
            focusCamera.m_Follow = worldMapMarker.transform;

            focusCamera.m_Priority = 1;
            focusCamera2.m_Priority = 0;
        }
        else if(focusCamera2.m_Priority == 0)
        {
            focusCamera2.m_LookAt = worldMapMarker.transform;
            focusCamera2.m_Follow = worldMapMarker.transform;

            focusCamera.m_Priority = 0;
            focusCamera2.m_Priority = 1;
        }

        focusedMapMarker = worldMapMarker;
    }

    void SetTargetGroup(Transform[] targets, float[] weights, float[] radius, CinemachineTargetGroup targetGroup)
    {
        for (int i = targetGroup.m_Targets.Length - 1; i >= 0; i--)
        {
            targetGroup.RemoveMember(targetGroup.m_Targets[i].target);
        }

        for (int i = 0; i < targets.Length; i++)
        {
            targetGroup.AddMember(targets[i], weights[i], radius[i]);
        }
    }

    void PlacePlayerAtMapMarker(MapPoint marker)
    {
        if(playerObject != null)
        {
            if(startPoint != null)
            {
                playerObject.transform.position = marker.point + playerObjectOffset;
            }
        }
    }

    void PlacePlayerAtMapPoint(MapPoint mapPoint)
    {
        playerObject.transform.position = mapPoint.point + playerObjectOffset;
    }

    bool CheckCanMoveToMapMarker()
    {
        bool canMoveToMapMarker = false;

        foreach (int neighbour in playerCurrentMapPoint.neighbours)
        {
            MapPoint neighbourPoint = GetMapPointByID(mapPoints, neighbour);
            if(neighbourPoint != null)
            {
                if (focusedMapMarker.mapPoint.point == neighbourPoint.point)
                {
                    canMoveToMapMarker = true;
                    break;
                }
                else
                {
                   // Debug.Log("CheckCanMoveToMapMarker - point invalid");
                }
            }

        }

        return canMoveToMapMarker;
    }

    public void MovePlayerToMapMarker(bool goToCombatScreen = false)
    {
        mapCamera.m_Priority = 0;

        if (focusCamera.m_Priority == 0)
        {
            focusCamera.m_LookAt = playerObject.transform;
            focusCamera.m_Follow = playerObject.transform;

            focusCamera.m_Priority = 1;
            focusCamera2.m_Priority = 0;
        }
        else if (focusCamera2.m_Priority == 0)
        {
            focusCamera2.m_LookAt = playerObject.transform;
            focusCamera2.m_Follow = playerObject.transform;

            focusCamera.m_Priority = 0;
            focusCamera2.m_Priority = 1;
        }

        StopAllCoroutines();
        StartCoroutine(AnimatePlayerToMapMarker(goToCombatScreen));
    }

    IEnumerator AnimatePlayerToMapMarker(bool goToCombatScreen)
    {
        while((playerObject.transform.position - (focusedMapMarker.transform.position + playerObjectOffset)).sqrMagnitude > 1f)
        {
            playerObject.transform.position = Vector3.SmoothDamp(playerObject.transform.position, focusedMapMarker.transform.position + playerObjectOffset, ref playerSmoothDampVelocity, playerAnimateTime, 1000f);

            yield return null;
        }


        playerCurrentMapPoint = focusedMapMarker.mapPoint;
        if(gameManager != null)
        {
            gameManager.currentMapPoint = playerCurrentMapPoint;
        }

        if (goToCombatScreen)
        {
            levelLoader.sceneIndex = gameManager.currentMapPoint.mapPointCombatSceneIndex;
            levelLoader.LoadNextLevel();
        }
        else
        {
            mouseClickBlocker.SetActive(false);

            //FocusCameraOnWholeMap();

            if(AnimatePlayerToMapMarkerComplete != null)
            {
                AnimatePlayerToMapMarkerComplete.Invoke(playerCurrentMapPoint, CheckCanMoveToMapMarker(), true);
            }
        }
    }

    private void OnValidate()
    {
        if(Application.isPlaying == false)
        {
            switch (mapGenerationMode)
            {
                case 0:
                    GeneratePDSMapRoutineOnValidate();

                    break;
                case 1:
                    GenerateSTSMapRoutineOnValidate();
                    break;
            }
        }
    }

    void RemoveDuplicatePoints(List<int> pointsList)
    {
        List<int> temp = new List<int>();

        for(int i = 0; i < pointsList.Count; i++)
        {
            for(int j = pointsList.Count - 1; j > i; j--)
            {
                if(pointsList[i] == pointsList[j])
                {
                    pointsList.RemoveAt(j);
                    duplicateCount++;

                }
            }
        }
    }

    Color GetColor(float r = 1f, float g = 1f, float b = 1f, float a = 1f)
    {
        Color color;
        color.r = r;
        color.g = g;
        color.b = b;
        color.a = a;

        return color;
    }

    private void OnDrawGizmos()
    {
        if (drawDebug)
        {
            Gizmos.DrawWireCube(regionSize / 2, regionSize);

            bool drawPoints = false;
            bool drawMapPoints = false;

            if (points != null && points.Count > 0)
            {
                drawPoints = true;
            }
            if (mapPoints != null && mapPoints.Count > 0)
            {
                drawPoints = false;
                drawMapPoints = true;
            }

            if (drawPoints)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(points[i].point, displayPointRadius);

                    if (points[i].neighbours != null)
                    {
                        if (points[i].neighbours.Count > 0)
                        {
                            Gizmos.color = Color.yellow;
                            foreach (int neighbour in points[i].neighbours)
                            {
                                Gizmos.DrawLine(points[i].point, points[neighbour].point);
                            }
                        }
                    }
                }

            }

            Color enemyColor = Color.white;
            enemyColor.r = .9f;
            enemyColor.g = .15f;
            enemyColor.b = .15f;

            if (drawMapPoints)
            {
                for (int i = 0; i < mapPoints.Count; i++)
                {
                    if (mapPoints[i].mapPointType == MapPointType.START)
                    {
                        Gizmos.color = Color.blue;
                    }
                    else if (mapPoints[i].mapPointType == MapPointType.END)
                    {
                        Gizmos.color = Color.green;
                    }
                    else if(mapPoints[i].mapPointType == MapPointType.BATTLE)
                    {
                        Gizmos.color = enemyColor;
                    }
                    else if (mapPoints[i].mapPointType == MapPointType.ELITEBATTLE)
                    {
                        Gizmos.color = Color.red;
                    }
                    else if (mapPoints[i].mapPointType == MapPointType.RANDOM)
                    {
                        Gizmos.color = Color.white;
                    }
                    else if (mapPoints[i].mapPointType == MapPointType.SHOP)
                    {
                        Gizmos.color = Color.magenta;
                    }
                    else if (mapPoints[i].mapPointType == MapPointType.SAVE)
                    {
                        Gizmos.color = Color.cyan;
                    }
                    else if (mapPoints[i].mapPointType == MapPointType.TREASURE)
                    {
                        Gizmos.color = Color.yellow;
                    }
                    else if (mapPoints[i].mapPointType == MapPointType.MINIBOSS)
                    {
                        Gizmos.color = Color.black;
                    }
                    Gizmos.DrawWireSphere(mapPoints[i].point, displayPointRadius);


                    if (mapPoints[i].neighbours != null)
                    {
                        if (mapPoints[i].neighbours.Count > 0)
                        {
                            Color lineColor = Color.white;
                            lineColor.r = .5f;
                            lineColor.g = .5f;
                            lineColor.b = 0f;
                            Gizmos.color = lineColor;

                            foreach (int neighbour in mapPoints[i].neighbours)
                            {
                                MapPoint neighbourPoint = GetMapPointByID(mapPoints, neighbour);
                                if(neighbourPoint != null)
                                {
                                    Gizmos.DrawLine(mapPoints[i].point, neighbourPoint.point);

                                }
                            }
                        }
                    }
                }
            }

            //if (startPoint != null)
            //{
            //    Gizmos.color = Color.blue;
            //    Gizmos.DrawWireSphere(startPoint.point, displayPointRadius);
            //}

            //if (endPoint != null)
            //{
            //    Gizmos.color = Color.green;
            //    Gizmos.DrawWireSphere(endPoint.point, displayPointRadius);
            //}
        }

    }

    private void OnDestroy()
    {
        ClearLists();

        WorldMapMarker.OnWorldMapMarkerPressed -= MapMarkerFocused;
    }

    //PDS MAP GENREATION
    /// <summary>
    /// Methods for generating a map using PDS distrbuted points and random point setting
    /// </summary>

    void GeneratePDSMapRoutine()
    {
        mapValid = false;
        int loopCount = 0;

        while (mapValid == false)
        {
            ClearLists();

            GeneratePDSPoints();

            GetNeighbours(points, maxNeighoursPerMapPoint);

            RemoveSingleMapPoints(points);

            GetMapPoints(points);

            RemoveDuplicateMapPoints(mapPoints);

            FindNorthAndSouthPoints();

            mapValid = CheckMapIsValid();

            loopCount++;

            if (loopCount > 2000)
            {
                break;
            }
        }

       // Debug.Log("map generation attempts: " + loopCount);

        if (mapValid)
        {
            GetNeighbours(mapPoints);

            SetMapPointTypes();

            foreach (MapPoint mapPoint in mapPoints)
            {
                GameObject mapMarkerObject = Instantiate(worldMapMarkerObject, mapPoint.point + worldMapMarkerObjectParent.transform.position, Quaternion.identity, worldMapMarkerObjectParent);

                WorldMapMarker mapMarker = mapMarkerObject.GetComponent<WorldMapMarker>();
                mapMarker.mapPoint = mapPoint;

                if (mapMarker.mapPoint.mapPointType == MapPointType.START)
                {
                    startPointTransform = mapMarker.transform;
                }
                else if (mapMarker.mapPoint.mapPointType == MapPointType.END)
                {
                    endPointTransform = mapMarker.transform;
                }

                if (mapPoint.point == northPoint.point)
                {
                    northernMostPoint = mapMarker.transform;
                }

                if (mapPoint.point == southPoint.point)
                {
                    southernMostPoint = mapMarker.transform;
                }

                mapMarker.SetMapMarkerMaterial();
            }

            GeneratePaths(true);

            if (startPointTransform != null &&
                endPointTransform != null &&
                northernMostPoint != null &&
                southernMostPoint != null)
            {
                Transform[] targets = new Transform[4];
                targets[0] = startPointTransform;
                targets[1] = endPointTransform;
                targets[2] = northernMostPoint;
                targets[3] = southernMostPoint;

                float[] weights = new float[4];
                weights[0] = 1f;
                weights[1] = 1f;
                weights[2] = 1f;
                weights[3] = 1f;

                float[] radius = new float[4];
                radius[0] = 1f;
                radius[1] = 1f;
                radius[2] = 1f;
                radius[3] = 1f;

                SetTargetGroup(targets, weights, radius, startEndPointGroup);
            }

            FocusCameraOnWholeMap();

            playerCurrentMapPoint = startPoint;

            WorldMapMarker.OnWorldMapMarkerPressed -= MapMarkerFocused;
            WorldMapMarker.OnWorldMapMarkerPressed += MapMarkerFocused;

            PlacePlayerAtMapMarker(startPoint);

            if (OnGenerateMapSuccess != null)
            {
                OnGenerateMapSuccess.Invoke();
            }
        }
        else
        {
            //Debug.Log("map generation failed catastrophically, there is no game!");
        }

    }

    void GeneratePDSMapRoutineOnValidate()
    {
        mapValid = false;

        int loopCount = 0;
        while (mapValid == false)
        {
            ClearLists();

            GeneratePDSPoints();

            GetNeighbours(points, maxNeighoursPerMapPoint);

            RemoveSingleMapPoints(points);

            GetMapPoints(points);

            RemoveDuplicateMapPoints(mapPoints);

            mapValid = CheckMapIsValid();

            loopCount++;

            if (loopCount > 2000)
            {
                break;
            }
        }

        //Debug.Log("map generation attempts: " + loopCount);

        if (mapValid)
        {
            GetNeighbours(mapPoints);

            SetMapPointTypes();
        }
        else
        {
            //Debug.Log("map generation failed catastrophically, there is no game!");
        }
    }

    bool CheckMapIsValid()
    {
        bool Result = false;

        if (mapPoints.Contains(endPoint))
        {
            if (mapPoints.Count > minimumMapPoints &&
                mapPoints.Count < maximumMapPoints)
            {
                Result = true;
               // Debug.Log("map generation success");
            }
        }

        return Result;
    }

    void GeneratePDSPoints()
    {
        points = PDS.GeneratePoints(maxRadius, regionSize, rejectionSamples);
        
        for(int i = 0; i < points.Count; i++)
        {
            points[i].id = i;
        }
    }

    void GetNeighbours(List<MapPoint> pointsList, int maxNeighbours = 20)
    {
        foreach (MapPoint point in pointsList)
        {
            if (point.neighbours == null)
            {
                point.neighbours = new List<int>();
            }
            else
            {
                point.neighbours.Clear();
            }
        }

        for (int i = 0; i < pointsList.Count; i++)
        {
            MapPoint a = pointsList[i];

            foreach (MapPoint b in pointsList)
            {
                if (a.neighbours.Count == maxNeighbours)
                {
                    break;
                }
                else
                {
                    if (a.point != b.point)
                    {
                        float searchRadius = maxRadius * searchRadiusMultiplier;
                        if ((a.point - b.point).magnitude < searchRadius)
                        {
                            if (a.neighbours.Contains(b.id) == false)
                            {
                                a.neighbours.Add(b.id);
                                b.neighbours.Add(a.id);
                            }
                        }
                    }
                }

            }
        }
    }

    void GetMapPoints(List<MapPoint> points)
    {
        tempPoints.Clear();

        foreach (MapPoint point in points)
        {
            if (point.point.z < startPointBoundaryDistance)
            {
                tempPoints.Add(point);
            }
        }

        startPoint = tempPoints[Random.Range(0, tempPoints.Count - 1)];
        startPoint.mapPointType = MapPointType.START;
        tempPoints.Clear();

        foreach (MapPoint point in points)
        {
            if (point.point.z > regionSize.z - endPointBoundaryDistance)
            {
                tempPoints.Add(point);
            }
        }

        endPoint = tempPoints[Random.Range(0, tempPoints.Count - 1)];
        endPoint.mapPointType = MapPointType.END;
        tempPoints.Clear();

        mapPoints.Add(startPoint);

        foreach (int neighbour in startPoint.neighbours)
        {
            if (mapPoints.Contains(endPoint) == false)
            {
                MapPoint neighbourPoint = GetMapPointByID(points, neighbour);
                FollowNeighbour(points, startPoint, neighbourPoint, 1, (endPoint.point - startPoint.point).sqrMagnitude);
            }
        }

        if (mapPoints.Contains(endPoint) == false)
        {
            Debug.Log("failed map generation");
        }

    }

    void FollowNeighbour(List<MapPoint> points, MapPoint fromPoint, MapPoint neighbour, int stepsRemain, float currentDistanceToEndPoint)
    {
        if (neighbour == endPoint)
        {
            mapPoints.Add(neighbour);
            return;
        }

        if (stepsRemain <= 0)
        {
            foreach (int nextNeighbour in neighbour.neighbours)
            {
                MapPoint neighbourPoint = GetMapPointByID(points, nextNeighbour);
                if (neighbourPoint == endPoint)
                {
                    mapPoints.Add(neighbour);
                    FollowNeighbour(points, neighbour, neighbourPoint, 1, 0f);
                }
            }

            return;
        }

        if (mapPoints.Contains(neighbour) == false)
        {
            mapPoints.Add(neighbour);

            if (neighbour.neighbours.Count == 1)
            {
                return;
            }

            Vector3 endPointDirection = (endPoint.point - neighbour.point).normalized;
            Vector3 toNeighbourDirection = (neighbour.point - fromPoint.point).normalized;
            float distanceToEndPoint = currentDistanceToEndPoint;
            float newDistanceToEndPoint = (endPoint.point - neighbour.point).sqrMagnitude;

            if (newDistanceToEndPoint > currentDistanceToEndPoint)
            {
                stepsRemain--;
            }
            else
            {
                if (Vector3.Dot(endPointDirection, toNeighbourDirection) < 0.6f)
                {
                    stepsRemain--;
                }

                distanceToEndPoint = newDistanceToEndPoint;
            }

            foreach (int nextNeighbour in neighbour.neighbours)
            {
                MapPoint neighbourPoint = GetMapPointByID(points, nextNeighbour);
                FollowNeighbour(points, neighbour, neighbourPoint, stepsRemain, distanceToEndPoint);
            }

        }
    }

    MapPoint GetMapPointByID(List<MapPoint> points, int id)
    {
        MapPoint result = null;

        foreach(MapPoint point in points)
        {
            if(point.id == id)
            {
                result = point;
                break;
            }
        }

        return result;
    }

    void SetMapPointTypes()
    {
        tempPoints.Clear();

        int numEnemyPoints = Mathf.CeilToInt(mapPoints.Count * percentageEnemyPoints);

        foreach (MapPoint point in mapPoints)
        {
            if (point.mapPointType != MapPointType.START &&
               point.mapPointType != MapPointType.END)
            {
                tempPoints.Add(point);
            }
        }

        for (int i = 0; i < numEnemyPoints; i++)
        {
            MapPoint point = tempPoints[Random.Range(0, tempPoints.Count)];
            int pointIndex = mapPoints.IndexOf(point);
            mapPoints[pointIndex].mapPointType = MapPointType.BATTLE;

            tempPoints.Remove(point);
        }

        int numRandomPoints = Mathf.CeilToInt(tempPoints.Count * percentageRandomPoints);
        int numShopPoints = tempPoints.Count - numRandomPoints;

        for (int i = 0; i < numRandomPoints; i++)
        {
            MapPoint point = tempPoints[Random.Range(0, tempPoints.Count)];
            int pointIndex = mapPoints.IndexOf(point);
            mapPoints[pointIndex].mapPointType = MapPointType.RANDOM;

            tempPoints.Remove(point);
        }

        for (int i = 0; i < numShopPoints; i++)
        {
            MapPoint point = tempPoints[Random.Range(0, tempPoints.Count)];
            int pointIndex = mapPoints.IndexOf(point);
            mapPoints[pointIndex].mapPointType = MapPointType.SHOP;

            tempPoints.Remove(point);
        }

        foreach (int neighbour in endPoint.neighbours)
        {
            MapPoint neighbourPoint = GetMapPointByID(mapPoints, neighbour);
            neighbourPoint.mapPointType = MapPointType.BATTLE;
        }
    }

    void RemoveSingleMapPoints(List<MapPoint> pointsList)
    {
        tempPoints.Clear();

        for (int i = 0; i < pointsList.Count; i++)
        {
            if (pointsList[i].neighbours.Count == 0)
            {
                tempPoints.Add(pointsList[i]);
            }
        }

        foreach (MapPoint single in tempPoints)
        {
            pointsList.Remove(single);
        }
    }

    void RemoveDuplicateMapPoints(List<MapPoint> pointsList)
    {
        tempPoints.Clear();

        for (int i = 0; i < pointsList.Count; i++)
        {
            for (int j = 0; j < pointsList.Count; j++)
            {
                if (j != i)
                {
                    if (pointsList[j] == pointsList[i])
                    {
                        tempPoints.Add(pointsList[j]);
                    }
                }
            }
        }

        foreach(MapPoint point in tempPoints)
        {
            pointsList.Remove(point);
        }
    }

    void FindNorthAndSouthPoints()
    {
        northPoint = mapPoints[0];
        southPoint = mapPoints[1];

        foreach (MapPoint point in mapPoints)
        {
            if (point.point.x < northPoint.point.x)
            {
                northPoint = point;
            }
            if (point.point.x > southPoint.point.x)
            {
                southPoint = point;
            }
        }
    }

    void GenerateMapFromPoints(List<MapPoint> mapPoints)
    {
        FindNorthAndSouthPoints();

        foreach (MapPoint mapPoint in mapPoints)
        {
            GameObject mapMarkerObject = Instantiate(worldMapMarkerObject, mapPoint.point + worldMapMarkerObjectParent.transform.position, Quaternion.identity, worldMapMarkerObjectParent);

            WorldMapMarker mapMarker = mapMarkerObject.GetComponent<WorldMapMarker>();
            mapMarker.mapPoint = mapPoint;

            if (mapMarker.mapPoint.mapPointType == MapPointType.START)
            {
                startPointTransform = mapMarker.transform;
            }
            else if (mapMarker.mapPoint.mapPointType == MapPointType.END)
            {
                endPointTransform = mapMarker.transform;
            }

            if (mapPoint.point == northPoint.point)
            {
                northernMostPoint = mapMarker.transform;
            }

            if (mapPoint.point == southPoint.point)
            {
                southernMostPoint = mapMarker.transform;
            }


            mapMarker.SetMapMarkerMaterial();
        }

        GeneratePaths();

        if (startPointTransform != null &&
            endPointTransform != null &&
            northernMostPoint != null &&
            southernMostPoint != null)
        {
            Transform[] targets = new Transform[4];
            targets[0] = startPointTransform;
            targets[1] = endPointTransform;
            targets[2] = northernMostPoint;
            targets[3] = southernMostPoint;

            float[] weights = new float[4];
            weights[0] = 1f;
            weights[1] = 1f;
            weights[2] = 1f;
            weights[3] = 1f;

            float[] radius = new float[4];
            radius[0] = 1f;
            radius[1] = 1f;
            radius[2] = 1f;
            radius[3] = 1f;

            SetTargetGroup(targets, weights, radius, startEndPointGroup);
        }

        FocusCameraOnWholeMap();

        WorldMapMarker.OnWorldMapMarkerPressed -= MapMarkerFocused;
        WorldMapMarker.OnWorldMapMarkerPressed += MapMarkerFocused;
    }

    void DefineMapPoint(MapPoint mapPoint)
    {
        if(gameManager != null)
        {
            switch (mapPoint.mapPointType)
            {
                case MapPointType.BATTLE:
                case MapPointType.ELITEBATTLE:
                case MapPointType.MINIBOSS:
                    {
                        gameManager.SetEnemyEncounter(mapPoint, this);
                    }
                    break;
                case MapPointType.RANDOM:
                    {

                    }
                    break;
                case MapPointType.TREASURE:
                    {

                    }
                    break;
                default:
                    break;
            }
        }

    }

    public void StopMusic()
    {
        if(mapMusic != null)
        {
            mapMusic.Stop();
        }
    }


    public void SaveGame()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/save.data";
        FileStream stream = new FileStream(path, FileMode.Create);

        int currentPlayerCharacterIndex = gameManager.currentPlayerCharacterIndex;
        int currentMapPointIndex = mapPoints.IndexOf(playerCurrentMapPoint);

        List<MapPointSerializable> serializedMapPoints = new List<MapPointSerializable>();
        for (int i = 0; i < mapPoints.Count; i++)
        {
            serializedMapPoints.Add(new MapPointSerializable(mapPoints[i]));
        }

        SaveFile saveData = new SaveFile(currentPlayerCharacterIndex, currentMapPointIndex, serializedMapPoints);
        formatter.Serialize(stream, saveData);

        stream.Close();

        Debug.Log("saving map data to " + path);

        if(OnSaveGame != null)
        {
            OnSaveGame.Invoke();
        }
    }

    public void LoadGame()
    {
        if(gameManager != null)
        {
            gameManager.LoadGame();
        }
    }

    public void LoadLevel()
    {
        if (levelLoader != null)
        {
            levelLoader.SetSceneIndex(1);
            levelLoader.LoadNextLevel();
        }
    }

    //public void SaveGame()
    //{
    //    if(gameManager != null)
    //    {
    //        gameManager.SaveGame();
    //    }
    //}

    //void GenerateTerrainPaths()
    //{
    //    startTerrainHeights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
    //    setHeights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

    //    Vector3 raisePosition = new Vector3(0, raiseTerrainHeight, 0);
    //    terrainEdges.transform.position = raisePosition;

    //    for (int x = 0; x < terrainData.heightmapResolution; x++)
    //    {
    //        for (int y = 0; y < terrainData.heightmapResolution; y++)
    //        {
    //            setHeights[x, y] = raiseTerrainHeight / terrainData.size.y;
    //        }
    //    }

    //    terrainData.SetHeights(0, 0, setHeights);


    //    //foreach (Transform worldMapPoint in worldMapPoints)
    //    //{
    //    //    Vector3 position = ConvertWorldPositionToTerrainPosition(worldMapPoint.position);
    //    //    Debug.Log(worldMapPoint.position + " " + position);

    //    //    setHeights[(int)position.z, (int)position.x] = flattenTerrainHeight / terrainData.size.y;
    //    //}

    //    //terrainData.SetHeights(0, 0, setHeights);

    //    //foreach (Transform worldMapPoint in worldMapPoints)
    //    //{
    //    //    Vector3 position = ConvertWorldPositionToTerrainPosition(worldMapPoint.position);
    //    //    Debug.Log(worldMapPoint.position + " " + position);

    //    //    float[,] terrainAreaTest = new float[samplePositionArea, samplePositionArea];
    //    //    int offset = samplePositionArea / 2;

    //    //    for (int x = 0; x < samplePositionArea; x++)
    //    //    {
    //    //        for (int z = 0; z < samplePositionArea; z++)
    //    //        {
    //    //            terrainAreaTest[x, z] = flattenTerrainHeight / terrainData.size.y;

    //    //        }
    //    //    }

    //    //    terrainData.SetHeights((int)Mathf.Clamp(position.x - offset, 0, terrainData.heightmapResolution), 
    //    //                           (int)Mathf.Clamp(position.z - offset, 0, terrainData.heightmapResolution), terrainAreaTest);
    //    //}

    //    if (pathCreator != null)
    //    {
    //        if (mapPoints != null)
    //        {
    //            foreach (MapPoint mapPoint in mapPoints)
    //            {
    //                if (mapPoint.neighbours != null)
    //                {
    //                    foreach (MapPoint neighbour in mapPoint.neighbours)
    //                    {
    //                        Vector3[] bezierPathPoints = new Vector3[5];

    //                        bezierPathPoints[0] = mapPoint.point;
    //                        bezierPathPoints[1] = mapPoint.point + ((neighbour.point - mapPoint.point) * 0.25f + Random.onUnitSphere * pathDeviationRadius);
    //                        bezierPathPoints[2] = mapPoint.point + ((neighbour.point - mapPoint.point) * 0.5f + Random.onUnitSphere * pathDeviationRadius);
    //                        bezierPathPoints[3] = mapPoint.point + ((neighbour.point - mapPoint.point) * 0.75f + Random.onUnitSphere * pathDeviationRadius);
    //                        bezierPathPoints[4] = neighbour.point;

    //                        BezierPath bezierPath = new BezierPath(bezierPathPoints);
    //                        bezierPath.ControlPointMode = BezierPath.ControlMode.Automatic;
    //                        bezierPath.GlobalNormalsAngle = 90f;
    //                        pathCreator.bezierPath = bezierPath;

    //                        VertexPath vertexPath = new VertexPath(bezierPath, pathCreator.transform);
    //                        sampleDistanceIncrement = vertexPath.length / pathSampleAmount;

    //                        vertexPath = pathCreator.EditorData.GetVertexPath(pathCreator.transform);

    //                        for (int i = 0; i < pathSampleAmount; i++)
    //                        {
    //                            samplePathPosition = ConvertWorldPositionToTerrainPosition(vertexPath.GetPointAtDistance(i * sampleDistanceIncrement));

    //                            int offset = samplePositionArea / 2;
    //                            float[,] terrainArea = new float[samplePositionArea, samplePositionArea];

    //                            for (int x = 0; x < samplePositionArea; x++)
    //                            {
    //                                for (int z = 0; z < samplePositionArea; z++)
    //                                {
    //                                    terrainArea[x, z] = flattenTerrainHeight / terrainData.size.y;
    //                                }
    //                            }

    //                            terrainData.SetHeights(Mathf.Clamp((int)samplePathPosition.x - offset, 0, terrainData.heightmapResolution - samplePositionArea),
    //                                                    Mathf.Clamp((int)samplePathPosition.z - offset, 0, terrainData.heightmapResolution - samplePositionArea), terrainArea);
    //                        }
    //                }
    //            }
    //        }
    //        }
    //    }
    //}

    //Vector3 ConvertWorldPositionToTerrainPosition(Vector3 worldPosition, bool localSpace = true)
    //{
    //    Vector3 Result = Vector3.zero;

    //    Result = worldPosition;

    //    if(localSpace)
    //    {
    //        Result -= terrain.GetPosition();
    //    }

    //    Result.x /= terrainData.size.x;
    //    Result.y /= terrainData.size.y;
    //    Result.z /= terrainData.size.z;

    //    Result = Result * terrainData.heightmapResolution;

    //    Result.x = Mathf.CeilToInt(Result.x);
    //    Result.y = Mathf.CeilToInt(Result.y);
    //    Result.z = Mathf.CeilToInt(Result.z);

    //    return Result;
    //}

    //private void OnDestroy()
    //{

    //if(terrainData != null)
    //{
    //    if(startTerrainHeights != null)
    //    {
    //        terrainData.SetHeights(0, 0, startTerrainHeights);
    //    }
    //}
    //}
}
