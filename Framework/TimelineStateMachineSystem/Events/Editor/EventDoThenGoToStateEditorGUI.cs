using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using TimelineSystem.Editor;
	using Serialization;

	namespace TimelineStateMachineSystem
	{
		namespace Editor
		{
			[EventCustomEditorGUI(typeof(EventDoThenGoToState))]
			public class EventDoThenGoToStateEditorGUI : EventEditorGUI
			{
#if UNITY_EDITOR
				private bool _firstStateEditorFoldout = true;
				private bool _secondStateEditorFoldout = true;
#endif
				public override bool RenderObjectProperties(GUIContent label)
				{
					EventDoThenGoToState evnt = GetEditableObject() as EventDoThenGoToState;
					bool dataChanged = false;

					_firstStateEditorFoldout = EditorGUILayout.Foldout(_firstStateEditorFoldout, "First");
					if (_firstStateEditorFoldout)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;
						
						evnt._preStateType = SerializationEditorGUILayout.ObjectField(evnt._preStateType, "State Type", ref dataChanged);

						switch (evnt._preStateType)
						{
							case EventDoThenGoToState.eStateType.Timeline:
								evnt._preState = SerializationEditorGUILayout.ObjectField(evnt._preState, new GUIContent("State"), ref dataChanged);
								break;
							case EventDoThenGoToState.eStateType.Coroutine:
								evnt._preCoroutine = SerializationEditorGUILayout.ObjectField(evnt._preCoroutine, new GUIContent("State"), ref dataChanged);
								break;
						}

						EditorGUI.indentLevel = origIndent;
					}

					_secondStateEditorFoldout = EditorGUILayout.Foldout(_secondStateEditorFoldout, "Then");
					if (_secondStateEditorFoldout)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;
						
						evnt._stateType = SerializationEditorGUILayout.ObjectField(evnt._stateType, "State Type", ref dataChanged);

						switch (evnt._stateType)
						{
							case EventDoThenGoToState.eStateType.Timeline:
								evnt._state = SerializationEditorGUILayout.ObjectField(evnt._state, new GUIContent("State"), ref dataChanged);
								break;
							case EventDoThenGoToState.eStateType.Coroutine:
								evnt._coroutine = SerializationEditorGUILayout.ObjectField(evnt._coroutine, new GUIContent("State"), ref dataChanged);
								break;
						}

						EditorGUI.indentLevel = origIndent;
					}

					return dataChanged;
				}
			}
		}
	}
}