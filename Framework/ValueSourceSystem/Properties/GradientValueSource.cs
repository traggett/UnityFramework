using System;
using UnityEngine;

namespace Framework
{
	namespace ValueSourceSystem
	{
		[Serializable]
		public class GradientValueSource : ValueSource<Gradient>
		{
			public static implicit operator GradientValueSource(Gradient value)
			{
				GradientValueSource valueSource = new GradientValueSource();
				valueSource._value = value;
				return valueSource;
			}
		}
	}
}