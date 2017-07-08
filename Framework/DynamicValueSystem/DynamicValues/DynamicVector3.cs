using System;
using UnityEngine;

namespace Framework
{
	namespace DynamicValueSystem
	{
		[Serializable]
		public class DynamicVector3 : DynamicValue<Vector3>
		{
			public static implicit operator DynamicVector3(Vector3 value)
			{
				DynamicVector3 dynamicValue = new DynamicVector3();
				dynamicValue = value;
				return dynamicValue;
			}
		}
	}
}