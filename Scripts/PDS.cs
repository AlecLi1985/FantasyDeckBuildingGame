using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum MapPointType
{
    NONE,
    BATTLE,
    ELITEBATTLE,
    TREASURE,
    MINIBOSS,
    SHOP,
    RANDOM,
    SAVE,
    START,
    END
}

public enum RandomEventType
{
    NONE,
    GIVECARD,
    GIVEBUFF,
    GIVEDEBUFF,
    GIVEGOLD,
    EXCHANGECARD,
    REMOVERANDOMCARD,
    REMOVECARD,
    REMOVEGOLD,
    REMOVEHEALTH
}

[Serializable]
public class MapPoint
{
    public Vector3 point;
    public float radius;

    public MapPointType mapPointType;

    public int mapPointCombatSceneIndex;
    public int mapPointCombatSceneArrayIndex;
    public string title;
    public string description;

    public int enemySpawnCount;
    //min and max index range in the enemy characters list to spawn
    public int enemyCharacterRangeMin;
    public int enemyCharacterRangeMax;
    public RandomEventType eventType;

    public List<int> neighbours;
    public bool allowMultipleIncomingConnection = false;
    public bool allowMultipleOutgoingConnection = false;

    public int incomingConnections = 0;
    public int outgoingConnections = 0;

    public int id;
    public int stepCount;

    public bool Equals(MapPoint a)
    {
        bool pointEquals = point == a.point;
        bool idEquals = id == a.id;
        bool mapPointTypeEquals = mapPointType == a.mapPointType;
        bool neighboursEquals = true;

        if(neighbours.Count > 0)
        {
            foreach (int neighbour in neighbours)
            {
                foreach (int otherNeighbour in a.neighbours)
                {
                    if (neighbour != otherNeighbour)
                    {
                        neighboursEquals = false;
                        break;
                    }
                }
            }
        }

        return idEquals && pointEquals && mapPointTypeEquals && neighboursEquals; 
    }
}


public static class PDS
{
    public static List<MapPoint> GeneratePoints(float radius, Vector3 sampleRegionSize, int numSamplesBeforeRejection = 30)
    {
        float cellSize = radius / Mathf.Sqrt(2);

        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.z / cellSize)];
        List<MapPoint> points = new List<MapPoint>();
        List<Vector3> spawnPoints = new List<Vector3>();

        spawnPoints.Add(sampleRegionSize / 2);
        while (spawnPoints.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector3 spawnCentre = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector3 dir = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                MapPoint candidatePoint = new MapPoint();
                float randomRadius = Random.Range(radius, 2 * radius);
                candidatePoint.point = spawnCentre + dir * randomRadius;
                candidatePoint.radius = randomRadius;

                if (IsValid(candidatePoint, sampleRegionSize, cellSize, radius, points, grid))
                {
                    points.Add(candidatePoint);
                    spawnPoints.Add(candidatePoint.point);
                    grid[(int)(candidatePoint.point.x / cellSize), (int)(candidatePoint.point.z / cellSize)] = points.Count; //index of the point in the grid cell, 1-based, not zero-based
                    candidateAccepted = true;
                    break;
                }
            }

            if (!candidateAccepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }

        return points;
    }

    static bool IsValid(MapPoint candidate, Vector3 sampleRegionSize, float cellSize, float radius, List<MapPoint> points, int[,] grid)
    {
        if (candidate.point.x >= 0 && candidate.point.x < sampleRegionSize.x && candidate.point.z >= 0 && candidate.point.z < sampleRegionSize.z)
        {
            int cellX = (int)(candidate.point.x / cellSize);
            int cellZ = (int)(candidate.point.z / cellSize);

            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
            int searchStartZ = Mathf.Max(0, cellZ - 2);
            int searchEndZ = Mathf.Min(cellZ + 2, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int z = searchStartZ; z <= searchEndZ; z++)
                {
                    int pointIndex = grid[x, z] - 1; //convert 1-based index to zero-based index
                    if (pointIndex != -1)
                    {
                        float sqrDistance = (candidate.point - points[pointIndex].point).sqrMagnitude;
                        if (sqrDistance < radius * radius)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
        return false;
    }
}



//public static class PDS
//{
//    public static List<PDSPoint> GeneratePoints(float minRadius, float maxRadius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30)
//    {
//        int maxLoopCount = 100;
//        int currentLoopCount = 0;

//        float cellSize = maxRadius / Mathf.Sqrt(2);

//        List<int>[,] grid = new List<int>[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
//        for (int x = 0; x < Mathf.CeilToInt(sampleRegionSize.x / cellSize); x++)
//        {
//            for (int y = 0; y < Mathf.CeilToInt(sampleRegionSize.y / cellSize); y++)
//            {
//                grid[x, y] = new List<int>();
//            }
//        }
//        List<PDSPoint> points = new List<PDSPoint>();
//        List<PDSPoint> spawnPoints = new List<PDSPoint>();

//        PDSPoint pdsPoint = new PDSPoint();
//        pdsPoint.point = sampleRegionSize / 2;
//        pdsPoint.radius = Random.Range(minRadius, maxRadius);

//        spawnPoints.Add(pdsPoint);
//        while (spawnPoints.Count > 0)
//        {
//            int spawnIndex = Random.Range(0, spawnPoints.Count);

//            PDSPoint spawnPoint = spawnPoints[spawnIndex];
//            Vector2 spawnCentre = spawnPoint.point;
//            float spawnRadius = Random.Range(pdsPoint.radius, 2 * pdsPoint.radius);
//            bool candidateAccepted = false;

//            for (int i = 0; i < numSamplesBeforeRejection; i++)
//            {
//                float angle = Random.value * Mathf.PI * 2;
//                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));

//                PDSPoint candidatePoint = new PDSPoint();
//                candidatePoint.point = spawnCentre + dir * spawnRadius;
//                float randomRadius = Random.Range(minRadius, maxRadius);
//                candidatePoint.radius = Random.Range(randomRadius, randomRadius * 2);

//                if (IsValid(candidatePoint.point, sampleRegionSize, cellSize, candidatePoint.radius, points, grid))
//                {
//                    points.Add(candidatePoint);
//                    spawnPoints.Add(candidatePoint);

//                    grid[(int)(candidatePoint.point.x / cellSize), (int)(candidatePoint.point.y / cellSize)].Add(points.Count); //index of the point in the grid cell, 1-based, not zero-based

//                    candidateAccepted = true;

//                    currentLoopCount++;

//                    break;
//                }
//            }

//            if (!candidateAccepted)
//            {
//                spawnPoints.RemoveAt(spawnIndex);
//                currentLoopCount++;
//            }
//        }

//        return points;
//    }

//    static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<PDSPoint> points, List<int>[,] grid)
//    {
//        if (candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y)
//        {
//            int cellX = (int)(candidate.x / cellSize);
//            int cellY = (int)(candidate.y / cellSize);

//            int searchStartX = Mathf.Max(0, cellX - 2);
//            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
//            int searchStartY = Mathf.Max(0, cellY - 2);
//            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

//            for (int x = searchStartX; x <= searchEndX; x++)
//            {
//                for (int y = searchStartY; y <= searchEndY; y++)
//                {
//                    foreach (int index in grid[x, y])
//                    {
//                        int pointIndex = index - 1;
//                        if (pointIndex != -1)
//                        {
//                            float sqrDistance = (candidate - points[pointIndex].point).sqrMagnitude;
//                            if (sqrDistance < radius * radius)
//                            {
//                                return false;
//                            }
//                        }
//                    }
//                    //int pointIndex = grid[x, y] - 1; //convert 1-based index to zero-based index
//                    //if (pointIndex != -1)
//                    //{
//                    //    float sqrDistance = (candidate - points[pointIndex]).sqrMagnitude;
//                    //    if (sqrDistance < radius * radius)
//                    //    {
//                    //        return false;
//                    //    }
//                    //}
//                }
//            }

//            return true;
//        }
//        return false;
//    }
//}
