using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalObjectResourceScript : PhysicalObjectScript
{
    [Header("Resource Parameters")]
    [SerializeField] ResourceType resourceType = ResourceType.Wood;

    bool canBePlacedOnGiantConstruction;
    public void SetCantBePlacedOnGiantConstruction(bool canBePlaced)
    {
        canBePlacedOnGiantConstruction = canBePlaced;
    }

    public override bool CheckForDestroy(float speedForce)
    {
        canBePlacedOnGiantConstruction = true;
        return base.CheckForDestroy(speedForce);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (canBePlacedOnGiantConstruction)
        {
            GiantConstructionScript giantConstruction = other.GetComponent<GiantConstructionScript>();
            if (giantConstruction != null)
            {
                giantConstruction.AddResourceToConstruction(resourceType);
                if (spawnOnDestroy != null)
                    Instantiate(spawnOnDestroy, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }

    [Header("Rendering")]
    [SerializeField] GameObject spawnOnDestroy = default;
}

public enum ResourceType
{
    Wood, Stone, Iron
}

[System.Serializable]
public struct ResourceWithQuantity
{
    public ResourceType resourceType;
    public int quantity;
}