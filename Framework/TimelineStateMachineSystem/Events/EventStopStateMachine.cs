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
		public class EventStopStateMachine : Event, ITimelineStateEvent
		{
			#region Event
#if UNITY_EDITOR
			public override bool EndsTimeline()
			{
				return true;
			}

			public override Color GetColor()
			{
				return new Color(0.9f, 0.53f, 0.45f);
			}

			public override string GetEditorDescription()
			{
				return "Stop StateMachine";
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachineComponent stateMachine)
			{
				stateMachine.Stop();
				return eEventTriggerReturn.EventFinishedExitState;
			}

			public eEventTriggerReturn Update(StateMachineComponent stateMachine, float eventTime)
			{
				return eEventTriggerReturn.EventOngoing;
			}

			public void End(StateMachineComponent stateMachine) { }
#if UNITY_EDITOR
			public StateMachineEditorLink[] GetEditorLinks() { return null; }
#endif
			#endregion
		}
	}
}
