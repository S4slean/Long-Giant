
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Drawer used to associate a Resource type with a display name and sprite
/// </summary>
[CustomPropertyDrawer(typeof(ResourceDisplayInformations))]
public class ResourcesDisplayInformationsDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 1.2f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float spaceBewteen = 10f;
        float thirdWidth = (position.width - spaceBewteen) * 0.3f;

        SerializedProperty typeProperty = property.FindPropertyRelative("type");
        SerializedProperty prefabProperty = property.FindPropertyRelative("name");
        SerializedProperty spriteProperty = property.FindPropertyRelative("sprite");

        Rect typeRect = new Rect(position.x, position.y, thirdWidth, EditorGUIUtility.singleLineHeight);
        Rect nameRect = new Rect(position.x + thirdWidth + spaceBewteen, position.y, thirdWidth, EditorGUIUtility.singleLineHeight);
        Rect spriteRect = new Rect(position.x + (thirdWidth + spaceBewteen) * 2, position.y, thirdWidth, EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(typeRect, typeProperty, GUIContent.none);
        EditorGUI.PropertyField(nameRect, prefabProperty, GUIContent.none);
        EditorGUI.PropertyField(spriteRect, spriteProperty, GUIContent.none);
    }
}
