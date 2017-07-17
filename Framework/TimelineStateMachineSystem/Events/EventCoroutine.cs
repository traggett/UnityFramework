using System;
using UnityEngine;

namespace Framework
{
	using StateMachineSystem;
	using TimelineSystem;
	using Utils;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[EventCategory("Flow")]
		public class EventCoroutine : Event, ITimelineStateEvent
		{
			#region Public Data
			public CoroutineRef _coroutine = new CoroutineRef();
			#endregion

			#region Event
#if UNITY_EDITOR
			public override Color GetColor()
			{
				return new Color(219.0f / 255.0f, 64.0f / 255.0f, 11.0f / 255.0f);
			}

			public override string GetEditorDescription()
			{
				return "StartCoroutine: <b>'" + _coroutine + "'</b>";
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachineComponent stateMachine)
			{
				stateMachine.StartCoroutine(_coroutine.RunCoroutine());
				return eEventTriggerReturn.EventFinished;
			}

			public eEventTriggerReturn Update(StateMachineComponent stateMachine, float eventTime) { return eEventTriggerReturn.EventFinished; }
			public void End(StateMachineComponent stateMachine) { }
#if UNITY_EDITOR
			public StateMachineEditorLink[] GetEditorLinks() { return null; }
#endif
			#endregion
		}
	}
}
