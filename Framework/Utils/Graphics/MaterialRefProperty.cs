using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Framework
{
	namespace Utils
	{
		[Serializable]
		public struct MaterialRefProperty
		{
			#region Serialized Data
			[SerializeField]
			private Material _material;
			[SerializeField]
			private int _materialIndex;
			[SerializeField]
			private Renderer _renderer;
			[SerializeField]
			private Graphic _graphic;
			#endregion

			public MaterialRefProperty(Material material = null, int materialIndex = 0, Renderer renderer = null, Graphic graphic = null)
			{
				_material = material;
				_materialIndex = materialIndex;
				_renderer = renderer;
				_graphic = graphic;
			}

			public static implicit operator Material(MaterialRefProperty property)
			{
				return property.GetMaterial();
			}

			public static implicit operator MaterialRefProperty(Material value)
			{
				return new MaterialRefProperty(value);
			}

			public Material GetMaterial()
			{
				//If material is null...
				if (_material == null)
				{
					//...get from UI graphic
					if (_materialIndex == MaterialRef.kGraphicMaterialIndex)
					{
						if (_graphic != null)
						{
#if UNITY_EDITOR
							if (!Application.isPlaying)
							{
								//Debug.LogError("Trying to instantiate a material in the editor, if you want to modify a material in editor use a shared material instead.");
								//return null;
								return _graphic.materialForRendering;
							}
#endif

							//Text Mesh Pro Graphics need to use fontSharedMaterial grr
							if (_graphic is TMP_Text textMeshPro)
							{
								//Make instance of this material
								if (textMeshPro.fontSharedMaterial != null)
								{
									textMeshPro.fontSharedMaterial = new Material(textMeshPro.fontSharedMaterial);
									textMeshPro.fontSharedMaterial.name = textMeshPro.fontSharedMaterial.name + " (Instance)";
									_material = textMeshPro.fontSharedMaterial;
								}

								return null;
							}

							//Make instance of this material
							if (_graphic.material != null)
							{
								_graphic.material = new Material(_graphic.material);
								_graphic.material.name = _graphic.material.name + " (Instance)";
								_material = _graphic.material;
							}
						}
					}
					//...get from renderer / index
					else if (_materialIndex != -1)
					{
						int numMaterials = _renderer != null ? _renderer.sharedMaterials.Length : 0;

						if (_renderer != null && 0 <= _materialIndex && _materialIndex < numMaterials)
						{
#if UNITY_EDITOR
							if (!Application.isPlaying)
							{
								//Debug.LogError("Trying to instantiate a material in the editor, if you want to modify a material in editor use a shared material instead.");
								//return null;
								return _renderer.sharedMaterials[_materialIndex];
							}
#endif
						
							_material = _renderer.materials[_materialIndex];
						}
					}
				}

				return _material;
			}
		}
	}
}