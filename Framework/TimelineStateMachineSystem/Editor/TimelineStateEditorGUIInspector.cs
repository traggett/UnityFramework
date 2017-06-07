using UnityEditor;

namespace Framework
{
	using Serialization;

	namespace TimelineStateMachineSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(TimelineStateEditorGUI))]
			public sealed class TimelineStateEditorGUIInspector : SerializedObjectEditorGUIInspector<TimelineState>
			{
			}
		}
	}
}