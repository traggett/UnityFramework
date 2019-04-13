using System;
using UnityEngine;
using UnityEngine.UI;

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
						if (_graphic != null && _graphic.material != null)
						{

#if UNITY_EDITOR
							if (!Application.isPlaying)
							{
								Debug.LogError("Trying to instantiate a material in the editor, if you want to modify a material in editor use a shared material instead.");
								return null;
							}
#endif
							//Make instance of this material
							_graphic.material = new Material(_graphic.material);
							_graphic.material.name = _graphic.material.name + " (Instance)";
							_material = _graphic.material;
						}
					}
					//...get from renderer / index
					else if (_materialIndex != -1)
					{
						if (_renderer != null && 0 <= _materialIndex && _materialIndex < _renderer.sharedMaterials.Length)
						{
#if UNITY_EDITOR
							if (!Application.isPlaying)
							{
								Debug.LogError("Trying to instantiate a material in the editor, if you want to modify a material in editor use a shared material instead.");
								return null;
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