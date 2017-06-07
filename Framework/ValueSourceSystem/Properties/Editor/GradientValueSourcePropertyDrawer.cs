using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(GradientValueSource))]
			public class GradientValueSourcePropertyDrawer : ValueSourcePropertyDrawer<Gradient>
			{
			}
		}
	}
}