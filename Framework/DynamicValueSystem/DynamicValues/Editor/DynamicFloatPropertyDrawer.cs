using UnityEditor;

namespace Framework
{
	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicFloat))]
			public class DynamicFloatPropertyDrawer : DynamicValuePropertyDrawer<float>
			{
			}
		}
	}
}