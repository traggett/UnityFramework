using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(Vector3ValueSource))]
			public class Vector3ValueSourcePropertyDrawer : ValueSourcePropertyDrawer<Vector3>
			{
			}
		}
	}
}