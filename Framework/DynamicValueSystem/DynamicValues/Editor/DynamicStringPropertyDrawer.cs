using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicString))]
			public class DynamicStringPropertyDrawer : DynamicValuePropertyDrawer<string>
			{
			}
		}
	}
}