using UnityEngine;
using System;

namespace Framework
{
	using Maths;
	using StateMachineSystem;
	using TimelineSystem;
	using TimelineStateMachineSystem;
	using Event = TimelineSystem.Event;
	using Framework.Utils;
	
	namespace AnimationSystem
	{
		[Serializable]
		[EventCategory("Animation")]
		public class EventStopAnimation : Event, ITimelineStateEvent
		{
			#region Public Data
			public ComponentRef<IAnimator> _animator;
			public int _channel = 0;
			public float _blendTime = 0.0f;
			public InterpolationType _easeType = InterpolationType.InOutSine;
			#endregion

			#region Event
			public override float GetDuration()
			{
				return _blendTime;
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(133.0f / 255.0f, 202.0f / 255.0f, 111.0f / 255.0f);
			}

			public override string GetEditorDescription()
			{
				return _animator + ": Stop Animations on Channel (<b>" + _channel + "</b>)";
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachineComponent stateMachine)
			{
				IAnimator target = _animator.GetComponent();

				if (target != null)
				{
					target.Stop(_channel, _blendTime, _easeType);
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
