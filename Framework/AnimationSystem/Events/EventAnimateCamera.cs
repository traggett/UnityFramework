using UnityEngine;
using System;

namespace Framework
{
	using Maths;
	using TimelineStateMachineSystem;
	using StateMachineSystem;
	using TimelineSystem;
	using Utils;

	namespace AnimationSystem
	{
		[Serializable]
		[EventCategory("Animation")]
		public class EventAnimateCamera : Event, ITimelineStateEvent
		{
			#region Public Data
			public ComponentRef<AnimatedCamera> _camera;
			public ComponentRef<AnimatedCameraSnapshot>[] _animationSnapshots;
			public float _animationDuration = 1.0f;
			public eInterpolation _animationEaseType = eInterpolation.InOutSine;
			public WrapMode _animationWrapMode = WrapMode.PingPong;
			public float _blendTime = 0.0f;
			public eInterpolation _blendEaseType = eInterpolation.InOutSine;
			
			#endregion

			#region Event
			public override float GetDuration()
			{
				return _blendTime;
			}

#if UNITY_EDITOR
			public override Color GetColor()
			{
				return new Color(133.0f / 255.0f, 202.0f / 255.0f, 111.0f / 255.0f);
			}

			public override string GetEditorDescription()
			{
				return "Animate Camera between Snapshots";
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachineComponent stateMachine)
			{
				AnimatedCamera camera = _camera;

				if (camera != null)
				{
					AnimatedCamera.Animation animation = new AnimatedCamera.Animation();
					animation._snapshots = new AnimatedCameraSnapshot[_animationSnapshots.Length];
					for (int i=0; i< _animationSnapshots.Length; i++)
					{
						animation._snapshots[i] = _animationSnapshots[i].GetComponent();
					}
					animation._duration = _animationDuration;
					animation._easeType = _animationEaseType;
					animation._wrapMode = _animationWrapMode;
					
					camera.SetAnimation(animation, _blendEaseType, _blendTime);
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
