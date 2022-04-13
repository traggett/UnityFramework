using System;
using UnityEngine;

namespace Framework
{
	namespace Graphics
	{
		[Serializable]
		public struct InstancedMaterial
		{
			#region Serialized Data
			[SerializeField]
			private Material _material;
			private Material _instancedMaterial;
			#endregion

			public InstancedMaterial(Material material = null)
			{
				_material = material;
				_instancedMaterial = null;
			}

			public static implicit operator Material(InstancedMaterial property)
			{
				return property.GetMaterial();
			}

			public static implicit operator InstancedMaterial(Material value)
			{
				return new InstancedMaterial(value);
			}

			public Material GetMaterial()
			{
				if (_instancedMaterial == null && _material != null)
				{
					_instancedMaterial = new Material(_material);
				}

				return _instancedMaterial;
			}
		}
	}
}