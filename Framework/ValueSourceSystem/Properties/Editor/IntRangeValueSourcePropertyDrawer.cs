using UnityEditor;

namespace Framework
{
	using Maths;

	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(IntRangeValueSource))]
			public class IntRangeValueSourcePropertyDrawer : ValueSourcePropertyDrawer<IntRange>
			{
			}
		}
	}
}