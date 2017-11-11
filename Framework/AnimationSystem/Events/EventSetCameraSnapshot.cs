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
		public class EventSetCameraSnapshot : Event, ITimelineStateEvent
		{
			#region Public Data
			public ComponentRef<AnimatedCamera> _camera;
			public ComponentRef<AnimatedCameraSnapshot> _snapshot;
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
				return new Color(108.0f / 255.0f, 198.0f / 255.0f, 197.0f / 255.0f);
			}

			public override string GetEditorDescription()
			{
				return (_blendTime > 0.0f ? "Blend " : "Set ") + _camera + " to " + _snapshot;
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachineComponent stateMachine)
			{
				AnimatedCamera camera = _camera;

				if (camera != null)
				{
					AnimatedCamera.Animation animation = new AnimatedCamera.Animation(_snapshot.GetComponent());
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
