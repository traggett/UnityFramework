using UnityEditor;

namespace Framework
{
	using Serialization;

	namespace TimelineStateMachineSystem
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