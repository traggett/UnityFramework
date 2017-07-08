using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicGameObjectRef))]
			public class DynamicGameObjectRefPropertyDrawer : DynamicValuePropertyDrawer<GameObject>
			{
			}
		}
	}
}