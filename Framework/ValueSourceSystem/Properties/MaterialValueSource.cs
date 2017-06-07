using System;
using UnityEngine;

namespace Framework
{
	namespace ValueSourceSystem
	{
		[Serializable]
		public class MaterialValueSource : ValueSource<Material>
		{
			public static implicit operator MaterialValueSource(Material value)
			{
				MaterialValueSource valueSource = new MaterialValueSource();
				valueSource._value = value;
				return valueSource;
			}
		}
	}
}