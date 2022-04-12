using UnityEditor;

namespace Framework
{
	using Serialization;

	namespace StateMachineSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(StateEditorGUI), true)]
			public sealed class StateEditorGUIInspector : SerializedObjectEditorGUIInspector<State>
			{
			}
		}
	}
}