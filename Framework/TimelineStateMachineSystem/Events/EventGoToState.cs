using UnityEngine;
using System;

namespace Framework
{
	using StateMachineSystem;
	using TimelineSystem;
	using Utils;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[EventCategory(EventCategoryAttribute.kCoreEvent)]
		public class EventGoToState : Event, IStateMachineEvent
		{
			public enum eStateType
			{
				Timeline,
				Coroutine
			}

			#region Public Data
			public eStateType _stateType = eStateType.Timeline;
			public TimelineStateRef _state = new TimelineStateRef();
			public CoroutineRef _coroutine = new CoroutineRef();
			#endregion

			#region Event
#if UNITY_EDITOR
			public override bool EndsTimeline()
			{
				return true;
			}

			public override Color GetColor()
			{
				return new Color(217.0f / 255.0f, 80.0f / 255.0f, 58.0f / 255.0f);
			}

			public override string GetEditorDescription()
			{
				string text = string.Empty;

				switch (_stateType)
				{
					case eStateType.Timeline:
						{
							text = "Go To: <b>" + _state.GetStateName() + "</b>";
						}
						break;
					case eStateType.Coroutine:
						{
							text = "Go To: <b>" + _coroutine + "</b>";
						}
						break;
				}

				return text;
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachine stateMachine)
			{
				switch (_stateType)
				{
					case eStateType.Coroutine:
						{
							stateMachine.GoToState(_coroutine.RunCoroutine());
						}
						break;
					case eStateType.Timeline:
						{
							stateMachine.GoToState(TimelineStateMachine.Run(stateMachine, _state));
						}
						break;
				}
				
				return eEventTriggerReturn.EventFinishedExitState;
			}

			public eEventTriggerReturn Update(StateMachine stateMachine, float eventTime)
			{
				return eEventTriggerReturn.EventOngoing;
			}

			public void End(StateMachine stateMachine) { }

#if UNITY_EDITOR
			public EditorStateLink[] GetEditorLinks()
			{
				EditorStateLink[] links = null;

				switch (_stateType)
				{
					case eStateType.Coroutine:
						break;
					case eStateType.Timeline:
						links = new EditorStateLink[1];
						links[0] = new EditorStateLink();
						links[0]._timeline = _state;
						links[0]._description = "Go To";
						break;
				}

				return links;
			}
#endif
			#endregion
		}
	}
}