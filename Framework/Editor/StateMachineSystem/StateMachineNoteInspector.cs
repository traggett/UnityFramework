using UnityEditor;

namespace Framework
{
	namespace StateMachineSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(StateMachineNote), true)]
			public sealed class StateMachineNoteInspector : UnityEditor.Editor
			{
				public override void OnInspectorGUI()
				{
					EditorGUILayout.LabelField("Note");

					EditorGUILayout.Separator();


					SerializedProperty text = serializedObject.FindProperty("_text");

					text.stringValue = EditorGUILayout.TextArea(text.stringValue);

					serializedObject.ApplyModifiedProperties();
				}
			}
		}
	}
}