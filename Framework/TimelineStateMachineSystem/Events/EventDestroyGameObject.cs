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
		public class EventDestroyGameObject : Event, ITimelineStateEvent
		{
			#region Public Data
			public GameObjectRef _gameObject;
			#endregion

			#region Event
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(0.3f, 0.6f, 0.8f);
			}

			public override string GetEditorDescription()
			{
				return "Destroy GameObject (<b>" + _gameObject + "</b>)";
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachineComponent stateMachine)
			{
				GameObject gameObject = _gameObject.GetGameObject();

				if (gameObject != null)
				{
					GameObject.Destroy(gameObject);
				}

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
