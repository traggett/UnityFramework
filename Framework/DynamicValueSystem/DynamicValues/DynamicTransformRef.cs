using System;
using UnityEngine;

namespace Framework
{
	namespace DynamicValueSystem
	{
		[Serializable]
		public class DynamicTransformRef : DynamicValue<Transform>
		{
			public static implicit operator DynamicTransformRef(Transform value)
			{
				DynamicTransformRef dynamicValue = new DynamicTransformRef();
				dynamicValue = value;
				return dynamicValue;
			}
		}
	}
}