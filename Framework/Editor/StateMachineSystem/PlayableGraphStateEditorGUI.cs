using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Utils.Editor;

	namespace StateMachineSystem
	{
		namespace Editor
		{
			[StateCustomEditorGUI(typeof(PlayableGraphState))]
			public class PlayableGraphStateEditorGUI : StateEditorGUI
			{
				public override bool RenderObjectProperties(GUIContent label)
				{
					bool dataChanged = false;
					
					dataChanged |= RenderStateDescriptionField();
					dataChanged |= RenderStateColorField();

					EditorGUILayout.Separator();

					Color orig = GUI.backgroundColor;
					GUI.backgroundColor = _titleLabelColor;
					EditorGUILayout.LabelField("<b>Playable Graph:</b>", EditorUtils.InspectorSubHeaderStyle, GUILayout.Height(24.0f));
					GUI.backgroundColor = orig;

					EditorGUILayout.Separator();

					Serialization.SerializationEditorGUILayout.ObjectField(GetEditableObject(), GUIContent.none, ref dataChanged);

					return dataChanged;
				}

				public override Color GetColor(StateMachineEditorStyle style)
				{
					return style._playableGraphStateColor;
				}
			}
		}
	}
}