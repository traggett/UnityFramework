using System;

namespace Framework
{
	namespace DynamicValueSystem
	{
		[Serializable]
		public class DynamicString : DynamicValue<string>
		{
			public static implicit operator DynamicString(string value)
			{
				DynamicString dynamicValue = new DynamicString();
				dynamicValue._value = value;
				return dynamicValue;
			}
		}
	}
}