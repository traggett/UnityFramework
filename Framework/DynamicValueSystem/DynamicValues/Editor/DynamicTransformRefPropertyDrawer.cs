using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicTransformRef))]
			public class DynamicTransformRefPropertyDrawer : DynamicValuePropertyDrawer<Transform>
			{
			}
		}
	}
}