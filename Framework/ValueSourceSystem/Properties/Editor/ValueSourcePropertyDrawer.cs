using UnityEngine;
using UnityEditor;
using System;

namespace Framework
{
	using Utils;
	using System.Collections.Generic;
	using System.Reflection;
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
					SerializedProperty sourceObjectMemberProp = property.FindPropertyRelative("_sourceObjectMember");

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

						ValueSource<T>.eEdtiorType sourceType = (ValueSource<T>.eEdtiorType)editorTypeProperty.intValue;
						bool tempOverrideType = sourceType == ValueSource<T>.eEdtiorType.Static && EditorUtils.GetDraggingComponent<IValueSource<T>>() != null;
						if (tempOverrideType)
						{
							sourceType = ValueSource<T>.eEdtiorType.Source;
						}

						EditorGUI.BeginChangeCheck();

						Rect typePosition = new Rect(position.x, position.y + editorHeightProp.floatValue, position.width, EditorGUIUtility.singleLineHeight);
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
									editorHeightProp.floatValue += DrawSourceObjectField(sourceObjectProp, sourceObjectMemberProp, valuePosition, editorTypeProperty);
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


				private float DrawSourceObjectField(SerializedProperty sourceObjectProp, SerializedProperty sourceObjectMemberProp, Rect valuePosition, SerializedProperty editorTypeProperty)
				{
					Component currentComponent = sourceObjectProp.objectReferenceValue as Component;

					float height;
					Component selectedComponent = EditorUtils.ComponentField<MonoBehaviour>(new GUIContent("Source Object"), valuePosition, currentComponent, out height);

					if (currentComponent != selectedComponent)
					{
						sourceObjectProp.objectReferenceValue = selectedComponent;
						editorTypeProperty.intValue = Convert.ToInt32(ValueSource<T>.eEdtiorType.Source);
					}

					valuePosition.y += height;

					if (currentComponent != null)
					{
						height += DrawObjectDropDown(currentComponent, valuePosition, sourceObjectMemberProp);
					}

					return height;
				}

				private float DrawObjectDropDown(object obj, Rect valuePosition, SerializedProperty sourceObjectMemberProp)
				{
					List<GUIContent> fieldNames = new List<GUIContent>();
					List<FieldInfo> fieldInfos = new List<FieldInfo>();

					int index = 0;

					//If the object itself is an IValueSource<T> then it can be selected
					if (SystemUtils.IsTypeOf(typeof(IValueSource<T>), obj.GetType()))
					{
						fieldNames.Add(new GUIContent(".this"));
						fieldInfos.Add(null);

						if (string.IsNullOrEmpty(sourceObjectMemberProp.stringValue))
							index = fieldInfos.Count - 1;
					}

					//Otherwise can select any of its fields if they are IValueSource<T> 
					FieldInfo[] fields = ValueSource<T>.GetValueSourceFields(obj);

					foreach (FieldInfo field in fields)
					{
						fieldNames.Add(new GUIContent("." + field.Name));
						fieldInfos.Add(field);

						if (sourceObjectMemberProp.stringValue == field.Name)
							index = fieldInfos.Count - 1;
					}
					
					//Warn if there are no valid options for the object
					if (fieldInfos.Count == 0)
					{
						fieldNames.Add(new GUIContent("No valid IValueSource<" + typeof(T).Name + ">" + " member!"));
						fieldInfos.Add(null);
					}

					index = EditorGUI.Popup(valuePosition, new GUIContent("Component Member"), index, fieldNames.ToArray());

					if (fieldInfos[index] == null)
					{
						sourceObjectMemberProp.stringValue = null;
					}
					else
					{
						sourceObjectMemberProp.stringValue = fieldInfos[index].Name;
					}

					return EditorGUIUtility.singleLineHeight;
				}
			}
		}
	}
}