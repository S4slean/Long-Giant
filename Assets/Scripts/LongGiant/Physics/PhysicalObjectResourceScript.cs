using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalObjectResourceScript : PhysicalObjectScript
{
    [Header("Resource Parameters")]
    [SerializeField] ResourceType resourceType = ResourceType.Wood;
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