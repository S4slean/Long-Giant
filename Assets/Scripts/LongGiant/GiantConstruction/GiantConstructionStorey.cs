using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Construction Storey", menuName = "Long Giant/Construction/Storey")]
public class GiantConstructionStorey : ScriptableObject
{
    [Header("Needed Resources")]
    public List<ResourceWithQuantity> neededResources = new List<ResourceWithQuantity>();
    public Mesh meshToPlace = default;
}
