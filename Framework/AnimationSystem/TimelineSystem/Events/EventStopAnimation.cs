using UnityEngine;
using System;

namespace Framework
{
	using Maths;
	using Utils;
	using TimelineSystem;

	namespace AnimationSystem
	{
		namespace TimelineSystem
		{
			[Serializable]
			[EventCategory("Animation")]
			public class EventStopAnimation : Event
			{
				#region Public Data
				public ComponentRef<IAnimator> _animator;
				public int _channel = 0;
				public float _blendTime = 0.0f;
				public InterpolationType _easeType = InterpolationType.InOutSine;
				#endregion

				#region Event
				public override void Trigger()
				{
					IAnimator target = _animator.GetComponent();

					if (target != null)
					{
						target.Stop(_channel, _blendTime, _easeType);
					}
				}

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
			}
		}
	}
}