using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicVector4))]
			public class DynamicVector4PropertyDrawer : DynamicValuePropertyDrawer<Vector4>
			{
			}
		}
	}
}