using System;

namespace Framework
{
	namespace ValueSourceSystem
	{
		[Serializable]
		public class FloatValueSource : ValueSource<float>
		{
			public static implicit operator FloatValueSource(float value)
			{
				FloatValueSource valueSource = new FloatValueSource();
				valueSource._value = value;
				return valueSource;
			}
		}
	}
}