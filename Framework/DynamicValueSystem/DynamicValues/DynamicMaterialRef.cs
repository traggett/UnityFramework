using System;

namespace Framework
{
	using UnityEngine;
	using Utils;

	namespace DynamicValueSystem
	{
		[Serializable]
		public class DynamicMaterialRef : DynamicValue<MaterialRefProperty>
		{
			public Material GetMaterial()
			{
				return _value.GetMaterial();
			}

			public static DynamicMaterialRef StaticMaterialRef(Material material)
			{
				DynamicMaterialRef dynamicMaterial = new DynamicMaterialRef
				{
					_value = new MaterialRefProperty(material)
				};

				return dynamicMaterial;
			}

			public static DynamicMaterialRef InstancedMaterialRef(Renderer renderer, int materialIndex = 0)
			{
				DynamicMaterialRef dynamicMaterial = new DynamicMaterialRef();

				if (renderer != null)
				{
					dynamicMaterial._value = new MaterialRefProperty(renderer.materials[materialIndex], materialIndex, renderer);
				}

				return dynamicMaterial;
			}
		}
	}
}