using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicGradient))]
			public class DynamicGradientPropertyDrawer : DynamicValuePropertyDrawer<Gradient>
			{
			}
		}
	}
}