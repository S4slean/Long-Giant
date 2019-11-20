using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalObjectConstructionScript : PhysicalObjectScript
{
    [Header("Construction")]
    [SerializeField] List<ResourceWithQuantity> allResourcesSpawnedOnDestroy = new List<ResourceWithQuantity>();
    [SerializeField] float minimumEjectionForce = 5f;
    [SerializeField] float maximumEjectionForce = 10f;
    [SerializeField] float resourcesSpawnDistanceFromObject = 0.5f;


    public override void DestroyPhysicalObject()
    {
        PhysicalObjectResourceScript resourcePrefab = null;
        PhysicalObjectResourceScript newResource = null;
        Vector3 randomThrowVelocity = Vector3.up;

        foreach (ResourceWithQuantity resourceWithQuantity in allResourcesSpawnedOnDestroy)
        {
            Debug.Log("Instantiate " + resourceWithQuantity.quantity + " of " + resourceWithQuantity.resourceType);
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
        base.DestroyPhysicalObject();
    }
}
