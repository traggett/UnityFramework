using System;

namespace Framework
{
	namespace DynamicValueSystem
	{
		[Serializable]
		public class DynamicInt : DynamicValue<int>
		{
			public static implicit operator DynamicInt(int value)
			{
				DynamicInt dynamicValue = new DynamicInt();
				dynamicValue = value;
				return dynamicValue;
			}
		}
	}
}