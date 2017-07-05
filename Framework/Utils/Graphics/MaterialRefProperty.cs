using System;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		[Serializable]
		public struct MaterialRefProperty
		{
			[SerializeField]
			private Material _material;
			[SerializeField]
			private int _materialIndex;
			[SerializeField]
			private Renderer _renderer;

			public MaterialRefProperty(Material material = null, int materialIndex=0, Renderer renderer=null)
			{
				_material = material;
				_materialIndex = materialIndex;
				_renderer = renderer;
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
				FindMaterial();
				return _material;
			}

			private void FindMaterial()
			{
				if (_material == null && _materialIndex != -1 && _renderer != null)
				{
					if (0 <= _materialIndex && _materialIndex < _renderer.sharedMaterials.Length)
					{
						_material = _renderer.materials[_materialIndex];
					}
				}
			}
		}
	}
}