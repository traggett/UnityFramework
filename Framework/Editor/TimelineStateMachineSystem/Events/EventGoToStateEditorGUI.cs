using UnityEngine;

namespace Framework
{
	using StateMachineSystem.Editor;
	using TimelineSystem.Editor;

	namespace TimelineStateMachineSystem
	{
		namespace Editor
		{
			[EventCustomEditorGUI(typeof(EventGoToState))]
			public class EventGoToStateEditorGUI : EventEditorGUI
			{
				private string kLabelText = "Go To";
				private Vector2 kButtonPadding = new Vector2(10, 0);
				private Vector2 kButtonBorder = new Vector2(2, 2);
				private bool _goToState;

				#region EventEditorGUI
				public override void DrawLabel(GUIStyle style)
				{
					EventGoToState evnt = (EventGoToState)GetEditableObject();

					GUIContent labelContent = new GUIContent(kLabelText);
					Vector2 labelSize = style.CalcSize(labelContent);
					Rect labelRect = new Rect(0, 1, labelSize.x, labelSize.y);
					GUI.Label(labelRect, labelContent, style);

					GUIContent buttonContent = new GUIContent(evnt._state.GetStateName());
					Vector2 buttonSize = style.CalcSize(buttonContent) + kButtonPadding;

					Rect buttonRect = new Rect(labelSize.x, 1, buttonSize.x, buttonSize.y);

					if (GUI.Button(buttonRect, evnt._state.GetStateName()))
					{
						StateMachineEditor stateMachineEditor = GetTimelineEditor().GetParent() as StateMachineEditor;

						if (stateMachineEditor != null)
						{
							stateMachineEditor.ShowStateDetails(evnt._state.GetStateID());
						}
					}
				}

				public override Vector2 GetLabelSize(GUIStyle style)
				{
					EventGoToState evnt = (EventGoToState)GetEditableObject();
					
					GUIContent labelContent = new GUIContent(kLabelText);
					Vector2 labelSize = style.CalcSize(labelContent);

					GUIContent buttonContent = new GUIContent(evnt._state.GetStateName());
					Vector2 buttonSize = style.CalcSize(buttonContent) + kButtonPadding + kButtonBorder;

					return new Vector2(labelSize.x + buttonSize.x, buttonSize.y);
				}
				#endregion
			}
		}
	}
}