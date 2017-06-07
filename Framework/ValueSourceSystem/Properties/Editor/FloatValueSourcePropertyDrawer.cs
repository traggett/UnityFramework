using UnityEditor;

namespace Framework
{
	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(FloatValueSource))]
			public class FloatValueSourcePropertyDrawer : ValueSourcePropertyDrawer<float>
			{
			}
		}
	}
}