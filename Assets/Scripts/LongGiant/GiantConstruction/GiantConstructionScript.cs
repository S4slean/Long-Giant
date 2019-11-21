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

    [Header("UI")]
    [SerializeField] Transform UIParent = default;
    [SerializeField] ResourceSingleInformationsUI singleInformationsUIPrefab = default;
    [SerializeField] Transform singleInformationsUIParent = default;
    [SerializeField] float spaceBetweenTwoLines = 0.5f;

    Dictionary<ResourceType, ResourceSingleInformationsUI> informations = new Dictionary<ResourceType, ResourceSingleInformationsUI>();

    bool setedUp = false;
    private void Start()
    {
        SetUp();
    }

    public void SetUp()
    {
        if (setedUp)
            return;

        setedUp = true;

        remainingDamagesBeforeNextResourceLoss = damagesStepToLoseOneResource;
    }

    public void GenerateNeededResourcesDictionary(Dictionary<ResourceType, int> allNeededResources)
    {
        allNeededResourcesDictionnary = allNeededResources;
        foreach (ResourceType resourceType in allNeededResourcesDictionnary.Keys)
        {
            currentlyStoredResourcesDictionary.Add(resourceType, 0);
            ResourceSingleInformationsUI newInfoUI = Instantiate(singleInformationsUIPrefab, singleInformationsUIParent);
            newInfoUI.SetUp(GameManager.gameManager.GetResourceDisplayInformations(resourceType));
            newInfoUI.UpdateText(0, allNeededResources[resourceType]);

            informations.Add(resourceType, newInfoUI);
        }

        int informationsCount = informations.Count;
        int counter = 0;
        foreach (ResourceType resourceType in informations.Keys)
        {
            informations[resourceType].transform.localPosition = new Vector3(0, spaceBetweenTwoLines * (informations.Count - 1) * 0.5f - counter * spaceBetweenTwoLines, 0);

            counter++;
        }
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
                    int currentValue = currentlyStoredResourcesDictionary[resourceType];
                    int maxValue = allNeededResourcesDictionnary[resourceType];

                    if (currentValue >= maxValue)
                        ejectResource = true;
                    else
                    {
                        currentlyStoredResourcesDictionary[resourceType]++;
                        informations[resourceType].UpdateText(currentlyStoredResourcesDictionary[resourceType], maxValue);
                        placedInExistantElement = true;
                    }

                    break;
                }
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
        newResourceObject.transform.parent = GameManager.gameManager.GetAllGameObjectsParent;
        newResourceObject.SetUp();

        newResourceObject.SetCantBePlacedOnGiantConstruction(false);
        newResourceObject.Throw(randomThrowVelocity * Random.Range(minimumEjectionForce, maximumEjectionForce));

        currentlyStoredResourcesDictionary[resourceTypeToEject]--;

        informations[resourceTypeToEject].UpdateText(currentlyStoredResourcesDictionary[resourceTypeToEject], allNeededResourcesDictionnary[resourceTypeToEject]);
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
            newResourceObject.transform.parent = GameManager.gameManager.GetAllGameObjectsParent;
            newResourceObject.SetUp();

            newResourceObject.SetCantBePlacedOnGiantConstruction(false);
            newResourceObject.Throw(randomThrowVelocity * Random.Range(minimumEjectionForce, maximumEjectionForce));
        }
    }
    #endregion

    #region Damages Management
    public void ReceiveDamages(int damagesAmount)
    {
        //Debug.Log("Receive damages : " + damagesAmount);
        remainingDamagesBeforeNextResourceLoss -= damagesAmount;

        while (remainingDamagesBeforeNextResourceLoss <= 0)
        {
            Debug.Log("Lose Resource");
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

