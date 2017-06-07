using System;
using UnityEngine;

namespace Framework
{
	namespace ValueSourceSystem
	{
		[Serializable]
		public class TextureValueSource : ValueSource<Texture>
		{
			public static implicit operator TextureValueSource(Texture value)
			{
				TextureValueSource valueSource = new TextureValueSource();
				valueSource._value = value;
				return valueSource;
			}
		}
	}
}