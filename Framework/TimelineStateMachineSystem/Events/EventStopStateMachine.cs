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
		public class EventStopStateMachine : Event, IStateMachineEvent
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
			public eEventTriggerReturn Trigger(StateMachine stateMachine)
			{
				stateMachine.Stop();
				return eEventTriggerReturn.EventFinishedExitState;
			}

			public eEventTriggerReturn Update(StateMachine stateMachine, float eventTime)
			{
				return eEventTriggerReturn.EventOngoing;
			}

			public void End(StateMachine stateMachine) { }
#if UNITY_EDITOR
			public EditorStateLink[] GetEditorLinks() { return null; }
#endif
			#endregion
		}
	}
}
