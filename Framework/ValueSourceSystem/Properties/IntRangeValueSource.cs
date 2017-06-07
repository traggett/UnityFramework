using System;

namespace Framework
{
	using Maths;

	namespace ValueSourceSystem
	{
		[Serializable]
		public class IntRangeValueSource : ValueSource<IntRange>
		{
			public static implicit operator IntRangeValueSource(IntRange value)
			{
				IntRangeValueSource valueSource = new IntRangeValueSource();
				valueSource._value = value;
				return valueSource;
			}
		}
	}
}