using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

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
					RendererMaterialInstance,
					UIGraphicMaterialInstance,
					SharedMaterial,
				}

				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);
					
					SerializedProperty materialProperty = property.FindPropertyRelative("_material");
					SerializedProperty materialIndexProp = property.FindPropertyRelative("_materialIndex");
					SerializedProperty rendererProp = property.FindPropertyRelative("_renderer");
					SerializedProperty graphicProp = property.FindPropertyRelative("_graphic");

					float yPos = position.y;
					Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

					property.isExpanded = EditorGUI.Foldout(foldoutPosition, property.isExpanded, property.displayName);
					yPos += EditorGUIUtility.singleLineHeight;

					if (property.isExpanded)
					{
						eEdtiorType editorType = eEdtiorType.RendererMaterialInstance;

						if (materialIndexProp.intValue == -1)
							editorType = eEdtiorType.SharedMaterial;
						else if (materialIndexProp.intValue == MaterialRef.kGraphicMaterialIndex)
							editorType = eEdtiorType.UIGraphicMaterialInstance;

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
									case eEdtiorType.RendererMaterialInstance:
										{
											materialProperty.objectReferenceValue = null;
											materialIndexProp.intValue = 0;
											//Try to default to renderer component on this object.
											Component component = property.serializedObject.targetObject as Component;
											rendererProp.objectReferenceValue = component != null ? component.GetComponent<Renderer>() : null;
											graphicProp.objectReferenceValue = null;
										}
										break;
									case eEdtiorType.UIGraphicMaterialInstance:
										{
											materialProperty.objectReferenceValue = null;
											materialIndexProp.intValue = MaterialRef.kGraphicMaterialIndex;
											rendererProp.objectReferenceValue = null;
											//Try to default to graphic component on this object.
											Component component = property.serializedObject.targetObject as Component;
											graphicProp.objectReferenceValue = component != null ? component.GetComponent<Graphic>() : null;
										}
										break;
									case eEdtiorType.SharedMaterial:
										{
											materialProperty.objectReferenceValue = null;
											materialIndexProp.intValue = -1;
											rendererProp.objectReferenceValue = null;
											graphicProp.objectReferenceValue = null;
										}
										break;
								}
							}
						}


						//Draw renderer field
						if (editorType == eEdtiorType.RendererMaterialInstance)
						{
							Rect rendererPosition = new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight);

							EditorGUI.BeginChangeCheck();
							EditorGUI.PropertyField(rendererPosition, rendererProp, new GUIContent("Renderer"));
							yPos += EditorGUIUtility.singleLineHeight;
							if (EditorGUI.EndChangeCheck())
							{
								materialProperty.objectReferenceValue = null;
								materialIndexProp.intValue = 0;
								graphicProp.objectReferenceValue = null;
							}

							//Show drop down for materials
							Renderer renderer = rendererProp.objectReferenceValue as Renderer;

							if (renderer != null)
							{
								Rect valuePosition = new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight);
								string[] materialNames = new string[renderer.sharedMaterials.Length];

								for (int i=0; i<materialNames.Length; i++)
								{
									if (renderer.sharedMaterials[i] != null)
										materialNames[i] = renderer.sharedMaterials[i].name;
									else
										materialNames[i] = "(None)";
								}
								
								materialIndexProp.intValue = EditorGUI.Popup(valuePosition, "Material", materialIndexProp.intValue, materialNames);
								yPos += EditorGUIUtility.singleLineHeight;
							}
						}
						//Draw graphic field
						else if(editorType == eEdtiorType.UIGraphicMaterialInstance)
						{
							Rect rendererPosition = new Rect(position.x, yPos, position.width, EditorGUIUtility.singleLineHeight);

							EditorGUI.BeginChangeCheck();
							EditorGUI.PropertyField(rendererPosition, graphicProp, new GUIContent("Graphic"));
							yPos += EditorGUIUtility.singleLineHeight;
							if (EditorGUI.EndChangeCheck())
							{
								materialProperty.objectReferenceValue = null;
								materialIndexProp.intValue = MaterialRef.kGraphicMaterialIndex;
								rendererProp.objectReferenceValue = null;
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
						eEdtiorType editorType = eEdtiorType.RendererMaterialInstance;

						if (materialIndexProp.intValue == -1)
							editorType = eEdtiorType.SharedMaterial;
						else if (materialIndexProp.intValue == MaterialRef.kGraphicMaterialIndex)
							editorType = eEdtiorType.UIGraphicMaterialInstance;

						if (editorType == eEdtiorType.RendererMaterialInstance)
						{
							SerializedProperty rendererProp = property.FindPropertyRelative("_renderer");

							if (rendererProp.objectReferenceValue as Renderer != null)
								return EditorGUIUtility.singleLineHeight * 4;
							else
								return EditorGUIUtility.singleLineHeight * 3;
						}
						else if (editorType == eEdtiorType.RendererMaterialInstance)
						{
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