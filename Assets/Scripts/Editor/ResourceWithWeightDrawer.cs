using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Drawer used to associate a Resource with a certain probability weight
/// </summary>
[CustomPropertyDrawer(typeof(ResourceWithWeight))]
public class ResourceWithWeightDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        float spaceBewteen = 10f;
        float midWidth = (position.width - spaceBewteen) * 0.5f;

        SerializedProperty typeProperty = property.FindPropertyRelative("resourceType");
        SerializedProperty weightProperty = property.FindPropertyRelative("weight");

        Rect typeRect = new Rect(position.x, position.y, midWidth, EditorGUIUtility.singleLineHeight);
        Rect weightRect = new Rect(position.x + midWidth + spaceBewteen, position.y, midWidth, EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(typeRect, typeProperty, GUIContent.none);
        EditorGUI.PropertyField(weightRect, weightProperty, GUIContent.none);
    }
}