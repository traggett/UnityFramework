using System;
using UnityEngine;

namespace Framework
{
	using StateMachineSystem;
	using TimelineSystem;
	using TimelineStateMachineSystem;
	
	namespace AnimationSystem
	{
		[Serializable]
		[EventCategory("Animation")]
		public class EventPlayTimedAnimation : EventPlayAnimation
		{
			#region Public Data
			public float _duration = 0.0f;
			#endregion

			private IAnimator _animator;

			#region Event
			public override float GetDuration()
			{
				return _duration;
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(88.0f / 255.0f, 194.0f / 255.0f, 82.0f / 255.0f);
			}

			public override string GetEditorDescription()
			{
				return _animation._animator + ": " + (_queued ? "Queue Timed" : "Play Timed") + "(" + _animation._animationId + ")";
			}			
#endif
			#endregion

			#region IStateMachineSystemEvent
			public new eEventTriggerReturn Trigger(StateMachineComponent stateMachine)
			{
				_animator = _animation.GetAnimator();

				if (_animator != null)
				{
					_animator.Play(_channel, _animation._animationId, _wrapMode,  _blendTime, _easeType, _weight, _queued);
					return eEventTriggerReturn.EventOngoing;
				}

				return eEventTriggerReturn.EventFinished;
			}

			public new eEventTriggerReturn Update(StateMachineComponent stateMachine, float eventTime)
			{
				return eEventTriggerReturn.EventOngoing;
			}

			public new void End(StateMachineComponent stateMachine)
			{
				_animator.Stop(_channel, _blendTime, _easeType);
			}
			#endregion
		}
	}
}
