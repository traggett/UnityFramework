using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicQuaternion))]
			public class DynamicQuaternionPropertyDrawer : DynamicValuePropertyDrawer<Quaternion>
			{
			}
		}
	}
}