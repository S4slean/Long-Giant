using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalObjectConstructionScript : PhysicalObjectScript
{
    [Header("Construction")]
    [SerializeField] List<ResourceWithQuantity> allResourcesSpawnedOnDestroy = new List<ResourceWithQuantity>();
    public List<ResourceWithQuantity> GetAllResourcesSpawnedOnDestroy { get { return allResourcesSpawnedOnDestroy; } }
    [SerializeField] float minimumEjectionForce = 5f;
    [SerializeField] float maximumEjectionForce = 10f;
    [SerializeField] float resourcesSpawnDistanceFromObject = 0.5f;

    [Header("Spawning")]
    [SerializeField] bool isSpawner = false;
    [SerializeField] float spawningOffset = 1;
    [SerializeField] int numberOfHumanSpawnedOnDestroy = 0;
    HumanSpawningManager spawningManager = default;

    /// <summary>
    /// Overriding SetUp in order to add this Construction as spawner in the spawning system.
    /// </summary>
    public override void SetUp()
    {
        base.SetUp();

        spawningManager = GameManager.gameManager.GetHumanSpawningManager;

        if (isSpawner)
            spawningManager.AddSpawner(this);
    }


    /// <summary>
    /// Overriding DestroyPhysicalObject in order to spawn Resources and Humans when destroyed
    /// </summary>
    public override void DestroyPhysicalObject()
    {
        PhysicalObjectResourceScript resourcePrefab = null;
        PhysicalObjectResourceScript newResource = null;
        Vector3 randomThrowVelocity = Vector3.up;

        //We check each Resource that should be spawn, spawn it and throws it around
        foreach (ResourceWithQuantity resourceWithQuantity in allResourcesSpawnedOnDestroy)
        {
            //Get the Resource Prefab
            resourcePrefab = GameManager.gameManager.ResourcesManager.GetResourcePrefab(resourceWithQuantity.resourceType);

            if (resourcePrefab == null)
                continue;

            //Spawn the related number of Resources
            for (int i = 0; i < resourceWithQuantity.quantity; i++)
            {
                //Get random direction
                randomThrowVelocity = Random.onUnitSphere;
                randomThrowVelocity.y = Mathf.Abs(randomThrowVelocity.y);

                //Instantiate object and set it up
                newResource = Instantiate(resourcePrefab, transform.position + randomThrowVelocity * resourcesSpawnDistanceFromObject, Random.rotation);
                newResource.transform.parent = GameManager.gameManager.GetAllGameObjectsParent;
                newResource.SetUp();

                //Eject with random force
                newResource.Throw(randomThrowVelocity * Random.Range(minimumEjectionForce, maximumEjectionForce));
            }
        }

        //Spawn as many human as needed, and throw them around
        HumanScript pickedHumanPrefab = null;
        HumanScript newHuman = null;
        for (int i = 0; i < numberOfHumanSpawnedOnDestroy; i++)
        {
            //Get a random Human prefab to spawn
            pickedHumanPrefab = spawningManager.GetRandomHumanPrefab();

            if (pickedHumanPrefab == null)
                continue;

            //Get random direction
            randomThrowVelocity = Random.onUnitSphere;
            randomThrowVelocity.y = Mathf.Abs(randomThrowVelocity.y);

            //Instantiate and set up
            newHuman = Instantiate(pickedHumanPrefab, transform.position + randomThrowVelocity * resourcesSpawnDistanceFromObject, Random.rotation);
            newHuman.transform.parent = GameManager.gameManager.GetAllGameObjectsParent;
            newHuman.SetUp();

            //Throw it with random velocity
            newHuman.Throw(randomThrowVelocity * Random.Range(minimumEjectionForce, maximumEjectionForce));

            spawningManager.IncreamentNumberOfHumans();
        }

        spawningManager.IncreamentNumberOfDestroyedConstructions();
        //If this construction was a spawner, remove it from the Spawning Manager list
        if (isSpawner)
            spawningManager.RemoveSpawner(this);

        base.DestroyPhysicalObject();
    }


    /// <summary>
    /// Returns a random spawning position for a Human to spawn on.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetRandomSpawnPos()
    {
        if (this == null)
            return Vector3.zero;

        Vector3 spawnPos = transform.position;

        Vector3 spawnPosOffset = Random.onUnitSphere;
        spawnPosOffset.y = 0;
        spawnPosOffset.Normalize();

        spawnPos += spawnPosOffset * spawningOffset;

        return spawnPos;
    }
}
