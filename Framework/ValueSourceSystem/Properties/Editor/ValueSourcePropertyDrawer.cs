using UnityEngine;
using UnityEditor;
using System;

namespace Framework
{
	using Utils.Editor;

	namespace ValueSourceSystem
	{
		namespace Editor
		{
			public abstract class ValueSourcePropertyDrawer<T> : PropertyDrawer
			{
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);

					SerializedProperty valueProperty = property.FindPropertyRelative("_value");
					SerializedProperty sourceObjectProp = property.FindPropertyRelative("_sourceObject");

					SerializedProperty editorTypeProperty = property.FindPropertyRelative("_editorType");
					SerializedProperty editorFoldoutProp = property.FindPropertyRelative("_editorFoldout");
					SerializedProperty editorHeightProp = property.FindPropertyRelative("_editorHeight");

					Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

					editorFoldoutProp.boolValue = EditorGUI.Foldout(foldoutPosition, editorFoldoutProp.boolValue, label != null ? label.text : property.displayName);
					editorHeightProp.floatValue = EditorGUIUtility.singleLineHeight;

					if (editorFoldoutProp.boolValue)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						Rect typePosition = new Rect(position.x, position.y + editorHeightProp.floatValue, position.width, EditorGUIUtility.singleLineHeight);
						

						ValueSource<T>.eEdtiorType sourceType = (ValueSource<T>.eEdtiorType)editorTypeProperty.intValue;
						bool tempOverrideType = sourceType == ValueSource<T>.eEdtiorType.Static && EditorUtils.GetDraggingComponent<IValueSource<T>>() != null;
						if (tempOverrideType)
						{
							sourceType = ValueSource<T>.eEdtiorType.Source;
						}

						EditorGUI.BeginChangeCheck();

						ValueSource<T>.eEdtiorType edtiorType = (ValueSource<T>.eEdtiorType)EditorGUI.EnumPopup(typePosition, "Source Type", sourceType);
						editorHeightProp.floatValue += EditorGUIUtility.singleLineHeight;

						if (EditorGUI.EndChangeCheck())
						{
							sourceObjectProp.objectReferenceValue = null;
							editorTypeProperty.intValue = Convert.ToInt32(edtiorType);
						}

						Rect valuePosition = new Rect(position.x, position.y + editorHeightProp.floatValue, position.width, EditorGUIUtility.singleLineHeight);

						switch (sourceType)
						{
							case ValueSource<T>.eEdtiorType.Source:
								{
									Component currentComponent = sourceObjectProp.objectReferenceValue as Component;
									float height;
									Component selectedComponent = EditorUtils.ComponentField<IValueSource<T>>(new GUIContent("Value Source"), valuePosition, currentComponent, out height);
									editorHeightProp.floatValue += height;

									if (currentComponent != selectedComponent)
									{
										sourceObjectProp.objectReferenceValue = selectedComponent;
										editorTypeProperty.intValue = Convert.ToInt32(ValueSource<T>.eEdtiorType.Source);
									}
								}
								break;
							case ValueSource<T>.eEdtiorType.Static:
								{
									editorHeightProp.floatValue += DrawValueField(valuePosition, valueProperty);
								}
								break;
						}

						EditorGUI.indentLevel = origIndent;
					}

					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					SerializedProperty editorHeightProp = property.FindPropertyRelative("_editorHeight");
					return editorHeightProp.floatValue;
				}

				public virtual float DrawValueField(Rect position, SerializedProperty valueProperty)
				{
					EditorGUI.PropertyField(position, valueProperty, new GUIContent("Value"));
					return EditorGUIUtility.singleLineHeight;
				}
			}
		}
	}
}