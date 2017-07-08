using System;
using UnityEngine;

namespace Framework
{
	namespace DynamicValueSystem
	{
		[Serializable]
		public class DynamicComponentRef : DynamicValue<Component>
		{
			public static implicit operator DynamicComponentRef(Component value)
			{
				DynamicComponentRef dynamicValue = new DynamicComponentRef();
				dynamicValue._value = value;
				return dynamicValue;
			}
		}
	}
}