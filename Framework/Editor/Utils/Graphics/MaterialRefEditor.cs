using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
	using Serialization;

	namespace Utils
	{
		namespace Editor
		{
			[SerializedObjectEditor(typeof(MaterialRef), "PropertyField")]
			public static class MaterialRefEditor
			{
				private enum eEdtiorType
				{
					RendererMaterialInstance,
					UIGraphicMaterialInstance,
					SharedMaterial,
				}

				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
				{
					MaterialRef materialRef = (MaterialRef)obj;

					if (label == null)
						label = new GUIContent();

					label.text += " (" + materialRef + ")";

					bool editorCollapsed = !EditorGUILayout.Foldout(!materialRef._editorCollapsed, label);

					if (editorCollapsed != materialRef._editorCollapsed)
					{
						materialRef._editorCollapsed = editorCollapsed;
						dataChanged = true;
					}
					
					if (!editorCollapsed)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						eEdtiorType editorType = eEdtiorType.RendererMaterialInstance;

						if (materialRef.GetMaterialIndex() == -1)
							editorType = eEdtiorType.SharedMaterial;
						else if (materialRef.GetMaterialIndex() == MaterialRef.kGraphicMaterialIndex)
							editorType = eEdtiorType.UIGraphicMaterialInstance;
					
						//Draw type dropdown
						{
							EditorGUI.BeginChangeCheck();
							editorType = (eEdtiorType)EditorGUILayout.EnumPopup("Material Type", editorType);

							if (EditorGUI.EndChangeCheck())
							{
								dataChanged = true;

								int materialIndex = -1;

								if (editorType == eEdtiorType.RendererMaterialInstance)
									materialIndex = 0;
								else if (editorType == eEdtiorType.UIGraphicMaterialInstance)
									materialIndex = MaterialRef.kGraphicMaterialIndex;

								materialRef = new MaterialRef(materialIndex);
							}
						}

						//Draw renderer field
						if (editorType == eEdtiorType.RendererMaterialInstance)
						{
							bool renderChanged = false;
							ComponentRef<Renderer> rendererComponentRef = SerializationEditorGUILayout.ObjectField(materialRef.GetRenderer(), new GUIContent("Renderer"), ref renderChanged);

							if (renderChanged)
							{
								dataChanged = true;
								materialRef = new MaterialRef(rendererComponentRef, 0);
							}

							//Show drop down for materials
							Renderer renderer = rendererComponentRef.GetComponent();

							if (renderer != null)
							{
								string[] materialNames = new string[renderer.sharedMaterials.Length];

								for (int i = 0; i < materialNames.Length; i++)
								{
									if (renderer.sharedMaterials[i] != null)
										materialNames[i] = renderer.sharedMaterials[i].name;
									else
										materialNames[i] = "(None)";
								}

								EditorGUI.BeginChangeCheck();
								int materialIndex = EditorGUILayout.Popup("Material", materialRef.GetMaterialIndex(), materialNames);
								if (EditorGUI.EndChangeCheck())
								{
									dataChanged = true;
									materialRef = new MaterialRef(rendererComponentRef, materialIndex);
								}
							}
						}
						//Graphic
						else if (editorType == eEdtiorType.UIGraphicMaterialInstance)
						{
							bool graphicChanged = false;
							ComponentRef<Graphic> graphicComponentRef = SerializationEditorGUILayout.ObjectField(materialRef.GetGraphic(), new GUIContent("Graphic"), ref graphicChanged);

							if (graphicChanged)
							{
								dataChanged = true;
								materialRef = new MaterialRef(graphicComponentRef);
							}
						}
						//Shader material
						else
						{
							bool assetChanged = false;
							AssetRef<Material> assetRef = SerializationEditorGUILayout.ObjectField(materialRef.GetAsset(), new GUIContent("Material"), ref assetChanged);

							if (assetChanged)
							{
								dataChanged = true;
								materialRef = new MaterialRef(assetRef);
							}
						}

						EditorGUI.indentLevel = origIndent;
					}


					return materialRef;
				}
				#endregion

			}
		}
	}
}