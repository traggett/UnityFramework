using System;

namespace Framework
{
	namespace ValueSourceSystem
	{
		[Serializable]
		public class StringValueSource : ValueSource<string>
		{
			public static implicit operator StringValueSource(string value)
			{
				StringValueSource valueSource = new StringValueSource();
				valueSource._value = value;
				return valueSource;
			}
		}
	}
}