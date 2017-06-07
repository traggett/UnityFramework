using System;

namespace Framework
{
	using Maths;

	namespace ValueSourceSystem
	{
		[Serializable]
		public class FloatRangeValueSource : ValueSource<FloatRange>
		{
			public static implicit operator FloatRangeValueSource(FloatRange value)
			{
				FloatRangeValueSource valueSource = new FloatRangeValueSource();
				valueSource._value = value;
				return valueSource;
			}
		}
	}
}