using System;
using UnityEngine;

namespace Framework
{
	namespace DynamicValueSystem
	{
		[Serializable]
		public class DynamicVector2 : DynamicValue<Vector2>
		{
			public static implicit operator DynamicVector2(Vector2 value)
			{
				DynamicVector2 dynamicValue = new DynamicVector2();
				dynamicValue._value = value;
				return dynamicValue;
			}
		}
	}
}