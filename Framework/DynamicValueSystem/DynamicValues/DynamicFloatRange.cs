using System;

namespace Framework
{
	using Maths;

	namespace DynamicValueSystem
	{
		[Serializable]
		public class DynamicFloatRange : DynamicValue<FloatRange>
		{
			public static implicit operator DynamicFloatRange(FloatRange value)
			{
				DynamicFloatRange DynamicValue = new DynamicFloatRange();
				DynamicValue._value = value;
				return DynamicValue;
			}
		}
	}
}