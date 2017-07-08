using System;
using UnityEngine;

namespace Framework
{
	namespace DynamicValueSystem
	{
		[Serializable]
		public class DynamicQuaternion : DynamicValue<Quaternion>
		{
			public static implicit operator DynamicQuaternion(Quaternion value)
			{
				DynamicQuaternion dynamicValue = new DynamicQuaternion();
				dynamicValue = value;
				return dynamicValue;
			}
		}
	}
}