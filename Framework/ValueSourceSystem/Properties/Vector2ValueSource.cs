using System;
using UnityEngine;

namespace Framework
{
	namespace ValueSourceSystem
	{
		[Serializable]
		public class Vector2ValueSource : ValueSource<Vector2>
		{
			public static implicit operator Vector2ValueSource(Vector2 value)
			{
				Vector2ValueSource valueSource = new Vector2ValueSource();
				valueSource._value = value;
				return valueSource;
			}
		}
	}
}