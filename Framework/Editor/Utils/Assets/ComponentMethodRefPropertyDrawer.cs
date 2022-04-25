using System;
using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(ComponentMethodRef<>), true)]
			public class ComponentMethodRefPropertyDrawer : PropertyDrawer
			{
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);

					Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
					property.isExpanded = EditorGUI.Foldout(foldoutPosition, property.isExpanded, new GUIContent(label.text + " ("+ GetComponentType().Name + " Reference)"), true);

					if (property.isExpanded)
					{
						EditorGUI.indentLevel++;



						EditorGUI.indentLevel--;
					}
					
					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					return property.isExpanded ? EditorGUIUtility.singleLineHeight * 3f : EditorGUIUtility.singleLineHeight;
				}

				private Type GetComponentType()
				{
					return fieldInfo.FieldType.GenericTypeArguments[0];
				}
			}
		}
	}
}