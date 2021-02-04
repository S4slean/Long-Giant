using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A UI element displaying information about Resources count on a line, with image, current count and needed count
/// </summary>
public class ResourceSingleInformationsUI : MonoBehaviour
{
    [SerializeField] TextMesh resourceText = default;
    [SerializeField] List<SpriteRenderer> resourceSpriteRenderers = default;
    Sprite resourceSprite = default;
    string typeName = "";

    /// <summary>
    /// Sets up the line by setting up the display name and sprite for the resource
    /// </summary>
    /// <param name="displayInformations"></param>
    public void SetUp(ResourceDisplayInformations displayInformations)
    {
        resourceSprite = displayInformations.sprite;

        foreach(SpriteRenderer resourceSpriteRenderer in resourceSpriteRenderers)
            resourceSpriteRenderer.sprite = resourceSprite;

        typeName = displayInformations.name;
    }

    /// <summary>
    /// Updates the Resources display counts (current / needed)
    /// </summary>
    /// <param name="currentValue"></param>
    /// <param name="maxValue"></param>
    public void UpdateText(int currentValue, int maxValue)
    {
        resourceText.text = currentValue + "/" + maxValue;
    }
}

/// <summary>
/// Binds a Resource type with a display name and a sprite
/// </summary>
[System.Serializable]
public struct ResourceDisplayInformations
{
    public ResourceType type;
    public string name;
    public Sprite sprite;
}