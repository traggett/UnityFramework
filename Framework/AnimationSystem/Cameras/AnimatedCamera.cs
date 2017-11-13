using UnityEngine;

using System;
using System.Collections.Generic;

namespace Framework
{
	using Maths;

	namespace AnimationSystem
	{
		public class AnimatedCamera : MonoBehaviour
		{
			public class Animation
			{
				public AnimatedCameraState[] _states;
				public float _duration;
				public eInterpolation _easeType;
				public WrapMode _wrapMode;
				public float _animationT;
				public float _weight;

				public Animation(params AnimatedCameraState[] states)
				{
					_states = states;
				}
			}

			#region Private Data
			private Camera _camera;
			private List<Animation> _animations = new List<Animation>();
			private float _currentAnimationBlendTime;
			protected eInterpolation _currentEaseType;
			#endregion

			#region MonoBehaviour Calls
			protected virtual void Update()
			{
				if (_animations.Count > 0)
				{
					float deltaTime = Time.deltaTime;

					_animations[_animations.Count - 1]._weight += deltaTime / _currentAnimationBlendTime;

					//If current animation weight is more than 1, Stop all other animations
					if (_animations.Count > 1 && _animations[_animations.Count - 1]._weight >= 1.0f)
					{
						_animations[_animations.Count - 1]._weight = 1.0f;
						_animations = new List<Animation>() { _animations[_animations.Count - 1] };
					}

					foreach (Animation animation in _animations)
					{
						UpdateAnimation(animation, deltaTime);
					}
				}
			}
			#endregion

			#region Public Functions
			public Camera GetCamera()
			{
				if (_camera == null)
					_camera = GetComponentInChildren<Camera>();

				return _camera;
			}
			
			public void SetAnimation(Animation animation, eInterpolation easeType = eInterpolation.InOutSine, float blendTime = 0.0f)
			{
				_currentAnimationBlendTime = blendTime;
				_currentEaseType = easeType;
				animation._animationT = 0.0f;
				if (blendTime > 0.0f)
				{
					animation._weight = 0.0f;
					_animations.Add(animation);
				}			
				else
				{
					animation._weight = 1.0f;
					_animations = new List<Animation>() { animation };
				}	
			}

			public virtual AnimatedCameraState GetState()
			{
				AnimatedCameraState state = new AnimatedCameraState();
				state._position = this.transform.position;
				state._rotation = this.transform.rotation;
				state._cameraRect = GetCamera().rect;
				state._fieldOfView = GetCamera().fieldOfView;
				return state;
			}

			public virtual void SetState(AnimatedCameraState state)
			{
				this.transform.position = state._position;
				this.transform.rotation = state._rotation;
				GetCamera().fieldOfView = state._fieldOfView;
				GetCamera().rect = state._cameraRect;
			}

			public void SetState(AnimatedCameraState snapshotState, float weight)
			{
				if (weight > 0.0f)
				{
					if (weight < 1.0f)
					{
						snapshotState = GetState().InterpolateTo(this, snapshotState, _currentEaseType, weight);
					}
					
					SetState(snapshotState);
				}
			}

			public virtual Type GetAnimatedCameraStateSourceType()
			{
				return typeof(AnimatedCameraSnapshot);
			}
			#endregion

			private void UpdateAnimation(Animation animation, float deltaTime)
			{
				if (animation._states.Length == 1)
				{
					SetState(animation._states[0], animation._weight);
				}
				else if (animation._states.Length > 1)
				{
					animation._animationT += deltaTime / animation._duration;

					if (animation._wrapMode == WrapMode.ClampForever)
					{
						animation._animationT = Mathf.Clamp01(animation._animationT);
					}
					else if (animation._wrapMode == WrapMode.Once)
					{
						if (animation._animationT > 1.0f)
						{
							animation._states = null;
						}
					}

					float baseValue = Mathf.Floor(animation._animationT);
					float fraction = animation._animationT - baseValue;

					if (animation._wrapMode == WrapMode.PingPong && ((int)baseValue % 2) == 1)
					{
						fraction = 1.0f - fraction;
					}

					float sectonTDist = 1.0f / (animation._states.Length - 1);
					float sectionT = fraction / sectonTDist;

					int sectionIndex = Mathf.FloorToInt(sectionT);
					sectionT = sectionT - (float)sectionIndex;

					AnimatedCameraState state = animation._states[sectionIndex].InterpolateTo(this, animation._states[sectionIndex + 1], animation._easeType, sectionT);
					SetState(state, animation._weight);
				}
			}
		}
	}
}