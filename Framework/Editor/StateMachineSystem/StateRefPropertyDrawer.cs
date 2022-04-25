using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace StateMachineSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(StateRef))]
			public sealed class StateRefPropertyDrawer : PropertyDrawer
			{
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					return 0f;
				}
			}
		}
	}
}