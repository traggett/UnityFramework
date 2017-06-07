using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(AnimationCurveValueSource))]
			public class AnimationCurveValueSourcePropertyDrawer : ValueSourcePropertyDrawer<AnimationCurve>
			{
			}
		}
	}
}