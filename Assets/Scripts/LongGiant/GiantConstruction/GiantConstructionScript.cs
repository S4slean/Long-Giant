using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantConstructionScript : MonoBehaviour
{
    bool constructionFinished = false;

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
    Dictionary<ResourceType, int> allNeededResourcesDictionnary = new Dictionary<ResourceType, int>();
    Dictionary<ResourceType, int> currentlyStoredResourcesDictionary = new Dictionary<ResourceType, int>();

   [Header("Damages Reception")]
    [SerializeField] int damagesStepToLoseOneResource = 10;
    int remainingDamagesBeforeNextResourceLoss;

    [Header("Ejection")]
    [SerializeField] Transform ejectionPosition = default;
    [SerializeField] float minimumEjectionForce = 5;
    [SerializeField] float maximumEjectionForce = 8;
    [SerializeField] float ejectionDistanceFromCenter = 1;

    private void Awake()
    {
        SetUp();
    }

    public void SetUp()
    {
        allNeededResourcesDictionnary = new Dictionary<ResourceType, int>();
        foreach(ResourceWithQuantity neededResource in allNeededResources)
        {
            if (!allNeededResourcesDictionnary.ContainsKey(neededResource.resourceType))
                allNeededResourcesDictionnary.Add(neededResource.resourceType, neededResource.quantity);
        }

        remainingDamagesBeforeNextResourceLoss = damagesStepToLoseOneResource;
    }

    #region Resources Management
    public void AddResourceToConstruction(ResourceType resourceType)
    {
        bool ejectResource = false;

        if (!allNeededResourcesDictionnary.ContainsKey(resourceType))
            ejectResource = true;
        else
        {
            bool placedInExistantElement = false;

            foreach(ResourceType currentlyStoredResourceType in currentlyStoredResourcesDictionary.Keys)
            {
                if (currentlyStoredResourceType == resourceType)
                {
                    if (currentlyStoredResourcesDictionary[currentlyStoredResourceType] >= allNeededResourcesDictionnary[resourceType])
                        ejectResource = true;
                    else
                    {
                        currentlyStoredResourcesDictionary[currentlyStoredResourceType]++;
                        placedInExistantElement = true;
                    }

                    break;
                }
            }

            if (!placedInExistantElement && !ejectResource)
            {
                if (allNeededResourcesDictionnary[resourceType] <= 0)
                    ejectResource = true;
                else
                    currentlyStoredResourcesDictionary.Add(resourceType, 1);
            }
        }

        if (ejectResource)
            StartCoroutine(RefuseResource(resourceType));
        else
        {
            if (CheckIfConstructionFinished())
            {
                Debug.Log("Construction finished !");
                constructionFinished = true;
            }
        }
    }

    public void EjectRandomResource()
    {
        List<ResourceType> availableResourcesTypes = new List<ResourceType>();

        foreach(ResourceType storedResourceType in currentlyStoredResourcesDictionary.Keys)
        {
            if (currentlyStoredResourcesDictionary[storedResourceType] > 0)
                availableResourcesTypes.Add(storedResourceType);
        }

        if (availableResourcesTypes.Count == 0)
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

        currentlyStoredResourcesDictionary[resourceTypeToEject]--;
    }

    IEnumerator RefuseResource(ResourceType resourceTypeToEject)
    {
        yield return new WaitForSeconds(0.2f);

        PhysicalObjectResourceScript prefab = GameManager.gameManager.ResourcesManager.GetResourcePrefab(resourceTypeToEject);

        if (prefab != null)
        {
            Vector3 randomThrowVelocity = Random.onUnitSphere;
            randomThrowVelocity.y = Mathf.Abs(randomThrowVelocity.y);
            randomThrowVelocity = Vector3.Slerp(randomThrowVelocity, Vector3.up, 0.5f);

            PhysicalObjectResourceScript newResourceObject = Instantiate(prefab, ejectionPosition.position + randomThrowVelocity * ejectionDistanceFromCenter, Quaternion.identity);
            newResourceObject.SetUp();

            newResourceObject.SetCantBePlacedOnGiantConstruction(false);
            newResourceObject.Throw(randomThrowVelocity * Random.Range(minimumEjectionForce, maximumEjectionForce));
        }
    }
    #endregion

    #region Damages Management
    public void ReceiveDamages(int damagesAmount)
    {
        remainingDamagesBeforeNextResourceLoss -= damagesAmount;

        while (remainingDamagesBeforeNextResourceLoss <= 0)
        {
            remainingDamagesBeforeNextResourceLoss += damagesStepToLoseOneResource;
            EjectRandomResource();
        }
    }
    #endregion

    public bool CheckIfConstructionFinished()
    {
        bool finished = true;

        foreach(ResourceType neededResourceType in allNeededResourcesDictionnary.Keys)
        {
            Debug.Log(neededResourceType + " : " + (currentlyStoredResourcesDictionary.ContainsKey(neededResourceType) ? currentlyStoredResourcesDictionary[neededResourceType] : 0) + "/" + allNeededResourcesDictionnary[neededResourceType]);
            if (currentlyStoredResourcesDictionary.ContainsKey(neededResourceType))
            {
                if(currentlyStoredResourcesDictionary[neededResourceType] < allNeededResourcesDictionnary[neededResourceType])
                {
                    finished = false;
                    //break;
                }
            }
            else
            {
                finished = false;
                //break;
            }
        }

        return finished;
    }
}

