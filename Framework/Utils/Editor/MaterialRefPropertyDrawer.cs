using UnityEngine;
using UnityEditor;
using System;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(MaterialRefProperty))]
			public class MaterialRefPropertyDrawer : PropertyDrawer
			{
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);
					
					SerializedProperty materialProperty = property.FindPropertyRelative("_material");
					SerializedProperty materialIndexProp = property.FindPropertyRelative("_materialIndex");
					SerializedProperty rendererProp = property.FindPropertyRelative("_renderer");

					SerializedProperty editorTypeProperty = property.FindPropertyRelative("_editorType");
					SerializedProperty editorFoldoutProp = property.FindPropertyRelative("_editorFoldout");
					SerializedProperty editorHeightProp = property.FindPropertyRelative("_editorHeight");


					Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

					editorFoldoutProp.boolValue = EditorGUI.Foldout(foldoutPosition, editorFoldoutProp.boolValue, property.displayName);
					editorHeightProp.floatValue = EditorGUIUtility.singleLineHeight;

					if (editorFoldoutProp.boolValue)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						//Draw type dropdown
						{
							Rect typePosition = new Rect(position.x, position.y + editorHeightProp.floatValue, position.width, EditorGUIUtility.singleLineHeight);

							EditorGUI.BeginChangeCheck();
							editorTypeProperty.enumValueIndex = Convert.ToInt32(EditorGUI.EnumPopup(typePosition, "Material Type", (MaterialRefProperty.eEdtiorType)editorTypeProperty.enumValueIndex));
							editorHeightProp.floatValue += EditorGUIUtility.singleLineHeight;

							if (EditorGUI.EndChangeCheck())
							{
								switch ((MaterialRefProperty.eEdtiorType)editorTypeProperty.enumValueIndex)
								{
									case MaterialRefProperty.eEdtiorType.Instance:
										{
											materialProperty.objectReferenceValue = null;
											materialIndexProp.intValue = 0;
											Component component = property.serializedObject.targetObject as Component;
											rendererProp.objectReferenceValue = component != null ? component.GetComponent<Renderer>() : null;
										}
										break;
									case MaterialRefProperty.eEdtiorType.Shared:
										{
											materialProperty.objectReferenceValue = null;
											materialIndexProp.intValue = -1;
											rendererProp.objectReferenceValue = null;
										}
										break;
								}
							}
						}


						//Draw renderer field
						if ((MaterialRefProperty.eEdtiorType)editorTypeProperty.enumValueIndex == MaterialRefProperty.eEdtiorType.Instance)
						{
							Rect rendererPosition = new Rect(position.x, position.y + editorHeightProp.floatValue, position.width, EditorGUIUtility.singleLineHeight);

							EditorGUI.BeginChangeCheck();
							EditorGUI.PropertyField(rendererPosition, rendererProp, new GUIContent("Renderer"));
							editorHeightProp.floatValue += EditorGUIUtility.singleLineHeight;
							if (EditorGUI.EndChangeCheck())
							{
								materialProperty.objectReferenceValue = null;
								materialIndexProp.intValue = 0;
							}

							//Show drop down for materials
							Renderer renderer = rendererProp.objectReferenceValue as Renderer;

							if (renderer != null)
							{
								Rect valuePosition = new Rect(position.x, position.y + editorHeightProp.floatValue, position.width, EditorGUIUtility.singleLineHeight);
								string[] materialNames = new string[renderer.sharedMaterials.Length];

								for (int i=0; i<materialNames.Length; i++)
								{
									materialNames[i] = renderer.sharedMaterials[i].name;
								}
								
								materialIndexProp.intValue = EditorGUI.Popup(valuePosition, "Material", materialIndexProp.intValue, materialNames);
								editorHeightProp.floatValue += EditorGUIUtility.singleLineHeight;
							}
						}
						else
						{
							Rect valuePosition = new Rect(position.x, position.y + editorHeightProp.floatValue, position.width, EditorGUIUtility.singleLineHeight);
							EditorGUI.PropertyField(valuePosition, materialProperty, new GUIContent("Material"));
							editorHeightProp.floatValue += EditorGUIUtility.singleLineHeight;
						}			

						EditorGUI.indentLevel = origIndent;
					}
					
					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					SerializedProperty editorHeightProp = property.FindPropertyRelative("_editorHeight");
					if (editorHeightProp != null)
						return editorHeightProp.floatValue;
					return 0.0f;
				}
			}
		}
	}
}