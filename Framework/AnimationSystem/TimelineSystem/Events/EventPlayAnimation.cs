using System;
using UnityEngine;

namespace Framework
{
	using Maths;
	using TimelineSystem;

	namespace AnimationSystem
	{
		namespace TimelineSystem
		{
			[Serializable]
			[EventCategory("Animation")]
			public class EventPlayAnimation : Event
			{
				#region Public Data
				public int _channel = 0;
				public AnimationRef _animation = new AnimationRef();
				public WrapMode _wrapMode = WrapMode.Default;
				public float _blendTime = 0.0f;
				public InterpolationType _easeType = InterpolationType.InOutSine;
				public float _weight = 1.0f;
				public bool _queued = false;
				#endregion

				#region Event
				public override void Trigger()
				{
					IAnimator target = _animation.GetAnimator();

					if (target != null)
					{
						target.Play(_channel, _animation._animationId, _wrapMode, _blendTime, _easeType, _weight, _queued);
					}
				}

				public override float GetDuration()
				{
					return _blendTime;
				}

#if UNITY_EDITOR
				public override Color GetEditorColor()
				{
					if (_queued)
						return new Color(155f / 255f, 219f / 255f, 145f / 255f);
					else
						return new Color(142f / 255f, 219f / 255f, 155f / 255f);
				}

				public override string GetEditorDescription()
				{
					return "(<b>" + _animation._animator + "</b>) " + (_queued ? "Queue" : "Play") + "Anim (<b>" + _animation._animationId + "</b>)";
				}
#endif
				#endregion
			}
		}
	}
}
