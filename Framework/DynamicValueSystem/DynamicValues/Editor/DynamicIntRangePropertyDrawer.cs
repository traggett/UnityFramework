using UnityEditor;

namespace Framework
{
	using Maths;

	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicIntRange))]
			public class DynamicIntRangePropertyDrawer : DynamicValuePropertyDrawer<IntRange>
			{
			}
		}
	}
}