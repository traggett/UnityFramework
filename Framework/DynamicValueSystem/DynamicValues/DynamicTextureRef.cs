using System;
using UnityEngine;

namespace Framework
{
	namespace DynamicValueSystem
	{
		[Serializable]
		public class DynamicTextureRef : DynamicValue<Texture>
		{
			public static implicit operator DynamicTextureRef(Texture value)
			{
				DynamicTextureRef dynamicValue = new DynamicTextureRef();
				dynamicValue = value;
				return dynamicValue;
			}
		}
	}
}