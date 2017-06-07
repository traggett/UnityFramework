using UnityEditor;

namespace Framework
{
	namespace Maths
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(FloatRange))]
			public class FloatRangeDrawer : RangePropertyDrawer<float>
			{
			}
		}
	}
}