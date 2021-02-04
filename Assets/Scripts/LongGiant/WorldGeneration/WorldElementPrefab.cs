using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple prefab containing a list of Constructions, and holding their references in order to process the total amount of Resources it contains.
/// </summary>
public class WorldElementPrefab : MonoBehaviour
{
    /// <summary>
    /// The list of Constructions gathered on this prefab.
    /// </summary>
    [Header("References")]
    [Tooltip("The list of Constructions gathered on this prefab.")]
    [SerializeField] PhysicalObjectConstructionScript[] allElements = new PhysicalObjectConstructionScript[0];
    public PhysicalObjectConstructionScript[] GetAllElements { get { return allElements; } }

    /// <summary>
    /// Automaticaly gets the references to the Construction contained on this prefab.
    /// </summary>
    [ContextMenu("AutoAssignChildren")]
    public void AutoAssignChildren()
    {
        allElements = new PhysicalObjectConstructionScript[0];

        allElements = GetComponentsInChildren<PhysicalObjectConstructionScript>();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    /// <summary>
    /// Calls Set Up on every Construction of this prefab.
    /// </summary>
    public void SetUpAllChildrenElements()
    {
        foreach (PhysicalObjectConstructionScript element in allElements)
                element.SetUp();
    }
}
