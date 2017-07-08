using System;
using UnityEngine;

namespace Framework
{
	namespace DynamicValueSystem
	{
		[Serializable]
		public class DynamicGradient : DynamicValue<Gradient>
		{
			public static implicit operator DynamicGradient(Gradient value)
			{
				DynamicGradient dynamicValue = new DynamicGradient();
				dynamicValue = value;
				return dynamicValue;
			}
		}
	}
}