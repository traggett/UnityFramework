using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(Vector4ValueSource))]
			public class Vector4ValueSourcePropertyDrawer : ValueSourcePropertyDrawer<Vector4>
			{
			}
		}
	}
}