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
				dynamicValue = value;
				return dynamicValue;
			}
		}
	}
}