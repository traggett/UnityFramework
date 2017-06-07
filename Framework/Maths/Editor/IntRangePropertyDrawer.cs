using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace Maths
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(IntRange))]
			public class IntRangePropertyDrawer : RangePropertyDrawer<int>
			{
			}
		}
	}
}