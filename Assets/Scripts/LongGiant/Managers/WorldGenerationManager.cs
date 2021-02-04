using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager responsible for the procedural generation of the world, and the processing of the generated Resources.
/// </summary>
[System.Serializable]
public class WorldGenerationManager 
{
    [Header("Prefabs")]
    [SerializeField] GiantConstructionScript giantConstructionPrefab = default;
    [SerializeField] List<WorldElementPrefabWithProbability> mainPrefabs = new List<WorldElementPrefabWithProbability>();
    [SerializeField] List<WorldElementPrefabWithProbability> fillingPrefabs = new List<WorldElementPrefabWithProbability>();
    [SerializeField] float worldElementsSize = 1.2f;
    [SerializeField] float worldElementsSpacing = 0.5f;
    [SerializeField] float randomPositionAmplitude = 0.25f;
    [SerializeField] float minimumDistanceWithCenter = 4f;
    [SerializeField] float worldFillingAmount = 0.25f;
    [SerializeField] int maximumNumberOfMainWorldElements = 25;
    [SerializeField] WorldElementPrefab phPrefab = default;
    [SerializeField] List<ResourceWithWeight> resourcesWithWeight = new List<ResourceWithWeight>();
    Dictionary<ResourceType, float> allResourcesWeights = new Dictionary<ResourceType, float>();

    /// <summary>
    /// The total amount of each resource generated through the whole world
    /// </summary>
    Dictionary<ResourceType, int> allResourcesQuantities = new Dictionary<ResourceType, int>();

    int totalMainPrefabsProbability = 0;
    int totalFillPrefabsProbability = 0;

    #region World Generation

    /// <summary>
    /// Generates the world, by generating positions inside of the circle game area (depening on the centerPosition and radius), and spawning random WorldElementPrefab on thosr positions.
    /// Processes all generated Constructions to get the total amount of each Resource Type.
    /// This places a certain amount of Main Prefabs, and then fills all the remaining positions with "Filling Prefabs" to avoid empty spaces.
    /// </summary>
    /// <param name="centerPosition"></param>
    /// <param name="radius"></param>
    public void GenerateWorld(Vector3 centerPosition, float radius)
    {
        //Initialize probabilities
        totalMainPrefabsProbability = GetTotalMainPrefabsProbability();
        totalFillPrefabsProbability = GetTotalFillPrefabsProbability();

        //Initialize Resources Weights
        allResourcesWeights = new Dictionary<ResourceType, float>();
        foreach (ResourceWithWeight resource in resourcesWithWeight)
        {
            if (!allResourcesWeights.ContainsKey(resource.resourceType))
                allResourcesWeights.Add(resource.resourceType, resource.weight);
        }

        allResourcesQuantities = new Dictionary<ResourceType, int>();

        //Spawns the Giant Construction
        GiantConstructionScript giantConstruction = Object.Instantiate(giantConstructionPrefab, centerPosition, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
        giantConstruction.transform.parent = GameManager.gameManager.GetAllGameObjectsParent;

        //Generates positions inside the circle game area and defines the number of main prefabs
        List<Vector3> allPossiblePositions = CirclePositionsGenerator.GetAllPositionsInCircle(worldElementsSize, worldElementsSpacing, radius, minimumDistanceWithCenter, (int)((float)maximumNumberOfMainWorldElements / worldFillingAmount));
        int numberOfMainElementsInWorld = Mathf.Clamp(Mathf.RoundToInt(allPossiblePositions.Count * worldFillingAmount), 0, maximumNumberOfMainWorldElements);

        if (allPossiblePositions.Count == 0)
            return;

        WorldElementPrefab pickedPrefab = null;
        WorldElementPrefab newObject = null;
        int pickedRandomInt = 0;
        Vector3 randomOffset = Vector3.zero;

        GameManager.gameManager.gameAreaRadius = Vector3.Distance(centerPosition, allPossiblePositions[allPossiblePositions.Count - 1]) + worldElementsSize;

        //Spawns as many Main Prefabs as needed
        for (int i = 0; i < numberOfMainElementsInWorld; i++)
        {
            //Pick a randoom prefab
            pickedPrefab = GetRandomMainElementPrefab();

            if (pickedPrefab == null)
                continue;

            //Generates a random offset to randomize a little the prefabs locations
            randomOffset = Random.onUnitSphere;
            randomOffset.y = 0;
            randomOffset.Normalize();

            //Pick a position index
            pickedRandomInt = Random.Range(0, allPossiblePositions.Count);

            //Instantiate the Prefab and set it up
            newObject = Object.Instantiate(pickedPrefab, centerPosition + allPossiblePositions[pickedRandomInt] + randomOffset * Random.Range(0, randomPositionAmplitude), Quaternion.Euler(0, Random.Range(0f, 360f), 0));
            newObject.transform.parent = GameManager.gameManager.GetAllGameObjectsParent;
            newObject.SetUpAllChildrenElements();
            allPossiblePositions.RemoveAt(pickedRandomInt);

            //Check each of the children Constructions to process total amount of generated Resources
            foreach (PhysicalObjectConstructionScript construction in newObject.GetAllElements)
                AddResourcesToTotal(construction);
        }

        //Fill remaining positions with Filling prefabs
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

        //Generate the goal on the Giant Construction
        giantConstruction.GenerateNeededResourcesDictionary(GenerateObjective());

        GameManager.gameManager.SetGiantConstruction(giantConstruction);
    }

        
    /// <summary>
    /// Adds all the Resources held by a Construction inside of the total world Resources 
    /// </summary>
    /// <param name="construction"></param>
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
    

    /// <summary>
    /// Generates an objective with amounts of Resources to gather depening on the total amount of each Resource generated and on the weight of each Resource type
    /// </summary>
    /// <returns></returns>
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

    #endregion


    #region Picking Prefabs Randomly

    /// <summary>
    /// Returns the total probabilities for each Main Element's prefab to spawn
    /// </summary>
    /// <returns></returns>
    public int GetTotalMainPrefabsProbability()
    {
        int totalProba = 0;

        foreach (WorldElementPrefabWithProbability elementWithProba in mainPrefabs)
            totalProba += elementWithProba.probability;

        return totalProba;
    }

    /// <summary>
    /// Returns a random Main Element prefab based on Main Elements' prefabs' probabilities
    /// </summary>
    /// <returns></returns>
    public WorldElementPrefab GetRandomMainElementPrefab()
    {
        WorldElementPrefab pickedPrefab = null;

        int randomProbability = Random.Range(0, totalMainPrefabsProbability);

        int probaCounter = 0;

        foreach (WorldElementPrefabWithProbability elementWithProba in mainPrefabs)
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


    /// <summary>
    /// Returns the total probabilities for each Filling Element's prefab to spawn
    /// </summary>
    /// <returns></returns>
    public int GetTotalFillPrefabsProbability()
    {
        int totalProba = 0;

        foreach (WorldElementPrefabWithProbability elementWithProba in fillingPrefabs)
            totalProba += elementWithProba.probability;

        return totalProba;
    }

    /// <summary>
    /// Returns a random Fill Element prefab based on Fill Elements' prefabs' probabilities
    /// </summary>
    /// <returns></returns>
    public WorldElementPrefab GetRandomFillElementPrefab()
    {
        WorldElementPrefab pickedPrefab = null;

        int randomProbability = Random.Range(0, totalFillPrefabsProbability);

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

    #endregion
}

/// <summary>
/// Binds a WorldElementPrefab with a probability weight.
/// </summary>
[System.Serializable]
public struct WorldElementPrefabWithProbability
{
    public WorldElementPrefab prefab;
    public int probability;
}

/// <summary>
/// Binds a Resource with a weight - Used to definr which Resources are required in higher proportions.
/// </summary>
[System.Serializable]
public struct ResourceWithWeight
{
    public ResourceType resourceType;
    public float weight;
}