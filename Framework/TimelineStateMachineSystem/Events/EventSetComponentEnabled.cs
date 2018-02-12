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
		public class EventSetComponentEnabled : Event, ITimelineStateEvent
		{
			#region Public Data
			public ComponentRef<MonoBehaviour> _target;
			public bool _enabled = false;
			#endregion

			#region Event
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(0.3f, 0.6f, 0.8f);
			}

			public override string GetEditorDescription()
			{
				return (_enabled ? "Enable" : "Disable") + " Component (<b>"+_target+"</b>)";
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachineComponent stateMachine)
			{
				MonoBehaviour target = _target.GetComponent();

				if (target != null)
				{
					target.enabled = _enabled;
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
