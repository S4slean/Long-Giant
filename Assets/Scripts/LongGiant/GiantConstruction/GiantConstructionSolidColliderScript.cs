using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantConstructionSolidColliderScript : MonoBehaviour
{
    [SerializeField] GiantConstructionScript giantConstruction = default;
    public GiantConstructionScript GetGiantConstruction { get { return giantConstruction; } }
}
