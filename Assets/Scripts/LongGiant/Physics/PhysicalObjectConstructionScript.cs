using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalObjectConstructionScript : PhysicalObjectScript
{
    public override void SetUp()
    {
        base.SetUp();

        spawningManager = GameManager.gameManager.GetHumanSpawningManager;

        if (isSpawner)
            spawningManager.AddSpawner(this);
    }

    [Header("Construction")]
    [SerializeField] List<ResourceWithQuantity> allResourcesSpawnedOnDestroy = new List<ResourceWithQuantity>();
    [SerializeField] float minimumEjectionForce = 5f;
    [SerializeField] float maximumEjectionForce = 10f;
    [SerializeField] float resourcesSpawnDistanceFromObject = 0.5f;

    [Header("Spawning")]
    [SerializeField] bool isSpawner = false;
    [SerializeField] float spawningOffset = 1;
    public Vector3 GetRandomSpawnPos()
    {
        Vector3 spawnPos = transform.position;

        Vector3 spawnPosOffset = Random.onUnitSphere;
        spawnPosOffset.y = 0;
        spawnPosOffset.Normalize();

        spawnPos += spawnPosOffset * spawningOffset;

        return spawnPos;
    }
    [SerializeField] int numberOfHumanSpawnedOnDestroy = 0;
    HumanSpawningManager spawningManager = default;

    public override void DestroyPhysicalObject()
    {
        PhysicalObjectResourceScript resourcePrefab = null;
        PhysicalObjectResourceScript newResource = null;
        Vector3 randomThrowVelocity = Vector3.up;

        foreach (ResourceWithQuantity resourceWithQuantity in allResourcesSpawnedOnDestroy)
        {
            resourcePrefab = GameManager.gameManager.ResourcesManager.GetResourcePrefab(resourceWithQuantity.resourceType);

            if (resourcePrefab == null)
                continue;

            for (int i = 0; i < resourceWithQuantity.quantity; i++)
            {
                randomThrowVelocity = Random.onUnitSphere;
                randomThrowVelocity.y = Mathf.Abs(randomThrowVelocity.y);

                newResource = Instantiate(resourcePrefab, transform.position + randomThrowVelocity * resourcesSpawnDistanceFromObject, Random.rotation);
                newResource.SetUp();

                newResource.Throw(randomThrowVelocity * Random.Range(minimumEjectionForce, maximumEjectionForce));
            }
        }

        HumanScript pickedHumanPrefab = null;
        HumanScript newHuman = null;
        for(int i = 0; i < numberOfHumanSpawnedOnDestroy; i++)
        {
            pickedHumanPrefab = spawningManager.GetRandomHumanPrefab();
            //newHuman = Instantiate(GetRandomSpawnPos);

            randomThrowVelocity = Random.onUnitSphere;
            randomThrowVelocity.y = Mathf.Abs(randomThrowVelocity.y);

            newHuman = Instantiate(pickedHumanPrefab, transform.position + randomThrowVelocity * resourcesSpawnDistanceFromObject, Random.rotation);
            newHuman.SetUp();

            newHuman.Throw(randomThrowVelocity * Random.Range(minimumEjectionForce, maximumEjectionForce));
        }

        spawningManager.IncreamentNumberOfDestroyedConstructions();
        if (isSpawner)
            spawningManager.RemoveSpawner(this);

        base.DestroyPhysicalObject();
    }
}
