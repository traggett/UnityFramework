using UnityEditor;

namespace Framework
{
	using Serialization;

	namespace TimelineSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(EventEditorGUI), true)]
			public class EventEditorGUIInspector : SerializedObjectEditorGUIInspector<Event>
			{
			}
		}
	}
}