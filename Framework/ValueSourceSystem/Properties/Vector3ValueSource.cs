using System;
using UnityEngine;

namespace Framework
{
	namespace ValueSourceSystem
	{
		[Serializable]
		public class Vector3ValueSource : ValueSource<Vector3>
		{
			public static implicit operator Vector3ValueSource(Vector3 value)
			{
				Vector3ValueSource valueSource = new Vector3ValueSource();
				valueSource._value = value;
				return valueSource;
			}
		}
	}
}