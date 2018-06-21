using System;
using UnityEngine;

namespace Framework
{
	namespace DynamicValueSystem
	{
		[Serializable]
		public class DynamicGameObjectRef : DynamicValue<GameObject>
		{
			public static implicit operator DynamicGameObjectRef(GameObject value)
			{
				DynamicGameObjectRef dynamicValue = new DynamicGameObjectRef();
				dynamicValue._value = value;
				return dynamicValue;
			}
		}
	}
}