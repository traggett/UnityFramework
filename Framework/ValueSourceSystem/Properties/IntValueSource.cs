using System;

namespace Framework
{
	namespace ValueSourceSystem
	{
		[Serializable]
		public class IntValueSource : ValueSource<int>
		{
			public static implicit operator IntValueSource(int value)
			{
				IntValueSource valueSource = new IntValueSource();
				valueSource._value = value;
				return valueSource;
			}
		}
	}
}