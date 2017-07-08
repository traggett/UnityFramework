using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicAnimationCurve))]
			public class DynamicAnimationCurvePropertyDrawer : DynamicValuePropertyDrawer<AnimationCurve>
			{
			}
		}
	}
}