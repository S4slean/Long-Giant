using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager that holds references to all of the game's Resources' prefabs
/// </summary>
[System.Serializable]
public class ResourcesManager 
{
    [Header("All Prefabs")]
    [SerializeField] List<ResourcePrefabWithType> allResourcesPrefabs = new List<ResourcePrefabWithType>();

    Dictionary<ResourceType, PhysicalObjectResourceScript> resourcesPrefabsDictionnary = new Dictionary<ResourceType, PhysicalObjectResourceScript>();
    
    /// <summary>
    /// Initialize the dictionary with the assigned resources prefabs
    /// </summary>
    public void SetUpResourcesDictionnary()
    {
        resourcesPrefabsDictionnary = new Dictionary<ResourceType, PhysicalObjectResourceScript>();
        foreach (ResourcePrefabWithType prefabWithType in allResourcesPrefabs)
            resourcesPrefabsDictionnary.Add(prefabWithType.resourceType, prefabWithType.prefab);
    }

    /// <summary>
    /// Returns the prefab linked to a specific Resource type
    /// </summary>
    /// <param name="resourceType"></param>
    /// <returns></returns>
    public PhysicalObjectResourceScript GetResourcePrefab(ResourceType resourceType)
    {
        if (resourcesPrefabsDictionnary.ContainsKey(resourceType))
            return resourcesPrefabsDictionnary[resourceType];
        else 
            return null;
    }
}

/// <summary>
/// Bindds a Resource type to a Resource Prefab
/// </summary>
[System.Serializable]
public struct ResourcePrefabWithType
{
    public ResourceType resourceType;
    public PhysicalObjectResourceScript prefab;
}