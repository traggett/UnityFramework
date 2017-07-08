using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicBool))]
			public class DynamicBoolValuePropertyDrawer : DynamicValuePropertyDrawer<bool>
			{
			}
		}
	}
}