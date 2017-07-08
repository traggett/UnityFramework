using System;
using UnityEngine;

namespace Framework
{
	namespace DynamicValueSystem
	{
		[Serializable]
		public class DynamicVector4 : DynamicValue<Vector4>
		{
			public static implicit operator DynamicVector4(Vector4 value)
			{
				DynamicVector4 dynamicValue = new DynamicVector4();
				dynamicValue = value;
				return dynamicValue;
			}
		}
	}
}