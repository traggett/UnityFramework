using System;

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
						
						evnt._preStateType = SerializationEditorGUILayout.ObjectField(evnt._preStateType, "State Type", out dataChanged);

						switch (evnt._preStateType)
						{
							case EventDoThenGoToState.eStateType.Timeline:
								dataChanged |= evnt._preState.RenderObjectProperties(new GUIContent("State"));
								break;
							case EventDoThenGoToState.eStateType.Coroutine:
								dataChanged |= evnt._preCoroutine.RenderObjectProperties(new GUIContent("State"));
								break;
						}

						EditorGUI.indentLevel = origIndent;
					}

					_secondStateEditorFoldout = EditorGUILayout.Foldout(_secondStateEditorFoldout, "Then");
					if (_secondStateEditorFoldout)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;
						
						evnt._stateType = SerializationEditorGUILayout.ObjectField(evnt._stateType, "State Type", out dataChanged);

						switch (evnt._stateType)
						{
							case EventDoThenGoToState.eStateType.Timeline:
								dataChanged |= evnt._state.RenderObjectProperties(new GUIContent("State"));
								break;
							case EventDoThenGoToState.eStateType.Coroutine:
								dataChanged |= evnt._coroutine.RenderObjectProperties(new GUIContent("State"));
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