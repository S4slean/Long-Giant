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
    }

    [Header("Physical Managers")]
    [SerializeField] PhysicalObjectsCollisionsManager collisionsManager = default;
     public PhysicalObjectsCollisionsManager CollisionsManager { get { return collisionsManager; } }

    [Header("Pooling Managers")]
    [SerializeField] ResourcesManager resourcesManager = default;
    public ResourcesManager ResourcesManager { get { return resourcesManager; } }

    [Header("Important References")]
    [SerializeField] GiantConstructionScript giantConstruction = default;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            giantConstruction.ReceiveDamages(10);
        }
    }

    private void LateUpdate()
    {
        collisionsManager.TreatRequests();
    }
}
