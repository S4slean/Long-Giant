using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A manager responsible for spawning Humans at a certain rate. Holds the Humans' prefabs with their respective probabilities.
/// </summary>
[System.Serializable]
public class HumanSpawningManager
{
    [Header("Humans Prefabs")]
    [SerializeField] List<HumanPrefabWithProbability> allHumanPrefabsWithSpawnProbability = new List<HumanPrefabWithProbability>();
    int totalProbability = 0;
    [SerializeField] public int maximumNumberOfHumans = 20;
    public int currentNumberOfHumans = 0;
    

    [Header("Spawning Parameters")]
    [SerializeField] int maxSpawnRateNumberOfDestroyedConstruction = 20;
    [SerializeField] AnimationCurve spawnRateDependingOnNumberOfDestroyedConstructions = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] float minimumSpawningRate = 0.5f;
    [SerializeField] float maximumSpawningRate = 3f;

    FrequenceSystem spawningFrequenceSystem = default;

    List<PhysicalObjectConstructionScript> inGameSpanwers = new List<PhysicalObjectConstructionScript>();
    int numberOfDestoyedConstructions = 0;


    #region Humans' Probabilities

    /// <summary>
    /// Returns the total probabilities for each human prefab to spawn
    /// </summary>
    /// <returns></returns>
    public int GetTotalProbability()
    {
        int totalProba = 0;

        foreach (HumanPrefabWithProbability humanWithProba in allHumanPrefabsWithSpawnProbability)
            totalProba += humanWithProba.probability;

        return totalProba;
    }

    /// <summary>
    /// Returns a random human prefab base on Humans' prefabs' probabilities
    /// </summary>
    /// <returns></returns>
    public HumanScript GetRandomHumanPrefab()
    {
        HumanScript pickedPrefab = null;

        int randomProbability = Random.Range(0, totalProbability);

        int probaCounter = 0;

        foreach (HumanPrefabWithProbability humanWithProba in allHumanPrefabsWithSpawnProbability)
        {
            probaCounter += humanWithProba.probability;
            if (randomProbability < probaCounter)
            {
                pickedPrefab = humanWithProba.humanPrefab;
                break;
            }
        }

        return pickedPrefab;
    }

    #endregion

          
    /// <summary>
    /// Initialize values and spawningFrequenceSystem
    /// </summary>
    public void SetUp()
    {
        totalProbability = GetTotalProbability();
        spawningFrequenceSystem = new FrequenceSystem(minimumSpawningRate);
        spawningFrequenceSystem.SetUp(SpawnHumanOnRandomSpawnerNoReturn);
    }


    #region Spawning System

    /// <summary>
    /// Updates the spawningFrequenceSystem if nothing is preventing it from doing so
    /// </summary>
    public void UpdateSpawningSystem()
    {
        //Prevent Humans from being spawned by the frequence system if no construction has been destroyed yet, or if there are too many humans
        if (numberOfDestoyedConstructions == 0 || currentNumberOfHumans >= maximumNumberOfHumans)
            return;

        spawningFrequenceSystem.UpdateFrequence();
    }

    /// <summary>
    /// Updates the spawning rate by processing the current Giant Construction's progression 
    /// </summary>
    public void UpdateSpawnRate()
    {
        float newFrequenceCoeff = GameManager.gameManager.GetGiantConstruction.GetCompletionCoeff;
        spawningFrequenceSystem.ChangeFrequence(Mathf.Lerp(minimumSpawningRate, maximumSpawningRate, newFrequenceCoeff));
    }

    /// <summary>
    /// Add a new spawner to the list of available ones
    /// </summary>
    /// <param name="newSpawner"></param>
    public void AddSpawner(PhysicalObjectConstructionScript newSpawner)
    {
        if (newSpawner != null)
            inGameSpanwers.Add(newSpawner);
    }

    /// <summary>
    /// Removes a spawner from the list of available ones
    /// </summary>
    /// <param name="newSpawner"></param>
    public void RemoveSpawner(PhysicalObjectConstructionScript spawnerToRemove)
    {
        if (inGameSpanwers.Contains(spawnerToRemove))
            inGameSpanwers.Remove(spawnerToRemove);
    }

    #endregion


    #region Spawning Humans
       
    /// <summary>
    /// Spawns a Human on any spawning point without returning it. Called by the frequency system.
    /// </summary>
    public void SpawnHumanOnRandomSpawnerNoReturn()
    {
        SpawnHumanOnRandomSpawner();
    }

    /// <summary>
    /// Spawns a Human on any spawning point and returns it.
    /// </summary>
    public HumanScript SpawnHumanOnRandomSpawner()
    {
        if (inGameSpanwers.Count > 0)
        {
            PhysicalObjectConstructionScript randomSpawner = inGameSpanwers[Random.Range(0, inGameSpanwers.Count)];
            return SpawnRandomHuman(randomSpawner.GetRandomSpawnPos());
        }
        else
            return SpawnHumanOutsideGameArea();
    }

    /// <summary>
    /// Spawnd a Human outside of the game area - This is called when no more spawning points are available.
    /// </summary>
    /// <returns></returns>
    public HumanScript SpawnHumanOutsideGameArea()
    {
        Vector3 randomDirection = Random.onUnitSphere;
        randomDirection.y = 0;
        randomDirection.Normalize();

        return SpawnRandomHuman(GameManager.gameManager.GetGiantConstruction.transform.position + randomDirection * GameManager.gameManager.gameAreaRadius + Vector3.up);
    }

    /// <summary>
    /// Spawns a human on a specific position and sets it up
    /// </summary>
    /// <param name="spawnPos"></param>
    /// <returns></returns>
    public HumanScript SpawnRandomHuman(Vector3 spawnPos)
    {
        //We get a random human prefab
        HumanScript pickedPrefab = GetRandomHumanPrefab();

        if (pickedPrefab == null)
            return null;

        HumanScript newHuman = Object.Instantiate(pickedPrefab, spawnPos, Quaternion.identity);
        newHuman.transform.parent = GameManager.gameManager.GetAllGameObjectsParent;
        newHuman.SetUp();

        IncreamentNumberOfHumans();

        return newHuman;
    }

    public void IncreamentNumberOfHumans()
    {
        currentNumberOfHumans++;
    }

    public void DecreamentNumberOfHumans()
    {
        if (currentNumberOfHumans > 0)
            currentNumberOfHumans--;
    }

    #endregion


    public void IncreamentNumberOfDestroyedConstructions()
    {
        numberOfDestoyedConstructions++;
    }
}

/// <summary>
/// Binds a Human's prefab with a probability weight
/// </summary>
[System.Serializable]
public struct HumanPrefabWithProbability
{
    public HumanScript humanPrefab;
    public int probability;
}