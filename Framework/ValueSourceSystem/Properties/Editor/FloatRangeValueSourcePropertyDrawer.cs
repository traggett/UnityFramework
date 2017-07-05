using UnityEditor;

namespace Framework
{
	using Maths;

	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(FloatRangeValueSource))]
			public class FloatRangeValueSourcePropertyDrawer : ValueSourcePropertyDrawer<FloatRange>
			{
			}
		}
	}
}