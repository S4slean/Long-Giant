using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HumanSpawningManager
{
    [Header("Humans Prefabs")]
    [SerializeField] List<HumanPrefabWithProbability> allHumanPrefabsWithSpawnProbability = new List<HumanPrefabWithProbability>();
    [SerializeField] int maximumNumberOfHumans = 20;
    int currentNumberOfHumans = 0;
    public int GetTotalProbability()
    {
        int totalProba = 0;

        foreach(HumanPrefabWithProbability humanWithProba in allHumanPrefabsWithSpawnProbability)
            totalProba += humanWithProba.probability;

        return totalProba;
    }
    int totalProbability = 0;

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


    List<PhysicalObjectConstructionScript> inGameSpanwers = new List<PhysicalObjectConstructionScript>();
    int numberOfDestoyedConstructions = 0;

    [Header("Spawning Parameters")]
    [SerializeField] int maxSpawnRateNumberOfDestroyedConstruction = 20;
    [SerializeField] AnimationCurve spawnRateDependingOnNumberOfDestroyedConstructions = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] float minimumSpawningRate = 0.5f;
    [SerializeField] float maximumSpawningRate = 3f;

    FrequenceSystem spawningFrequenceSystem = default;

    public void SetUp()
    {
        totalProbability = GetTotalProbability();
        spawningFrequenceSystem = new FrequenceSystem(minimumSpawningRate);
        spawningFrequenceSystem.SetUp(SpawnHumanOnRandomSpawnerNoReturn);
    }

    public void UpdateSpawningSystem()
    {
        if (numberOfDestoyedConstructions == 0 || currentNumberOfHumans >= maximumNumberOfHumans)
            return;

        spawningFrequenceSystem.UpdateFrequence();
    }

    public void AddSpawner(PhysicalObjectConstructionScript newSpawner)
    {
        if (newSpawner != null)
            inGameSpanwers.Add(newSpawner);

        Debug.Log("Number of spawners : " + inGameSpanwers.Count);
    }

    public void RemoveSpawner(PhysicalObjectConstructionScript spawnerToRemove)
    {
        if (inGameSpanwers.Contains(spawnerToRemove))
            inGameSpanwers.Remove(spawnerToRemove);
        Debug.Log("Number of spawners : " + inGameSpanwers.Count);
    }

    public void IncreamentNumberOfDestroyedConstructions()
    {
        numberOfDestoyedConstructions++;

        float newFrequenceCoeff = spawnRateDependingOnNumberOfDestroyedConstructions.Evaluate((float)numberOfDestoyedConstructions/maxSpawnRateNumberOfDestroyedConstruction);
        spawningFrequenceSystem.ChangeFrequence(Mathf.Lerp(minimumSpawningRate, maximumSpawningRate, newFrequenceCoeff));
        //Debug.Log("current frequence : " + Mathf.Lerp(minimumSpawningRate, maximumSpawningRate, newFrequenceCoeff));
    }

    public void SpawnHumanOnRandomSpawnerNoReturn()
    {
        SpawnHumanOnRandomSpawner();
    }

    public HumanScript SpawnHumanOnRandomSpawner()
    {
        if (inGameSpanwers.Count > 0)
        {
            PhysicalObjectConstructionScript randomSpawner = inGameSpanwers[Random.Range(0, inGameSpanwers.Count)];
            return SpawnRandomHuman(randomSpawner.GetRandomSpawnPos());
        }
        else
        {
            return SpawnHumanOutsideGameArea();
        }
    }

    public HumanScript SpawnHumanOutsideGameArea()
    {
        return SpawnRandomHuman(Vector3.zero);
    }

    public HumanScript SpawnRandomHuman(Vector3 spawnPos)
    {
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
}

[System.Serializable]
public struct HumanPrefabWithProbability
{
    public HumanScript humanPrefab;
    public int probability;
}