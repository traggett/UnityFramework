using UnityEditor;

namespace Framework
{
	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicBool))]
			public class DynamicBoolPropertyDrawer : DynamicValuePropertyDrawer<bool>
			{
			}
		}
	}
}