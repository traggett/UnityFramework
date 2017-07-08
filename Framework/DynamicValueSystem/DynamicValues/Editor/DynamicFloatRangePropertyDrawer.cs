using UnityEditor;

namespace Framework
{
	using Maths;

	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicFloatRange))]
			public class DynamicFloatRangePropertyDrawer : DynamicValuePropertyDrawer<FloatRange>
			{
			}
		}
	}
}