using UnityEngine;
using System;

namespace Framework
{
	using StateMachineSystem;
	using TimelineSystem;
	
	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[EventCategory(EventCategoryAttribute.kCoreEvent)]
		public class EventGoToState : Event, ITimelineStateEvent
		{
			#region Public Data
			public StateRef _state;
			#endregion

			#region Event
#if UNITY_EDITOR
			public override bool GetEditorShouldBeLastEventInTimeline()
			{
				return true;
			}

			public override Color GetEditorColor()
			{
				return new Color(217.0f / 255.0f, 80.0f / 255.0f, 58.0f / 255.0f);
			}

			public override string GetEditorDescription()
			{
				return "Go To: <b>" + _state.GetStateName() + "</b>";
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachineComponent stateMachine)
			{
				stateMachine.GoToState(StateMachine.Run(stateMachine, _state));
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
				StateMachineEditorLink[] links = new StateMachineEditorLink[1];
				links[0] = new StateMachineEditorLink(this, "state", "Go To");
				return links;
			}
#endif
			#endregion
		}
	}
}