using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldElementPrefab : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PhysicalObjectConstructionScript[] allElements = new PhysicalObjectConstructionScript[0];
    public PhysicalObjectConstructionScript[] GetAllElements { get { return allElements; } }

    [ContextMenu("AutoAssignChildren")]
    public void AutoAssignChildren()
    {
        allElements = new PhysicalObjectConstructionScript[0];

        allElements = GetComponentsInChildren<PhysicalObjectConstructionScript>();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public void SetUpAllChildrenElements()
    {
        foreach (PhysicalObjectConstructionScript element in allElements)
                element.SetUp();
    }
}
