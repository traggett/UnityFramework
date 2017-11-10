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
		}
	}
}