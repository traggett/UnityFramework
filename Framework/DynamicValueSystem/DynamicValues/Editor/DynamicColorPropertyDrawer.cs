using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicColor))]
			public class DynamicColorPropertyDrawer : DynamicValuePropertyDrawer<Color>
			{
			}
		}
	}
}