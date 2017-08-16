using System;
using UnityEngine;

namespace Framework
{

	namespace Utils
	{
		[Serializable]
		public struct MaterialRef
		{
			#region Serialized Data
			[SerializeField]
			private AssetRef<Material> _materialRef;
			[SerializeField]
			private int _materialIndex;
			[SerializeField]
			private ComponentRef<Renderer> _renderer;
#if UNITY_EDITOR
			[NonSerialized]
			public bool _editorCollapsed;
#endif
			#endregion

			#region Private Data
			private Material _material;
			#endregion

#if UNITY_EDITOR
			public MaterialRef(int materialIndex)
			{
				_materialRef = null;
				_materialIndex = materialIndex;
				_renderer = new ComponentRef<Renderer>();
				_editorCollapsed = false;
				_material = null;
			}

			public MaterialRef(ComponentRef<Renderer> renderer, int materialIndex)
			{
				_materialRef = null;
				_materialIndex = materialIndex;
				_renderer = renderer;
				_editorCollapsed = false;
				_material = null;
			}

			public MaterialRef(AssetRef<Material> materialRef)
			{
				_materialRef = materialRef;
				_materialIndex = -1;
				_renderer = new ComponentRef<Renderer>();
				_editorCollapsed = false;
				_material = null;
			}
#endif

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

#if UNITY_EDITOR
			public int GetMaterialIndex()
			{
				return _materialIndex;
			}

			public ComponentRef<Renderer> GetRenderer()
			{
				return _renderer;
			}

			public AssetRef<Material> GetAsset()
			{
				return _materialRef;
			}
#endif
		}
	}
}