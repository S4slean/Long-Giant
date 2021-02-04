using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A scriptable object that holds all the Resources information of the game, with type, display name and sprite
/// </summary>
[CreateAssetMenu(fileName = "Resources Informations Library", menuName = "Long Giant/UI/Resources Informations Library")]
public class ResourcesInformationsLibrary : ScriptableObject
{
    public List<ResourceDisplayInformations> resourceDisplayInformations = new List<ResourceDisplayInformations>();
}
