using UnityEngine;
using System;

namespace Framework
{
	using StateMachineSystem;
	using TimelineSystem;
	using TimelineStateMachineSystem;
	using Event = TimelineSystem.Event;
	using Utils;

	namespace AnimationSystem
	{
		[Serializable]
		[EventCategory("Animation")]
		public class EventSetFadableActive : Event, ITimelineStateEvent
		{
			#region Public Data
			public ComponentRef<IFadable> _target;
			public bool _active = false;
			public float _fadeTime = 0.0f;
			#endregion

			#region Event
			public override float GetDuration()
			{
				return _fadeTime;
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(0.3f, 0.6f, 0.8f);
			}

			public override string GetEditorDescription()
			{
				if (_active)
					return "Enable " + _target + " over " + _fadeTime + " secs";
				else
					return "Disable " + _target + " over " + _fadeTime + " secs";
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachineComponent stateMachine)
			{
				IFadable target = _target.GetComponent();

				if (target != null)
				{
					if (_active)
						target.FadeIn(_fadeTime);
					else
						target.FadeOut(_fadeTime);
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
