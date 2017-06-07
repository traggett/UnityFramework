using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(BoolValueSource))]
			public class BoolValueSourcePropertyDrawer : ValueSourcePropertyDrawer<bool>
			{
			}
		}
	}
}