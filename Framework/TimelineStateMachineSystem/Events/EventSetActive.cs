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
		public class EventSetActive : Event, IStateMachineEvent
		{
			#region Public Data
			public GameObjectRef _target;
			public bool _active = false;
			#endregion

			#region Event
#if UNITY_EDITOR
			public override Color GetColor()
			{
				return new Color(0.3f, 0.6f, 0.8f);
			}

			public override string GetEditorDescription()
			{
				return (_active ? "Activate" : "Deactivate") + " GameObject (<b>"+_target+"</b>)";
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachine stateMachine)
			{
				GameObject target = _target.GetGameObject();

				if (target != null)
				{
					target.SetActive(_active);
				}

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
