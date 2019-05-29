using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicVector3))]
			public class DynamicVector3PropertyDrawer : DynamicValuePropertyDrawer<Vector3>
			{
			}
		}
	}
}