using System;
using UnityEngine;

namespace Framework
{
	namespace ValueSourceSystem
	{
		[Serializable]
		public class ColorValueSource : ValueSource<Color>
		{
			public static implicit operator ColorValueSource(Color value)
			{
				ColorValueSource valueSource = new ColorValueSource();
				valueSource._value = value;
				return valueSource;
			}
		}
	}
}