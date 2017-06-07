using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(FloatRangeValueSource))]
			public class FloatRangeValueSourcePropertyDrawer : ValueSourcePropertyDrawer<Color>
			{
			}
		}
	}
}