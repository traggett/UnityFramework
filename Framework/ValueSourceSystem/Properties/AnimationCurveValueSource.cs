using System;
using UnityEngine;

namespace Framework
{
	namespace ValueSourceSystem
	{
		[Serializable]
		public class AnimationCurveValueSource : ValueSource<AnimationCurve>
		{
			public static implicit operator AnimationCurveValueSource(AnimationCurve value)
			{
				AnimationCurveValueSource valueSource = new AnimationCurveValueSource();
				valueSource._value = value;
				return valueSource;
			}
		}
	}
}