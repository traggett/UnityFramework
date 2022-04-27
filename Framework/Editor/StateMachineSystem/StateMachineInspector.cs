using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace StateMachineSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(StateMachine), true)]
			public sealed class StateMachineInspector : UnityEditor.Editor
			{
				public override void OnInspectorGUI()
				{
					if (GUILayout.Button("Edit State Machine"))
					{
						StateMachineEditorWindow.Open((StateMachine)target);
					}
				}
			}
		}
	}
}