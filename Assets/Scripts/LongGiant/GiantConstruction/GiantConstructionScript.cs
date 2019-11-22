using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantConstructionScript : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            FinishConstruction();
        }
    }

    bool constructionFinished = false;

    public void FinishConstruction()
    {
        GameManager.gameManager.OnGameWin?.Invoke();
        constructionFinished = true;
        constructionAnimator.SetBool("won", true);
        fireworksParent.SetActive(true);

        if (winSound != null)
            winSound.Play();
    }

    [Header("References")]
    [SerializeField] MeshRenderer constructionRenderer = default;
    [SerializeField] MeshRenderer constructionGhostRenderer = default;
    [SerializeField] Animator constructionAnimator = default;
    [SerializeField] GameObject fireworksParent = default;
    [SerializeField] AudioSource buildingSound = default;
    [SerializeField] AudioSource winSound = default;

    public void UpdateRenderer()
    {
        constructionRenderer.material.SetFloat("_Fill", GetCompletionCoeff);
        constructionGhostRenderer.material.SetFloat("_Fill", GetCompletionCoeff);
    }

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
    int totalNumberOfNeededResources = 0;
    int currentNumberOfResources = 0;
    public float GetCompletionCoeff { get { return totalNumberOfNeededResources > 0 ?(float)currentNumberOfResources / totalNumberOfNeededResources : 0; } }

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
            int resourceQuantity = allNeededResources[resourceType];
            totalNumberOfNeededResources += resourceQuantity;

            currentlyStoredResourcesDictionary.Add(resourceType, 0);
            ResourceSingleInformationsUI newInfoUI = Instantiate(singleInformationsUIPrefab, singleInformationsUIParent);
            newInfoUI.SetUp(GameManager.gameManager.GetResourceDisplayInformations(resourceType));
            newInfoUI.UpdateText(0, resourceQuantity);

            informations.Add(resourceType, newInfoUI);
        }

        int informationsCount = informations.Count;
        int counter = 0;
        foreach (ResourceType resourceType in informations.Keys)
        {
            informations[resourceType].transform.localPosition = new Vector3(0, spaceBetweenTwoLines * (informations.Count - 1) * 0.5f - counter * spaceBetweenTwoLines, 0);

            counter++;
        }

        UpdateRenderer();
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
                        currentNumberOfResources++;
                        UpdateRenderer();
                        constructionAnimator.SetTrigger("addResource");
                        if (buildingSound != null)
                            buildingSound.Play();
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
                FinishConstruction();
            }
        }
    }

    public void EjectRandomResource()
    {
        if (constructionFinished)
            return;

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
        currentNumberOfResources--;
        UpdateRenderer();
        constructionAnimator.SetTrigger("removeResource");

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
            
            constructionAnimator.SetTrigger("removeResource");
        }
    }
    #endregion

    #region Damages Management
    public void ReceiveDamages(int damagesAmount)
    {
        if (constructionFinished)
            return;

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

