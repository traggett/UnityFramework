using System;

namespace Framework
{
	using Maths;

	namespace DynamicValueSystem
	{
		[Serializable]
		public class DynamicIntRange : DynamicValue<IntRange>
		{
			public static implicit operator DynamicIntRange(IntRange value)
			{
				DynamicIntRange dynamicValue = new DynamicIntRange();
				dynamicValue._value = value;
				return dynamicValue;
			}
		}
	}
}