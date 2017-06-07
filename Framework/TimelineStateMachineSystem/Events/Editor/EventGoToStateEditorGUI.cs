using System;
using UnityEngine;

namespace Framework
{
	using Serialization;
	using TimelineSystem.Editor;
	
	namespace TimelineStateMachineSystem
	{
		namespace Editor
		{
			[EventCustomEditorGUI(typeof(EventGoToState))]
			public class EventGoToStateEditorGUI : EventEditorGUI
			{
				#region EventEditorGUI
				public override bool RenderObjectProperties(GUIContent label)
				{
					EventGoToState evnt = GetEditableObject() as EventGoToState;

					bool dataChanged;
					evnt._stateType = SerializedObjectEditorGUILayout.ObjectField(evnt._stateType, "State Type", out dataChanged);

					switch (evnt._stateType)
					{
						case EventGoToState.eStateType.Timeline:
							dataChanged |= evnt._state.RenderObjectProperties(new GUIContent("State"));
							break;
						case EventGoToState.eStateType.Coroutine:
							dataChanged |= evnt._coroutine.RenderObjectProperties(new GUIContent("State"));
							break;
					}

					return dataChanged;
				}
				#endregion
			}
		}
	}
}