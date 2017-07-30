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
				private enum eEdtiorType
				{
					Instanced,
					Shared,
				}

				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);
					
					SerializedProperty materialProperty = property.FindPropertyRelative("_material");
					SerializedProperty materialIndexProp = property.FindPropertyRelative("_materialIndex");
					SerializedProperty rendererProp = property.FindPropertyRelative("_renderer");

					float yPos = position.y;
					Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

					property.isExpanded = EditorGUI.Foldout(foldoutPosition, property.isExpanded, property.displayName);
					yPos += EditorGUIUtility.singleLineHeight;

					if (property.isExpanded)
					{
						eEdtiorType editorType = materialIndexProp.intValue != -1 ? eEdtiorType.Instanced : eEdtiorType.Shared;

						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						//Draw type dropdown
						{
							Rect typePosition = new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight);

							EditorGUI.BeginChangeCheck();
							editorType = (eEdtiorType)EditorGUI.EnumPopup(typePosition, "Material Type", editorType);
							yPos += EditorGUIUtility.singleLineHeight;

							if (EditorGUI.EndChangeCheck())
							{
								switch (editorType)
								{
									case eEdtiorType.Instanced:
										{
											materialProperty.objectReferenceValue = null;
											materialIndexProp.intValue = 0;
											Component component = property.serializedObject.targetObject as Component;
											rendererProp.objectReferenceValue = component != null ? component.GetComponent<Renderer>() : null;
										}
										break;
									case eEdtiorType.Shared:
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
						if (editorType == eEdtiorType.Instanced)
						{
							Rect rendererPosition = new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight);

							EditorGUI.BeginChangeCheck();
							EditorGUI.PropertyField(rendererPosition, rendererProp, new GUIContent("Renderer"));
							yPos += EditorGUIUtility.singleLineHeight;
							if (EditorGUI.EndChangeCheck())
							{
								materialProperty.objectReferenceValue = null;
								materialIndexProp.intValue = 0;
							}

							//Show drop down for materials
							Renderer renderer = rendererProp.objectReferenceValue as Renderer;

							if (renderer != null)
							{
								Rect valuePosition = new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight);
								string[] materialNames = new string[renderer.sharedMaterials.Length];

								for (int i=0; i<materialNames.Length; i++)
								{
									materialNames[i] = renderer.sharedMaterials[i].name;
								}
								
								materialIndexProp.intValue = EditorGUI.Popup(valuePosition, "Material", materialIndexProp.intValue, materialNames);
								yPos += EditorGUIUtility.singleLineHeight;
							}
						}
						else
						{
							Rect valuePosition = new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight);
							EditorGUI.PropertyField(valuePosition, materialProperty, new GUIContent("Material"));
							yPos += EditorGUIUtility.singleLineHeight;
						}			

						EditorGUI.indentLevel = origIndent;
					}
					
					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					if (property.isExpanded)
					{
						SerializedProperty materialIndexProp = property.FindPropertyRelative("_materialIndex");
						eEdtiorType editorType = materialIndexProp.intValue != -1 ? eEdtiorType.Instanced : eEdtiorType.Shared;

						if (editorType == eEdtiorType.Instanced)
						{
							SerializedProperty rendererProp = property.FindPropertyRelative("_renderer");

							if (rendererProp.objectReferenceValue as Renderer != null)
								return EditorGUIUtility.singleLineHeight * 4;
							else
								return EditorGUIUtility.singleLineHeight * 3;
						}
						else
						{
							return EditorGUIUtility.singleLineHeight * 3;
						}
					}

					return EditorGUIUtility.singleLineHeight;
				}
			}
		}
	}
}