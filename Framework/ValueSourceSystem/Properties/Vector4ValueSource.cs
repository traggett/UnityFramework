using System;
using UnityEngine;

namespace Framework
{
	namespace ValueSourceSystem
	{
		[Serializable]
		public class Vector4ValueSource : ValueSource<Vector4>
		{
			public static implicit operator Vector4ValueSource(Vector4 value)
			{
				Vector4ValueSource valueSource = new Vector4ValueSource();
				valueSource._value = value;
				return valueSource;
			}
		}
	}
}