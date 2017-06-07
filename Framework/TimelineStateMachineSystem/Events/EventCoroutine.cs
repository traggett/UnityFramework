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
		public class EventCoroutine : Event, IStateMachineEvent
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
			public eEventTriggerReturn Trigger(StateMachine stateMachine)
			{
				stateMachine.StartCoroutine(_coroutine.RunCoroutine());
				return eEventTriggerReturn.EventFinished;
			}

			public eEventTriggerReturn Update(StateMachine stateMachine, float eventTime) { return eEventTriggerReturn.EventFinished; }
			public void End(StateMachine stateMachine) { }
#if UNITY_EDITOR
			public EditorStateLink[] GetEditorLinks() { return null; }
#endif
			#endregion
		}
	}
}
