
using UnityEditor;
using UnityEngine;

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
					Instance,
					Shared,
				}

				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
				{
					MaterialRef materialRef = (MaterialRef)obj;

					if (label == null)
						label = new GUIContent();

					label.text += " (" + materialRef + ")";

					bool editorCollapsed = EditorGUILayout.Foldout(!materialRef._editorCollapsed, label);

					if (editorCollapsed != materialRef._editorCollapsed)
					{
						materialRef._editorCollapsed = editorCollapsed;
						dataChanged = true;
					}
					
					if (!editorCollapsed)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						eEdtiorType editorType = materialRef.GetMaterialIndex() == -1 ? eEdtiorType.Shared : eEdtiorType.Instance;


						//Draw type dropdown
						{
							EditorGUI.BeginChangeCheck();
							editorType = (eEdtiorType)EditorGUILayout.EnumPopup("Material Type", editorType);

							if (EditorGUI.EndChangeCheck())
							{
								dataChanged = true;
								materialRef = new MaterialRef(editorType == eEdtiorType.Shared ? -1 : 0);
							}
						}

						//Draw renderer field
						if (editorType == eEdtiorType.Instance)
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
									materialNames[i] = renderer.sharedMaterials[i].name;
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