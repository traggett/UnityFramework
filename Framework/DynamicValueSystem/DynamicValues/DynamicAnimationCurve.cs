using System;
using UnityEngine;

namespace Framework
{
	namespace DynamicValueSystem
	{
		[Serializable]
		public class DynamicAnimationCurve : DynamicValue<AnimationCurve>
		{
			public static implicit operator DynamicAnimationCurve(AnimationCurve value)
			{
				DynamicAnimationCurve dynamicValue = new DynamicAnimationCurve();
				dynamicValue._value = value;
				return dynamicValue;
			}
		}
	}
}