using System;

namespace Framework
{
	namespace DynamicValueSystem
	{
		[Serializable]
		public class DynamicBool : DynamicValue<bool>
		{
			public static implicit operator DynamicBool(bool value)
			{
				DynamicBool dynamicValue = new DynamicBool();
				dynamicValue._value = value;
				return dynamicValue;
			}
		}
	}
}