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

					bool dataChanged = false;
					evnt._stateType = SerializationEditorGUILayout.ObjectField(evnt._stateType, "State Type", ref dataChanged);

					switch (evnt._stateType)
					{
						case EventGoToState.eStateType.Timeline:
							evnt._state = SerializationEditorGUILayout.ObjectField(evnt._state, "State", ref dataChanged);
							break;
						case EventGoToState.eStateType.Coroutine:
							evnt._coroutine = SerializationEditorGUILayout.ObjectField(evnt._coroutine, "State", ref dataChanged);
							break;
					}

					return dataChanged;
				}
				#endregion
			}
		}
	}
}