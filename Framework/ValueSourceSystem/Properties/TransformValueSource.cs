using System;
using UnityEngine;

namespace Framework
{
	namespace ValueSourceSystem
	{
		[Serializable]
		public class TransformValueSource : ValueSource<Transform>
		{
			public static implicit operator TransformValueSource(Transform value)
			{
				TransformValueSource valueSource = new TransformValueSource();
				valueSource._value = value;
				return valueSource;
			}
		}
	}
}