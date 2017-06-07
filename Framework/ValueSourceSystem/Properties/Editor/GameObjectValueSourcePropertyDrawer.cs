using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(GameObjectValueSource))]
			public class GameObjectValueSourcePropertyDrawer : ValueSourcePropertyDrawer<GameObject>
			{
			}
		}
	}
}