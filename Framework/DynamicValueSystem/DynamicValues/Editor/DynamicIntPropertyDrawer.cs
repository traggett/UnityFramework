using UnityEditor;

namespace Framework
{
	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicInt))]
			public class DynamicIntPropertyDrawer : DynamicValuePropertyDrawer<int>
			{
			}
		}
	}
}