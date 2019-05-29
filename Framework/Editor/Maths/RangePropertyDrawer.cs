using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace Maths
	{
		namespace Editor
		{
			public class RangePropertyDrawer<T> : PropertyDrawer where T : struct
			{
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);

					SerializedProperty minProperty = property.FindPropertyRelative("_min");
					
					Rect labelPosition = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
					EditorGUI.LabelField(labelPosition, label);

					Rect minLabelPosition = new Rect(labelPosition.x + labelPosition.width, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
					EditorGUI.MultiPropertyField(minLabelPosition, new GUIContent[] { new GUIContent("f"), new GUIContent("t") }, minProperty);

					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					return EditorGUIUtility.singleLineHeight;
				}
			}
		}
	}
}