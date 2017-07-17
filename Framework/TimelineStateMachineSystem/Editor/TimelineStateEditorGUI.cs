namespace Framework
{
	using StateMachineSystem.Editor;
	using UnityEngine;

	namespace TimelineStateMachineSystem
	{
		namespace Editor
		{
			[StateCustomEditorGUI(typeof(TimelineState))]
			public class TimeLineStateEditorGUI : StateEditorGUI
			{
				#region Public Interface			
				public override void OnDoubleClick()
				{
					StateMachineEditor editor = (StateMachineEditor)GetEditor();
					editor.SwitchToTimelineStateView(GetStateId());
				}

				protected override string GetStateIdLabel()
				{
					return "Timeline (State" + GetStateId().ToString("00") +")";
				}

				public override bool RenderObjectProperties(GUIContent label)
				{
					bool dataChanged = false;

					dataChanged |= RenderStateDescriptionField();
					dataChanged |= RenderStateColorField();

					if (GUILayout.Button("Edit"))
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