using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ResourcePrefabWithType))]
public class ResourceWithPrefabDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //base.OnGUI(position, property, label);
        float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        float spaceBewteen = 10f;
        float midWidth = (position.width - spaceBewteen) * 0.5f;

        SerializedProperty typeProperty = property.FindPropertyRelative("resourceType");
        SerializedProperty prefabProperty = property.FindPropertyRelative("prefab");

        Rect typeRect = new Rect(position.x, position.y, midWidth, EditorGUIUtility.singleLineHeight);
        Rect prefabRect = new Rect(position.x + midWidth + spaceBewteen, position.y, midWidth, EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(typeRect, typeProperty, GUIContent.none);
        EditorGUI.PropertyField(prefabRect, prefabProperty, GUIContent.none);
    }
}
