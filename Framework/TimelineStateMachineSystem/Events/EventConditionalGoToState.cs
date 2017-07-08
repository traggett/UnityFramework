using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace Framework
{
	using StateMachineSystem;
	using TimelineSystem;
	using Serialization;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[EventCategory("Flow")]
		public class EventConditionalGoToState : Event, IStateMachineEvent, ICustomEditorInspector
		{
			#region Public Data
			public TimelineStateRef _state = new TimelineStateRef();
			public IConditional _condition;
			public float _duration = 0.0f;
			#endregion

#if UNITY_EDITOR
			private bool _editorFoldout;
#endif

			#region ICustomEditable
#if UNITY_EDITOR
			public bool RenderObjectProperties(GUIContent label)
			{
				bool dataChanged = false;

				_editorFoldout = EditorGUILayout.Foldout(_editorFoldout, "Condition (" + _condition + ")");
				if (_editorFoldout)
				{
					int origIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel++;
					
					_condition = SerializationEditorGUILayout.ObjectField(_condition, "Properties", ref dataChanged);
					
					EditorGUI.indentLevel = origIndent;
				}

				_state = SerializationEditorGUILayout.ObjectField(_state, new GUIContent("Go to"), ref dataChanged);

				EditorGUI.BeginChangeCheck();
				_duration = EditorGUILayout.FloatField("Time Window", _duration);
				dataChanged |= EditorGUI.EndChangeCheck();

				return dataChanged;
			}
#endif
			#endregion

			#region Event
			public override float GetDuration()
			{
				return _duration;
			}

#if UNITY_EDITOR
			public override bool EndsTimeline()
			{
				return false;
			}

			public override Color GetColor()
			{
				return new Color(217.0f / 255.0f, 80.0f / 255.0f, 50.0f / 255.0f);
			}

			public override string GetEditorDescription()
			{
				return (_condition != null ? _condition.GetEditorDescription() : "(condition)") + " Go to <b>" + _state.GetStateName() + "</b>";
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachine stateMachine)
			{
				eEventTriggerReturn ret = eEventTriggerReturn.EventFinished;

				if (_condition != null)
				{
					_condition.OnStartConditionChecking(stateMachine);

					if (_condition.IsConditionMet(stateMachine))
					{
						stateMachine.GoToState(TimelineStateMachine.Run(stateMachine, _state));
						ret = eEventTriggerReturn.EventFinishedExitState;
					}
					if (_duration > 0.0f)
					{
						ret = eEventTriggerReturn.EventOngoing;
					}
				}

				return ret;
			}

			public eEventTriggerReturn Update(StateMachine stateMachine, float eventTime)
			{
				if (_condition.IsConditionMet(stateMachine))
				{
					stateMachine.GoToState(TimelineStateMachine.Run(stateMachine, _state));
					return eEventTriggerReturn.EventFinishedExitState;
				}

				return eEventTriggerReturn.EventOngoing;
			}

			public void End(StateMachine stateMachine)
			{
				_condition.OnEndConditionChecking(stateMachine);
			}

#if UNITY_EDITOR
			public EditorStateLink[] GetEditorLinks()
			{
				EditorStateLink[] links = new EditorStateLink[1];

				links[0] = new EditorStateLink();
				links[0]._timeline = _state;
				links[0]._description = (_condition != null ? _condition.GetEditorDescription() : "...");

				return links;
			}
#endif
			#endregion
		}
	}
}