using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main game manager, holding all the other managers and handling the game start and end.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    public System.Action OnGameWin;
    public bool gameFinished;


    [Header("Physical Managers")]
    [SerializeField] PhysicalObjectsCollisionsManager collisionsManager = default;
    public PhysicalObjectsCollisionsManager CollisionsManager { get { return collisionsManager; } }

    [Header("Pooling Managers")]
    [SerializeField] ResourcesManager resourcesManager = default;
    public ResourcesManager ResourcesManager { get { return resourcesManager; } }

    [Header("Other Managers")]
    [SerializeField] HumanSpawningManager humanSpawningManager = default;
    public HumanSpawningManager GetHumanSpawningManager { get { return humanSpawningManager; } }

    [SerializeField] WorldGenerationManager worldGenerationManager = default;
    public WorldGenerationManager GetWorldGenerationManager { get { return worldGenerationManager; } }


    [Header("Important References")]

    GiantConstructionScript giantConstruction = default;
    public GiantConstructionScript GetGiantConstruction { get { return giantConstruction; } }
    public void SetGiantConstruction(GiantConstructionScript newConstruction)
    {
        giantConstruction = newConstruction;
    }

    /// <summary>
    /// Holds the display information of each resource
    /// </summary>
    [SerializeField] ResourcesInformationsLibrary resourcesInformationsLibrary = default;
    Dictionary<ResourceType, ResourceDisplayInformations> resourcesDisplayInformationsLibrary = new Dictionary<ResourceType, ResourceDisplayInformations>();

    [SerializeField] Transform allGameObjectsParent = default;
    public Transform GetAllGameObjectsParent { get { return allGameObjectsParent; } }
    /// <summary>
    /// All objects of the game are placed as children of an object to handle AR - This method sets it
    /// </summary>
    /// <param name="newParent"></param>
    public void SetAllGameObjectsParent(Transform newParent)
    {
        allGameObjectsParent = newParent;
    }

    public float gameAreaRadius = 7.5f;


    #region Engine Callbacks

    private void OnEnable()
    {
        OnGameWin += SetGameFinished;
    }

    private void OnDisable()
    {
        OnGameWin -= SetGameFinished;
    }

    /// <summary>
    /// Set up important values and other managers
    /// </summary>
    private void Awake()
    {
        gameManager = this;

        resourcesManager.SetUpResourcesDictionnary();
        SetUpResourcesDisplayInformationsLibrary();

        if (giantConstruction != null)
            giantConstruction.SetUp();

        humanSpawningManager.SetUp();
    }

    /// <summary>
    /// Update important managers - Human Spawning System
    /// </summary>
    private void Update()
    {
        if (!gameFinished)
            humanSpawningManager.UpdateSpawningSystem();
    }

    /// <summary>
    /// Treat Physical Object Collisions Requests on end frame
    /// </summary>
    private void LateUpdate()
    {
        collisionsManager.TreatRequests();
    }

    #endregion


    #region Resources Display Information

    /// <summary>
    /// Sets up the Resources display information for a more quick access during runtime
    /// </summary>
    public void SetUpResourcesDisplayInformationsLibrary()
    {
        resourcesDisplayInformationsLibrary = new Dictionary<ResourceType, ResourceDisplayInformations>();
        foreach (ResourceDisplayInformations infos in resourcesInformationsLibrary.resourceDisplayInformations)
        {
            if (!resourcesDisplayInformationsLibrary.ContainsKey(infos.type))
                resourcesDisplayInformationsLibrary.Add(infos.type, infos);
        }
    }

    /// <summary>
    /// Returns display information for the inputed Resource type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public ResourceDisplayInformations GetResourceDisplayInformations(ResourceType type)
    {
        ResourceDisplayInformations infos = default;
        if (resourcesDisplayInformationsLibrary.ContainsKey(type))
            infos = resourcesDisplayInformationsLibrary[type];

        return infos;
    }

    #endregion


    /// <summary>
    /// Simply sets the gameFinished value to true.
    /// </summary>
    public void SetGameFinished()
    {
        gameFinished = true;
    }
}
