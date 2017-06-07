using UnityEditor;

namespace Framework
{
	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(IntValueSource))]
			public class IntValueSourcePropertyDrawer : ValueSourcePropertyDrawer<int>
			{
			}
		}
	}
}