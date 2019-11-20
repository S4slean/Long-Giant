using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Resources Informations Library", menuName = "Long Giant/UI/Resources Informations Library")]
public class ResourcesInformationsLibrary : ScriptableObject
{
    public List<ResourceDisplayInformations> resourceDisplayInformations = new List<ResourceDisplayInformations>();
}
