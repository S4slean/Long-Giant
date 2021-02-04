using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple non-trigger collider used to receive the Humans projectiles.
/// </summary>
public class GiantConstructionSolidColliderScript : MonoBehaviour
{
    [SerializeField] GiantConstructionScript giantConstruction = default;
    public GiantConstructionScript GetGiantConstruction { get { return giantConstruction; } }
}
