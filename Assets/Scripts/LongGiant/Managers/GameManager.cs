using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;

    private void Awake()
    {
        gameManager = this;
        resourcesManager.SetUpResourcesDictionnary();
        SetUpResourcesDisplayInformationsLibrary();

        if (giantConstruction != null)
            giantConstruction.SetUp();

        humanSpawningManager.SetUp();
    }

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
    [SerializeField] GiantConstructionScript giantConstruction = default;
    public GiantConstructionScript GetGiantConstruction { get { return giantConstruction; } }
    public void SetGiantConstruction(GiantConstructionScript newConstruction)
    {
        giantConstruction = newConstruction;
    }

    [SerializeField] Transform allGameObjectsParent = default;
    public Transform GetAllGameObjectsParent { get { return allGameObjectsParent; } }

    [SerializeField] ResourcesInformationsLibrary resourcesInformationsLibrary = default;
    Dictionary<ResourceType, ResourceDisplayInformations> resourcesDisplayInformationsLibrary = new Dictionary<ResourceType, ResourceDisplayInformations>();

    public void SetUpResourcesDisplayInformationsLibrary()
    {
        resourcesDisplayInformationsLibrary = new Dictionary<ResourceType, ResourceDisplayInformations>();
        foreach (ResourceDisplayInformations infos in resourcesInformationsLibrary.resourceDisplayInformations)
        {
            if (!resourcesDisplayInformationsLibrary.ContainsKey(infos.type))
                resourcesDisplayInformationsLibrary.Add(infos.type, infos);
        }
    }

    public ResourceDisplayInformations GetResourceDisplayInformations(ResourceType type)
    {
        ResourceDisplayInformations infos = default;
        if (resourcesDisplayInformationsLibrary.ContainsKey(type))
            infos = resourcesDisplayInformationsLibrary[type];

        return infos;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (giantConstruction != null)
                giantConstruction.ReceiveDamages(10);
        }

        humanSpawningManager.UpdateSpawningSystem();
    }

    private void LateUpdate()
    {
        collisionsManager.TreatRequests();
    }
}
