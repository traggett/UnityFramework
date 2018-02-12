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
		[EventCategory("Flow")]
		public class EventRunMethod : Event, ITimelineStateEvent
		{
			#region Public Data
			public ComponentVoidMethodRef _method = new ComponentVoidMethodRef();
			#endregion

			#region Event
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(219.0f / 255.0f, 64.0f / 255.0f, 11.0f / 255.0f);
			}

			public override string GetEditorDescription()
			{
				return _method;
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachineComponent stateMachine)
			{
				_method.RunMethod();
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
