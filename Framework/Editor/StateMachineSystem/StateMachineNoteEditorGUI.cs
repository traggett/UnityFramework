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