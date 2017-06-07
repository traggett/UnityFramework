using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(QuaternionValueSource))]
			public class QuaternionValueSourcePropertyDrawer : ValueSourcePropertyDrawer<Quaternion>
			{
			}
		}
	}
}