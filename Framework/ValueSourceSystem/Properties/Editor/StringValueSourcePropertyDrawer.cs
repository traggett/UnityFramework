using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(StringValueSource))]
			public class StringValueSourcePropertyDrawer : ValueSourcePropertyDrawer<string>
			{
			}
		}
	}
}