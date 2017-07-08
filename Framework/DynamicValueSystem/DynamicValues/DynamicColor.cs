using System;
using UnityEngine;

namespace Framework
{
	namespace DynamicValueSystem
	{
		[Serializable]
		public class DynamicColor : DynamicValue<Color>
		{
			public static implicit operator DynamicColor(Color value)
			{
				DynamicColor dynamicValue = new DynamicColor();
				dynamicValue = value;
				return dynamicValue;
			}
		}
	}
}