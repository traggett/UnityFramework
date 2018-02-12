using UnityEngine;
using UnityEngine.Playables;
using System;

namespace Framework
{
	using StateMachineSystem;
	using TimelineSystem;
	
	using Utils;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[EventCategory("Playables")]
		public class EventStopPlayableDirector : Event, ITimelineStateEvent
		{
			#region Public Data
			public ComponentRef<PlayableDirector> _director;
			#endregion

			#region Event
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(0.859f, 0.439f, 0.576f);
			}

			public override string GetEditorDescription()
			{
				return "Stop (<b>"+ _director + "</b>)";
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachineComponent stateMachine)
			{
				PlayableDirector director = _director.GetComponent();

				if (director != null)
				{
					director.Stop();
				}

				return eEventTriggerReturn.EventFinished;
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
