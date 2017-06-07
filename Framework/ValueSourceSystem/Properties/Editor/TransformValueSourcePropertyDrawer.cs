using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(TransformValueSource))]
			public class TransformValueSourcePropertyDrawer : ValueSourcePropertyDrawer<Transform>
			{
			}
		}
	}
}