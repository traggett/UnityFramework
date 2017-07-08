using System;

namespace Framework
{
	namespace DynamicValueSystem
	{
		[Serializable]
		public class DynamicFloat : DynamicValue<float>
		{
			public static implicit operator DynamicFloat(float value)
			{
				DynamicFloat DynamicValue = new DynamicFloat();
				DynamicValue._value = value;
				return DynamicValue;
			}
		}
	}
}