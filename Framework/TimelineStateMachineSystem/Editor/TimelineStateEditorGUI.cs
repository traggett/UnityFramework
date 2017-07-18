using UnityEngine;

namespace Framework
{
	using StateMachineSystem.Editor;
	
	namespace TimelineStateMachineSystem
	{
		namespace Editor
		{
			[StateCustomEditorGUI(typeof(TimelineState))]
			public class TimeLineStateEditorGUI : StateEditorGUI
			{
				#region StateEditorGUI		
				public override void OnDoubleClick()
				{
					StateMachineEditor editor = (StateMachineEditor)GetEditor();
					editor.SwitchToTimelineStateView(GetStateId());
				}

				public override bool RenderObjectProperties(GUIContent label)
				{
					bool dataChanged = false;

					dataChanged |= RenderStateDescriptionField();
					dataChanged |= RenderStateColorField();

					if (GUILayout.Button("Edit Timeline"))
					{
						StateMachineEditor timelineStateMachineEditor = (StateMachineEditor)GetEditor();
						timelineStateMachineEditor.SwitchToTimelineStateView(GetEditableObject()._stateId);
					}

					return dataChanged;
				}
				#endregion

			}
		}
	}
}