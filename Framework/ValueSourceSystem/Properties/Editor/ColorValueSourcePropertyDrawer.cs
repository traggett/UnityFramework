using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(ColorValueSource))]
			public class ColorValueSourcePropertyDrawer : ValueSourcePropertyDrawer<Color>
			{
			}
		}
	}
}