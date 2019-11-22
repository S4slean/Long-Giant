using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldGenerationManager 
{
    [Header("Prefabs")]
    [SerializeField] GiantConstructionScript giantConstructionPrefab = default;
    [SerializeField] List<WorldElementPrefabWithProbability> allPrefabs = new List<WorldElementPrefabWithProbability>();
    [SerializeField] List<WorldElementPrefabWithProbability> fillingPrefabs = new List<WorldElementPrefabWithProbability>();
    [SerializeField] float worldElementsSize = 1.2f;
    [SerializeField] float worldElementsSpacing = 0.5f;
    [SerializeField] float randomPositionAmplitude = 0.25f;
    [SerializeField] float minimumDistanceWithCenter = 4f;
    [SerializeField] float worldFillingAmount = 0.25f;
    [SerializeField] int maximumNumberOfWorldElements = 25;
    [SerializeField] WorldElementPrefab phPrefab = default;
    [SerializeField] List<ResourceWithWeight> resourcesWithWeight = new List<ResourceWithWeight>();
    Dictionary<ResourceType, float> allResourcesWeights = new Dictionary<ResourceType, float>();

    Dictionary<ResourceType, int> allResourcesQuantities = new Dictionary<ResourceType, int>();

    public void GenerateWorld(Vector3 centerPosition, float radius)
    {
        totalProbability = GetTotalProbability();
        totalFillProbability = GetTotalFillObjectsProbability();

        allResourcesWeights = new Dictionary<ResourceType, float>();
        foreach (ResourceWithWeight resource in resourcesWithWeight)
        {
            if (!allResourcesWeights.ContainsKey(resource.resourceType))
                allResourcesWeights.Add(resource.resourceType, resource.weight);
        }

        allResourcesQuantities = new Dictionary<ResourceType, int>();

        GiantConstructionScript giantConstruction = Object.Instantiate(giantConstructionPrefab, centerPosition, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
        giantConstruction.transform.parent = GameManager.gameManager.GetAllGameObjectsParent;

        List<Vector3> allPossiblePositions = CirclePositionsGenerator.GetAllPositionsInCircle(worldElementsSize, worldElementsSpacing, radius, minimumDistanceWithCenter, (int)((float)maximumNumberOfWorldElements / worldFillingAmount));
        int numberOfElementsInWorld = Mathf.Clamp(Mathf.RoundToInt(allPossiblePositions.Count * worldFillingAmount), 0, maximumNumberOfWorldElements);

        if (allPossiblePositions.Count == 0)
            return;

        WorldElementPrefab pickedPrefab = null;
        WorldElementPrefab newObject = null;
        int pickedRandomInt = 0;
        Vector3 randomOffset = Vector3.zero;

        GameManager.gameManager.gameAreaRadius = Vector3.Distance(centerPosition, allPossiblePositions[allPossiblePositions.Count - 1]) + worldElementsSize;

        for (int i = 0; i < numberOfElementsInWorld; i++)
        {
            pickedPrefab = GetRandomElementPrefab();

            if (pickedPrefab == null)
                continue;

            randomOffset = Random.onUnitSphere;
            randomOffset.y = 0;
            randomOffset.Normalize();

            pickedRandomInt = Random.Range(0, allPossiblePositions.Count);

            newObject = Object.Instantiate(pickedPrefab, centerPosition + allPossiblePositions[pickedRandomInt] + randomOffset * Random.Range(0, randomPositionAmplitude), Quaternion.Euler(0, Random.Range(0f, 360f), 0));
            newObject.transform.parent = GameManager.gameManager.GetAllGameObjectsParent;
            newObject.SetUpAllChildrenElements();
            allPossiblePositions.RemoveAt(pickedRandomInt);

            foreach (PhysicalObjectConstructionScript construction in newObject.GetAllElements)
                AddResourcesToTotal(construction);
        }

        foreach (Vector3 fillingPos in allPossiblePositions)
        {
            pickedPrefab = GetRandomFillElementPrefab();

            if (pickedPrefab == null)
                continue;

            randomOffset = Random.onUnitSphere;
            randomOffset.y = 0;
            randomOffset.Normalize();

            newObject = Object.Instantiate(pickedPrefab, centerPosition + fillingPos + randomOffset * Random.Range(0, randomPositionAmplitude), Quaternion.Euler(0, Random.Range(0f, 360f), 0));
            newObject.transform.parent = GameManager.gameManager.GetAllGameObjectsParent;
        }

        giantConstruction.GenerateNeededResourcesDictionary(GenerateObjective());

        GameManager.gameManager.SetGiantConstruction(giantConstruction);
        //DebugAllResourcesQuantities();
    }

    public void AddResourcesToTotal(PhysicalObjectConstructionScript construction)
    {
        List<ResourceWithQuantity> allResourcesSpawnedOnDestroy = construction.GetAllResourcesSpawnedOnDestroy;

        foreach (ResourceWithQuantity resource in allResourcesSpawnedOnDestroy)
        {
            if (allResourcesQuantities.ContainsKey(resource.resourceType))
                allResourcesQuantities[resource.resourceType] += resource.quantity;
            else
                allResourcesQuantities.Add(resource.resourceType, resource.quantity);
        }
    }

    public void DebugAllResourcesQuantities()
    {
        foreach (ResourceType resource in allResourcesQuantities.Keys)
            Debug.Log(resource + " : " + allResourcesQuantities[resource]);
    }

    public Dictionary<ResourceType, int> GenerateObjective()
    {
        Dictionary<ResourceType, int> neededResources = new Dictionary<ResourceType, int>();

        foreach (ResourceType resource in allResourcesQuantities.Keys)
        {
            if (allResourcesWeights.ContainsKey(resource))
                neededResources.Add(resource, Mathf.RoundToInt(allResourcesQuantities[resource] * allResourcesWeights[resource]));
        }

        return neededResources;
    }

    public int GetTotalProbability()
    {
        int totalProba = 0;

        foreach (WorldElementPrefabWithProbability elementWithProba in allPrefabs)
            totalProba += elementWithProba.probability;

        return totalProba;
    }
    int totalProbability = 0;

    public WorldElementPrefab GetRandomElementPrefab()
    {
        WorldElementPrefab pickedPrefab = null;

        int randomProbability = Random.Range(0, totalProbability);

        int probaCounter = 0;

        foreach (WorldElementPrefabWithProbability elementWithProba in allPrefabs)
        {
            probaCounter += elementWithProba.probability;
            if (randomProbability < probaCounter)
            {
                pickedPrefab = elementWithProba.prefab;
                break;
            }
        }

        return pickedPrefab;
    }

    public int GetTotalFillObjectsProbability()
    {
        int totalProba = 0;

        foreach (WorldElementPrefabWithProbability elementWithProba in fillingPrefabs)
            totalProba += elementWithProba.probability;

        return totalProba;
    }
    int totalFillProbability = 0;

    public WorldElementPrefab GetRandomFillElementPrefab()
    {
        WorldElementPrefab pickedPrefab = null;

        int randomProbability = Random.Range(0, totalFillProbability);

        int probaCounter = 0;

        foreach (WorldElementPrefabWithProbability elementWithProba in fillingPrefabs)
        {
            probaCounter += elementWithProba.probability;
            if (randomProbability < probaCounter)
            {
                pickedPrefab = elementWithProba.prefab;
                break;
            }
        }

        return pickedPrefab;
    }

    #region V1
    /*[Header("Prefabs")]
    [SerializeField] GiantConstructionScript giantConstructionPrefab = default;
    [SerializeField] List<WorldElementPrefabWithProbability> allPrefabs = new List<WorldElementPrefabWithProbability>();
    [SerializeField] float worldElementsSize = 1.2f;
    [SerializeField] float worldElementsSpacing = 0.5f;
    [SerializeField] float randomPositionAmplitude = 0.25f;
    [SerializeField] float minimumDistanceWithCenter = 4f;
    [SerializeField] float worldFillingAmount = 0.25f;
    [SerializeField] int maximumNumberOfWorldElements = 25;
    [SerializeField] PhysicalObjectConstructionScript phPrefab = default;
    [SerializeField] List<ResourceWithWeight> resourcesWithWeight = new List<ResourceWithWeight>();
    Dictionary<ResourceType, float> allResourcesWeights = new Dictionary<ResourceType, float>();

    Dictionary<ResourceType, int> allResourcesQuantities = new Dictionary<ResourceType, int>();

    public void GenerateWorld(Vector3 centerPosition, float radius)
    {
        allResourcesWeights = new Dictionary<ResourceType, float>();
        foreach(ResourceWithWeight resource in resourcesWithWeight)
        {
            if (!allResourcesWeights.ContainsKey(resource.resourceType))
                allResourcesWeights.Add(resource.resourceType, resource.weight);
        }

        allResourcesQuantities = new Dictionary<ResourceType, int>();

        GiantConstructionScript giantConstruction = Object.Instantiate(giantConstructionPrefab, centerPosition, Quaternion.Euler(0, Random.Range(0f, 360f), 0));

        List<Vector3> allPossiblePositions = CirclePositionsGenerator.GetAllPositionsInCircle(worldElementsSize, worldElementsSpacing, radius, minimumDistanceWithCenter, (int)((float)maximumNumberOfWorldElements/ worldFillingAmount));
        int numberOfElementsInWorld = Mathf.Clamp(Mathf.RoundToInt(allPossiblePositions.Count * worldFillingAmount), 0, maximumNumberOfWorldElements);

        Debug.Log(numberOfElementsInWorld);

        PhysicalObjectConstructionScript newObject = null;
        int pickedRandomInt = 0;
        Vector3 randomOffset = Vector3.zero;
        for (int i = 0; i < numberOfElementsInWorld; i++)
        {
            randomOffset = Random.onUnitSphere;
            randomOffset.y = 0;
            randomOffset.Normalize();

            pickedRandomInt = Random.Range(0, allPossiblePositions.Count);

            newObject = Object.Instantiate(phPrefab, centerPosition + allPossiblePositions[pickedRandomInt] + Vector3.up * 0.75f + randomOffset * Random.Range(0, randomPositionAmplitude), Quaternion.Euler(0, Random.Range(0f, 360f), 0));
            newObject.SetUp();
            allPossiblePositions.RemoveAt(pickedRandomInt);

            AddResourcesToTotal(newObject);
        }

        giantConstruction.GenerateNeededResourcesDictionary(GenerateObjective());
        DebugAllResourcesQuantities();
    }

    public void AddResourcesToTotal(PhysicalObjectConstructionScript construction)
    {
        List<ResourceWithQuantity> allResourcesSpawnedOnDestroy = construction.GetAllResourcesSpawnedOnDestroy;

        foreach(ResourceWithQuantity resource in allResourcesSpawnedOnDestroy)
        {
            if (allResourcesQuantities.ContainsKey(resource.resourceType))
                allResourcesQuantities[resource.resourceType] += resource.quantity;
            else
                allResourcesQuantities.Add(resource.resourceType, resource.quantity);
        }
    }

    public void DebugAllResourcesQuantities()
    {
        foreach (ResourceType resource in allResourcesQuantities.Keys)
            Debug.Log(resource + " : " + allResourcesQuantities[resource]);
    }

    public Dictionary<ResourceType, int> GenerateObjective()
    {
        Dictionary<ResourceType, int> neededResources = new Dictionary<ResourceType, int>();

        foreach (ResourceType resource in allResourcesQuantities.Keys)
        {
            if (allResourcesWeights.ContainsKey(resource))
                neededResources.Add(resource, Mathf.RoundToInt(allResourcesQuantities[resource] * allResourcesWeights[resource]));
        }

        return neededResources;
         //GenerateNeededResourcesDictionary
    }*/
    #endregion
}

[System.Serializable]
public struct WorldElementPrefabWithProbability
{
    public WorldElementPrefab prefab;
    public int probability;
}

[System.Serializable]
public struct ResourceWithWeight
{
    public ResourceType resourceType;
    public float weight;
}