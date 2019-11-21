using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldGenerationManager 
{
    [Header("Prefabs")]
    [SerializeField] GiantConstructionScript giantConstructionPrefab = default;
    [SerializeField] List<WorldElementPrefabWithProbability> allPrefabs = new List<WorldElementPrefabWithProbability>();
    [SerializeField] float worldElementsSize = 1.2f;
    [SerializeField] float worldElementsSpacing = 0.5f;

    public void GenerateWorld(Vector3 centerPosition, float radius)
    {
        //Vector3 baseAllPositions = 
        //CirclePositionsGenerator
    }
}

[System.Serializable]
public struct WorldElementPrefabWithProbability
{
    public PhysicalObjectConstructionScript prefab;
    public int probability;
}