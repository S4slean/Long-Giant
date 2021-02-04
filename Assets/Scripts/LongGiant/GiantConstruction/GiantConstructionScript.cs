using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Giant's Construction, able to receive Resources in order to complete player's goal.
/// </summary>
public class GiantConstructionScript : MonoBehaviour
{
    bool setedUp = false;
    bool constructionFinished = false;
        

    [Header("References")]
    [SerializeField] MeshRenderer constructionRenderer = default;
    [SerializeField] MeshRenderer constructionGhostRenderer = default;
    [SerializeField] Animator constructionAnimator = default;
    [SerializeField] GameObject fireworksParent = default;
    [SerializeField] AudioSource buildingSound = default;
    [SerializeField] AudioSource winSound = default;

    [Header("Construction Parameters")]
    Dictionary<ResourceType, int> allNeededResourcesDictionnary = new Dictionary<ResourceType, int>();
    Dictionary<ResourceType, int> currentlyStoredResourcesDictionary = new Dictionary<ResourceType, int>();
    int totalNumberOfNeededResources = 0;
    int currentNumberOfResources = 0;
    /// <summary>
    /// Get a coefficient depending on the number of gathered Resources (0 is no Resource, 1 is all required Resources gathered)
    /// </summary>
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

    /// <summary>
    /// Holds the displayed information of each Resource type
    /// </summary>
    Dictionary<ResourceType, ResourceSingleInformationsUI> informations = new Dictionary<ResourceType, ResourceSingleInformationsUI>();


    #region Engine Callbacks

    private void Start()
    {
        SetUp();
    }

    private void Update()
    {
        //Just a debug input to complete construction automatically in editor
        if (Input.GetKeyDown(KeyCode.T))
        {
            FinishConstruction();
        }
    }

    #endregion


    public void SetUp()
    {
        if (setedUp)
            return;

        setedUp = true;

        remainingDamagesBeforeNextResourceLoss = damagesStepToLoseOneResource;
    }

    /// <summary>
    /// Generates the allNeededResourcesDictionnary with the passed parameter,
    /// </summary>
    /// <param name="allNeededResources"></param>
    public void GenerateNeededResourcesDictionary(Dictionary<ResourceType, int> allNeededResources)
    {
        allNeededResourcesDictionnary = allNeededResources;

        //We check every Resource in order to add its needed count to the total value and generate a UI element to display the required quantity for that resource
        foreach (ResourceType resourceType in allNeededResourcesDictionnary.Keys)
        {
            int resourceQuantity = allNeededResources[resourceType];
            //Increase total
            totalNumberOfNeededResources += resourceQuantity;

            //Initialize currentlyStoredResourcesDictionary value for that Resource type
            currentlyStoredResourcesDictionary.Add(resourceType, 0);

            //Generate a Resource info line (displays image, current count and needed count)
            ResourceSingleInformationsUI newInfoUI = Instantiate(singleInformationsUIPrefab, singleInformationsUIParent);
            newInfoUI.SetUp(GameManager.gameManager.GetResourceDisplayInformations(resourceType));
            newInfoUI.UpdateText(0, resourceQuantity);

            informations.Add(resourceType, newInfoUI);
        }

        //We place each Resource UI line correclty depending on number of lines and line spacing
        int informationsCount = informations.Count;
        int counter = 0;
        foreach (ResourceType resourceType in informations.Keys)
        {
            informations[resourceType].transform.localPosition = new Vector3(0, spaceBetweenTwoLines * (informations.Count - 1) * 0.5f - counter * spaceBetweenTwoLines, 0);

            counter++;
        }

        //We initialize the renderer once
        UpdateRenderer();
    }


    #region Resources Management

    /// <summary>
    /// Called when a Resource enters the construction trigger zone. 
    /// Tries to add it to the dictionary of current resources. 
    /// If the required count for that resource is already reached, ejects the resource.
    /// Else, add it to dictionary and plays progression-linked events (updating rendering, updating enemies spawning rate...)
    /// </summary>
    /// <param name="resourceType"></param>
    public void AddResourceToConstruction(ResourceType resourceType)
    {
        bool ejectResource = false;

        //We eject the resource automatically if it is not required at all.
        if (!allNeededResourcesDictionnary.ContainsKey(resourceType))
            ejectResource = true;
        else
        {
            //When the resource is accepted, increament the Resource count in the currentlyStoredResourcesDictionary, Update Rendering and Enemies Spawn Rate
            void AcceptResource(int _maxValue)
            {
                currentlyStoredResourcesDictionary[resourceType]++;
                currentNumberOfResources++;

                informations[resourceType].UpdateText(currentlyStoredResourcesDictionary[resourceType], _maxValue);
                UpdateRenderer();
                constructionAnimator.SetTrigger("addResource");
                if (buildingSound != null)
                    buildingSound.Play();

                GameManager.gameManager.GetHumanSpawningManager.UpdateSpawnRate();
            }

            int maxValue = allNeededResourcesDictionnary[resourceType];
            //Search for the added Resource type in the currentlyStoredResourcesDictionary
            if (currentlyStoredResourcesDictionary.ContainsKey(resourceType))
            {
                int currentValue = currentlyStoredResourcesDictionary[resourceType];

                //Accept resource if needed value hasn't been reached already
                if (currentValue < maxValue)
                    AcceptResource(maxValue);
                else
                    ejectResource = true;
            }
            else
            {
                //Accept resource if needed value isn't 0
                if (maxValue > 0)
                {
                    currentlyStoredResourcesDictionary.Add(resourceType, 0);
                    AcceptResource(maxValue);
                }
                else
                    ejectResource = true;
            }
        }

        //If the resource is refused, we play some feedback and eject the resource after a short amount of time
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

    /// <summary>
    /// Picks up a random Resource in the list of those which are currently stored on the construction, and ejects it.
    /// </summary>
    public void EjectRandomResource()
    {
        if (constructionFinished)
            return;

        //We check all currently stored Resources to get a list of ejectable Resources
        List<ResourceType> availableResourcesTypes = new List<ResourceType>();
        foreach(ResourceType storedResourceType in currentlyStoredResourcesDictionary.Keys)
        {
            if (currentlyStoredResourcesDictionary[storedResourceType] > 0)
                availableResourcesTypes.Add(storedResourceType);
        }

        //If no resource is available, do not eject anything
        if (availableResourcesTypes.Count == 0)
            return;

        //Get a random Resource amoung the available once and eject it
        ResourceType resourceTypeToEject = availableResourcesTypes[Random.Range(0, availableResourcesTypes.Count)];
        EjectResource(resourceTypeToEject);

        //Remove a Resource from current counts, then update Rendering and Enemies Spawn Rate
        currentlyStoredResourcesDictionary[resourceTypeToEject]--;
        currentNumberOfResources--;

        UpdateRenderer();
        informations[resourceTypeToEject].UpdateText(currentlyStoredResourcesDictionary[resourceTypeToEject], allNeededResourcesDictionnary[resourceTypeToEject]);

        GameManager.gameManager.GetHumanSpawningManager.UpdateSpawnRate();
    }

    /// <summary>
    /// Ejects a precise Resource after a short amount of time
    /// </summary>
    /// <param name="resourceTypeToEject"></param>
    /// <returns></returns>
    IEnumerator RefuseResource(ResourceType resourceTypeToEject)
    {
        yield return new WaitForSeconds(0.2f);
        EjectResource(resourceTypeToEject);
    }

    /// <summary>
    /// Ejects a precise Resource from the current construction.
    /// Spawns the right Resource prefab and eject it in a random direction with random force.
    /// </summary>
    /// <param name="resourceTypeToEject"></param>
    public void EjectResource(ResourceType resourceTypeToEject)
    {
        //Get the right prefabs
        PhysicalObjectResourceScript prefab = GameManager.gameManager.ResourcesManager.GetResourcePrefab(resourceTypeToEject);

        if (prefab == null)
            return;

        //Generate a random velocity
        Vector3 randomThrowVelocity = Random.onUnitSphere;
        randomThrowVelocity.y = Mathf.Abs(randomThrowVelocity.y);
        randomThrowVelocity = Vector3.Slerp(randomThrowVelocity, Vector3.up, 0.5f);

        //Spawns the resource object and throw it
        PhysicalObjectResourceScript newResourceObject = Instantiate(prefab, ejectionPosition.position + randomThrowVelocity * ejectionDistanceFromCenter, Quaternion.identity);
        newResourceObject.transform.parent = GameManager.gameManager.GetAllGameObjectsParent;
        newResourceObject.SetUp();

        newResourceObject.SetCantBePlacedOnGiantConstruction(false);
        newResourceObject.Throw(randomThrowVelocity * Random.Range(minimumEjectionForce, maximumEjectionForce));

        //Play some feedback
        constructionAnimator.SetTrigger("removeResource");
    }

    #endregion


    #region Damages Management

    /// <summary>
    /// Inflict a certain amount of damages to the construction, and check if a Resource should therefore be ejected 
    /// </summary>
    /// <param name="damagesAmount"></param>
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


    #region Construction Over Management

    /// <summary>
    /// Updates the rendering fill amount depending on the completion coefficient.
    /// </summary>
    public void UpdateRenderer()
    {
        constructionRenderer.material.SetFloat("_Fill", GetCompletionCoeff);
        constructionGhostRenderer.material.SetFloat("_Fill", GetCompletionCoeff);
    }

    /// <summary>
    /// Browse every currently stored Resource, and checks if count matches the needed count for each value
    /// </summary>
    /// <returns></returns>
    public bool CheckIfConstructionFinished()
    {
        foreach(ResourceType neededResourceType in allNeededResourcesDictionnary.Keys)
        {
            if (currentlyStoredResourcesDictionary.ContainsKey(neededResourceType))
            {
                if(currentlyStoredResourcesDictionary[neededResourceType] < allNeededResourcesDictionnary[neededResourceType])
                    return false;
            }
            else return false;
        }

        return true;
    }

    /// <summary>
    /// Called once all Resources have been gathered. Triggers the game final state, and plays some feedbacks
    /// </summary>
    public void FinishConstruction()
    {
        GameManager.gameManager.OnGameWin?.Invoke();
        constructionFinished = true;
        constructionAnimator.SetBool("won", true);
        fireworksParent.SetActive(true);

        if (winSound != null)
            winSound.Play();
    }

    #endregion
}