using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceSingleInformationsUI : MonoBehaviour
{
    [SerializeField] TextMesh resourceText = default;
    [SerializeField] List<SpriteRenderer> resourceSpriteRenderers = default;
    Sprite resourceSprite = default;
    string typeName = "";

    public void SetUp(ResourceDisplayInformations displayInformations)
    {
        resourceSprite = displayInformations.sprite;

        foreach(SpriteRenderer resourceSpriteRenderer in resourceSpriteRenderers)
            resourceSpriteRenderer.sprite = resourceSprite;

        typeName = displayInformations.name;
    }

    public void UpdateText(int currentValue, int maxValue)
    {
        resourceText.text = typeName + " : " + currentValue + "/" + maxValue;
    }
}

[System.Serializable]
public struct ResourceDisplayInformations
{
    public ResourceType type;
    public string name;
    public Sprite sprite;
}