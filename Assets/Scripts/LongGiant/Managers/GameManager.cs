using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;

    private void Awake()
    {
        gameManager = this;
    }

    [Header("Physical Managers")]
    [SerializeField] PhysicalObjectsCollisionsManager collisionsManager = default;
     public PhysicalObjectsCollisionsManager CollisionsManager { get { return collisionsManager; } }

    private void LateUpdate()
    {
        collisionsManager.TreatRequests();
    }
}
