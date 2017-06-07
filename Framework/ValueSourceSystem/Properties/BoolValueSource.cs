using System;

namespace Framework
{
	namespace ValueSourceSystem
	{
		[Serializable]
		public class BoolValueSource : ValueSource<bool>
		{
			public static implicit operator BoolValueSource(bool value)
			{
				BoolValueSource valueSource = new BoolValueSource();
				valueSource._value = value;
				return valueSource;
			}
		}
	}
}