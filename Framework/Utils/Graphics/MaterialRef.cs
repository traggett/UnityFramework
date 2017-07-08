using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using Serialization;

	namespace Utils
	{
		[Serializable]
		public sealed class MaterialRef : ICustomEditorInspector
		{
			#region Public Data
			[SerializeField]
			private AssetRef<Material> _materialRef;
			[SerializeField]
			private int _materialIndex;
			[SerializeField]
			private ComponentRef<Renderer> _renderer;
			#endregion

			#region Private Data
#if UNITY_EDITOR
			private enum eEdtiorType
			{
				Instance,
				Shared,
			}
			private eEdtiorType _editorType;
			private bool _editorFoldout = true;
#endif
			#endregion

			private Material _material;

			public static implicit operator string(MaterialRef property)
			{
				return property._materialRef;
			}

			public static implicit operator Material(MaterialRef property)
			{
				return property.GetMaterial();
			}

			public Material GetMaterial()
			{
				if (_material == null)
				{
					if (_materialIndex != -1)
					{
						Renderer renderer = _renderer.GetComponent();

						if (renderer != null && 0 <= _materialIndex && _materialIndex < renderer.sharedMaterials.Length)
						{
							_material = renderer.materials[_materialIndex];
						}
					}
					else
					{
						_material = _materialRef.LoadAsset();
					}
				}

				return _material;
			}

			#region ICustomEditable
#if UNITY_EDITOR
			public bool RenderObjectProperties(GUIContent label)
			{
				bool dataChanged = false;

				if (label == null)
					label = new GUIContent();

				label.text += " (" + this + ")";

				_editorFoldout = EditorGUILayout.Foldout(_editorFoldout, label);
				if (_editorFoldout)
				{
					int origIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel++;

					//Draw type dropdown
					{
						EditorGUI.BeginChangeCheck();
						_editorType = (eEdtiorType)EditorGUILayout.EnumPopup("Material Type", _editorType);
						
						if (EditorGUI.EndChangeCheck())
						{
							dataChanged = true;
							_materialRef.ClearAsset();
							_materialIndex = -1;
							_renderer.ClearComponent();
						}
					}

					//Draw renderer field
					if (_editorType == eEdtiorType.Instance)
					{
						bool renderChanged = false;
						_renderer = SerializationEditorGUILayout.ObjectField(_renderer, new GUIContent("Renderer"), ref renderChanged);

						if (renderChanged)
						{
							dataChanged = true;
							_materialRef.ClearAsset();
							_materialIndex = 0;
						}

						//Show drop down for materials
						Renderer renderer = _renderer.GetEditorComponent();

						if (renderer != null)
						{
							string[] materialNames = new string[renderer.sharedMaterials.Length];

							for (int i = 0; i < materialNames.Length; i++)
							{
								materialNames[i] = renderer.sharedMaterials[i].name;
							}

							_materialIndex = EditorGUILayout.Popup("Material", _materialIndex, materialNames);
						}
					}
					else
					{
						_materialRef = SerializationEditorGUILayout.ObjectField(_materialRef, new GUIContent("Material"), ref dataChanged);
					}
					
					EditorGUI.indentLevel = origIndent;
				}

				return dataChanged;
			}
#endif
			#endregion
		}
	}
}