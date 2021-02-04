using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalObjectResourceScript : PhysicalObjectScript
{
    [Header("Resource Parameters")]
    [SerializeField] ResourceType resourceType = ResourceType.Wood;

    [Header("Rendering")]
    [SerializeField] GameObject spawnOnDestroy = default;

    /// <summary>
    /// Used to prevent Resource to be instantly trigger back the GiantConstruction when it gets ejected from it
    /// </summary>
    bool canBePlacedOnGiantConstruction;

    /// <summary>
    /// Callback used to check if the Resource enters the GiantConstriuction Trigger zone
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (canBePlacedOnGiantConstruction)
        {
            //Check if hit trigger is the Giant Construction
            GiantConstructionScript giantConstruction = other.GetComponent<GiantConstructionScript>();
            if (giantConstruction != null)
            {
                //Try to add it to the construction, plays a feedback and destroy the object
                giantConstruction.AddResourceToConstruction(resourceType);
                if (spawnOnDestroy != null)
                    Instantiate(spawnOnDestroy, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Sets if the object can be placed on Giant Construction - Set this to false when Resource gets ejected from the construction.
    /// </summary>
    /// <param name="canBePlaced"></param>
    public void SetCantBePlacedOnGiantConstruction(bool canBePlaced)
    {
        canBePlacedOnGiantConstruction = canBePlaced;
    }

    /// <summary>
    /// We override CheckForDestroy to allow object to be placed again on the giant construction if when it hits something
    /// </summary>
    /// <param name="speedForce"></param>
    /// <returns></returns>
    public override bool CheckForDestroy(float speedForce)
    {
        canBePlacedOnGiantConstruction = true;
        return base.CheckForDestroy(speedForce);
    }    
}

/// <summary>
/// Defines the Resource Type
/// </summary>
public enum ResourceType
{
    Wood, Stone, Iron
}

/// <summary>
/// Binds a Resource with a specific quantity
/// </summary>
[System.Serializable]
public struct ResourceWithQuantity
{
    public ResourceType resourceType;
    public int quantity;
}