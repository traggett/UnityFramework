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
		public class EventGoToState : Event, ITimelineStateEvent
		{
			public enum eStateType
			{
				Timeline,
				Coroutine
			}

			#region Public Data
			public eStateType _stateType = eStateType.Timeline;
			public StateRef _state;
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
			public eEventTriggerReturn Trigger(StateMachineComponent stateMachine)
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
							stateMachine.GoToState(StateMachine.Run(stateMachine, _state));
						}
						break;
				}
				
				return eEventTriggerReturn.EventFinishedExitState;
			}

			public eEventTriggerReturn Update(StateMachineComponent stateMachine, float eventTime)
			{
				return eEventTriggerReturn.EventOngoing;
			}

			public void End(StateMachineComponent stateMachine) { }

#if UNITY_EDITOR
			public StateMachineEditorLink[] GetEditorLinks()
			{
				StateMachineEditorLink[] links = null;

				switch (_stateType)
				{
					case eStateType.Coroutine:
						break;
					case eStateType.Timeline:
						links = new StateMachineEditorLink[1];
						links[0] = new StateMachineEditorLink(this, "state", "Go To");
						break;
				}

				return links;
			}
#endif
			#endregion
		}
	}
}