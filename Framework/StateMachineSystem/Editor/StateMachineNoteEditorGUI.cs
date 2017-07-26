using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Utils.Editor;

	namespace StateMachineSystem
	{
		namespace Editor
		{
			[StateCustomEditorGUI(typeof(StateMachineNote))]
			public class StateMachineNoteEditorGUI : StateEditorGUI
			{
				public override bool RenderObjectProperties(GUIContent label)
				{
					bool dataChanged = false;

					dataChanged |= RenderStateColorField("Note Color");

					EditorGUILayout.Separator();

					Color orig = GUI.backgroundColor;
					GUI.backgroundColor = _titleLabelColor;
					EditorGUILayout.LabelField("<b>Note:</b>", EditorUtils.TextTitleStyle, GUILayout.Height(24.0f));
					GUI.backgroundColor = orig;

					EditorGUILayout.Separator();

					EditorGUI.BeginChangeCheck();
					GetEditableObject()._editorDescription = EditorGUILayout.TextArea(GetEditableObject()._editorDescription);
					dataChanged |= EditorGUI.EndChangeCheck();

					return dataChanged;
				}

				public override Color GetColor(StateMachineEditorStyle style)
				{
					if (GetEditableObject()._editorAutoColor)
						return style._noteColor;
					else
						return GetEditableObject()._editorColor;
				}

				public override float GetBorderSize(bool selected)
				{
					return selected ? 1.0f : 0.0f;
				}

				public override GUIStyle GetTextStyle(StateMachineEditorStyle style)
				{
					return style._noteTextStyle;
				}

				public override bool IsCentred()
				{
					return false;
				}

				public override int GetStateId()
				{
					return -1;
				}
			}
		}
	}
}