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
		public class EventRunMethod : Event, IStateMachineEvent
		{
			#region Public Data
			public ComponentVoidMethodRef _method = new ComponentVoidMethodRef();
			#endregion

			#region Event
#if UNITY_EDITOR
			public override Color GetColor()
			{
				return new Color(219.0f / 255.0f, 64.0f / 255.0f, 11.0f / 255.0f);
			}

			public override string GetEditorDescription()
			{
				return "Run: <b>(" + _method + ")</b>";
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachine stateMachine)
			{
				_method.RunMethod();
				return eEventTriggerReturn.EventFinished;
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
