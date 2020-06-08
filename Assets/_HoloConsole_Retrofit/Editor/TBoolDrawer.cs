using GeometricDrag;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TBool))]
class TBoolDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var field = property.FindPropertyRelative("_value");
        field.intValue = EditorGUI.Toggle(position, label, field.intValue != 0) ? 1 : 0;
    }
}
#endif
