using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourcesManager 
{
    [Header("All Prefabs")]
    [SerializeField] List<ResourcePrefabWithType> allResourcesPrefabs = new List<ResourcePrefabWithType>();

    Dictionary<ResourceType, PhysicalObjectResourceScript> resourcesPrefabsDictionnary = new Dictionary<ResourceType, PhysicalObjectResourceScript>();
    public void SetUpResourcesDictionnary()
    {
        resourcesPrefabsDictionnary = new Dictionary<ResourceType, PhysicalObjectResourceScript>();
        foreach (ResourcePrefabWithType prefabWithType in allResourcesPrefabs)
            resourcesPrefabsDictionnary.Add(prefabWithType.resourceType, prefabWithType.prefab);
    }

    public PhysicalObjectResourceScript GetResourcePrefab(ResourceType resourceType)
    {
        if (resourcesPrefabsDictionnary.ContainsKey(resourceType))
            return resourcesPrefabsDictionnary[resourceType];
        else 
            return null;
    }
}

[System.Serializable]
public struct ResourcePrefabWithType
{
    public ResourceType resourceType;
    public PhysicalObjectResourceScript prefab;
}