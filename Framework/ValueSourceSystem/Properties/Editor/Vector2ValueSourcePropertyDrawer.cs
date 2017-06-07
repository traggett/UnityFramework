using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(Vector2ValueSource))]
			public class Vector2ValueSourcePropertyDrawer : ValueSourcePropertyDrawer<Vector2>
			{
			}
		}
	}
}