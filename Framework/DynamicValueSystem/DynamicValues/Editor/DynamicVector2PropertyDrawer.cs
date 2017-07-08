using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicVector2))]
			public class DynamicVector2PropertyDrawer : DynamicValuePropertyDrawer<Vector2>
			{
			}
		}
	}
}