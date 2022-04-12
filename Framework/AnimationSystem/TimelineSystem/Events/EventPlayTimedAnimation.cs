using System;
using UnityEngine;

namespace Framework
{
	using TimelineSystem;

	namespace AnimationSystem
	{
		namespace TimelineSystem
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
				public override void Trigger()
				{
					_animator = _animation.GetAnimator();

					if (_animator != null)
					{
						_animator.Play(_channel, _animation._animationId, _wrapMode, _blendTime, _easeType, _weight, _queued);
					}
				}

				public override void End()
				{
					if (_animator != null)
					{
						_animator.Stop(_channel, _blendTime, _easeType);
					}
				}

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
			}
		}
	}
}