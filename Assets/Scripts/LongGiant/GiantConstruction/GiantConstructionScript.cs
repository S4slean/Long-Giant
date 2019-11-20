using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantConstructionScript : MonoBehaviour
{
    /*[SerializeField] List<GiantConstructionStorey> allConstructionStoreys = new List<GiantConstructionStorey>();
    int currentStoryCounter = 0;
    GiantConstructionStorey currentStoreyParameters = default;

    public void SelectCurrentStoreyParameters()
    {
        if (currentStoryCounter < allConstructionStoreys.Count)
            currentStoreyParameters = allConstructionStoreys[currentStoryCounter];
    }*/
    [Header("Construction Parameters")]
    [SerializeField] List<ResourceWithQuantity> allNeededResources = new List<ResourceWithQuantity>();
    [SerializeField] List<ResourceWithQuantity> currentlyStoredResources = new List<ResourceWithQuantity>();

    [Header("Constant Parameters")]
    [SerializeField] Transform ejectionPosition = default;
    [SerializeField] float minimumEjectionForce = 5;
    [SerializeField] float maximumEjectionForce = 8;
    [SerializeField] float ejectionDistanceFromCenter = 1;


    public void AddResourceToConstruction(ResourceType resourceType)
    {
        bool placedInExistantElement = false;

        for(int i =0; i < currentlyStoredResources.Count; i++)
        {
            ResourceWithQuantity resourceWithQuantity = currentlyStoredResources[i];
            if(resourceWithQuantity.resourceType == resourceType)
            {
                resourceWithQuantity.quantity++;
                currentlyStoredResources[i] = resourceWithQuantity;
                placedInExistantElement = true;
            }
        }

        if (!placedInExistantElement)
        {
            ResourceWithQuantity newResourceWithQuantity = new ResourceWithQuantity();
            newResourceWithQuantity.resourceType = resourceType;
            newResourceWithQuantity.quantity = 1;
            currentlyStoredResources.Add(newResourceWithQuantity);
        }
    }

    public void EjectRandomResource()
    {
        List<ResourceType> availableResourcesTypes = new List<ResourceType>();

        foreach(ResourceWithQuantity resource in currentlyStoredResources)
        {
            if (resource.quantity > 0)
                availableResourcesTypes.Add(resource.resourceType);
        }

        if(availableResourcesTypes.Count == 0)
            return;

        ResourceType resourceTypeToEject = availableResourcesTypes[Random.Range(0, availableResourcesTypes.Count)];

        PhysicalObjectResourceScript prefab = GameManager.gameManager.ResourcesManager.GetResourcePrefab(resourceTypeToEject);

        if (prefab == null)
            return;

        Vector3 randomThrowVelocity = Random.onUnitSphere;
        randomThrowVelocity.y = Mathf.Abs(randomThrowVelocity.y);
        randomThrowVelocity = Vector3.Slerp(randomThrowVelocity, Vector3.up, 0.5f);

        PhysicalObjectResourceScript newResourceObject = Instantiate(prefab, ejectionPosition.position + randomThrowVelocity * ejectionDistanceFromCenter, Quaternion.identity);
        newResourceObject.SetUp();

        newResourceObject.SetCantBePlacedOnGiantConstruction(false);
        newResourceObject.Throw(randomThrowVelocity * Random.Range(minimumEjectionForce, maximumEjectionForce));

        for (int i = 0; i < currentlyStoredResources.Count; i++)
        {
            ResourceWithQuantity resource = currentlyStoredResources[i];
            if (resource.resourceType == resourceTypeToEject)
            {
                ResourceWithQuantity newValue = resource;
                newValue.quantity--;
                currentlyStoredResources[i] = newValue;
                break;
            }
        }
    }

    public void CheckIfConstructionFinished()
    {

    }
}

