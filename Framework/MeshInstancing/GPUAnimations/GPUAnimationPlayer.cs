using Framework.Utils;
using UnityEngine;

namespace Framework
{
	using Utils;

	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public struct GPUAnimationPlayer
			{
				#region Private Data
				private readonly GPUAnimations.Animation _animation;
				private float _frame;
				private int _loops;
				private float _speed;
				private WrapMode _wrapMode;
				#endregion

				#region Public Interface
				public GPUAnimationPlayer(GPUAnimations.Animation animation, WrapMode wrapMode = WrapMode.Default, float speed = 1.0f)
				{
					_animation = animation;
					_wrapMode = wrapMode;
					_speed = speed;
					_frame = 0f;
					_loops = 0;
				}
				
				public bool Update(float deltaTime, GameObject eventListener = null)
				{
					if (_animation._totalFrames > 0 && deltaTime > 0f && _speed > 0f)
					{
						float prevFrame = _frame;
						
						_frame += deltaTime * _animation._fps * _speed * GetPlaybackDirection();

						if (eventListener != null)
						{
							CheckForEvents(eventListener, prevFrame, _frame);
						}
						
						if (_frame > _animation._totalFrames || _frame < 0)
						{
							switch (_wrapMode)
							{
								case WrapMode.Once:
									{
										_frame = _animation._totalFrames;
										return true;
									}
								case WrapMode.ClampForever:
									{
										_frame = _animation._totalFrames;
										break;
									}
								case WrapMode.PingPong:
								case WrapMode.Loop:
								case WrapMode.Default:
								default:
									{
										_frame = _frame < 0 ? _frame + _animation._totalFrames : _frame - _animation._totalFrames;
										_loops++;
										break;
									}
							}
						}
					}

					return false;
				}

				public GPUAnimations.Animation GetAnimation()
				{
					return _animation;
				}

				public float GetCurrentTexureFrame()
				{
					return _animation._startFrameOffset + _frame;
				}

				public void GetRootMotionVelocities(out Vector3 velocity, out Vector3 angularVelocity)
				{
					if (_animation._hasRootMotion)
					{
						int preSampleFrame = Mathf.FloorToInt(_frame);
						int nextSampleFrame = preSampleFrame + 1;

						if (nextSampleFrame > _animation._totalFrames)
						{
							velocity = _animation._rootMotionVelocities[preSampleFrame];
							angularVelocity = _animation._rootMotionAngularVelocities[preSampleFrame];
						}
						else
						{
							float frameLerp = _frame - preSampleFrame;
							velocity = Vector3.Lerp(_animation._rootMotionVelocities[preSampleFrame], _animation._rootMotionVelocities[nextSampleFrame], frameLerp);
							angularVelocity = Vector3.Lerp(_animation._rootMotionAngularVelocities[preSampleFrame], _animation._rootMotionAngularVelocities[nextSampleFrame], frameLerp);
						}
					}
					else
					{
						velocity = Vector3.zero;
						angularVelocity = Vector3.zero;
					}
				}

				public WrapMode GetWrapMode()
				{
					return _wrapMode;
				}

				public void SetWrapMode(WrapMode wrapMode)
				{
					_wrapMode = wrapMode;
				}

				public float GetCurrentTime()
				{
					return GetNormalizedTime() * _animation._length;
				}
				
				public void SetCurrentTime(float time, GameObject eventListener = null)
				{
					float normalizedTime = time / _animation._length;
					SetNormalizedTime(normalizedTime, eventListener);
				}

				public float GetNormalizedTime()
				{
					return _loops + (_frame / _animation._totalFrames);
				}
				
				public void SetNormalizedTime(float normalizedTime, GameObject eventListener = null)
				{
					float prevFrame = _frame;

					_loops = Mathf.FloorToInt(normalizedTime);
					float fraction = normalizedTime - _loops;

					_frame = fraction * _animation._totalFrames;
					
					if (eventListener != null && _frame > prevFrame)
					{
						CheckForEvents(eventListener, prevFrame, _frame);
					}	
				}

				public float GetSpeed()
				{
					return _speed;
				}

				public void SetSpeed(float speed)
				{
					_speed = speed;
				}

				public float GetNormalizedSpeed()
				{
					return 1.0f / (_speed * _animation._length);
				}

				public void SetNormalizedSpeed(float normalizedSpeed)
				{
					_speed = normalizedSpeed / _animation._length;
				}
				#endregion

				#region Private Functions
				private float GetPlaybackDirection()
				{
					if (_wrapMode == WrapMode.PingPong)
					{
						return _loops % 2 == (_loops > 0 ? 1 : 0) ? -1.0f : 1.0f;
					}

					return 1.0f;
				}

				private void CheckForEvents(GameObject gameObject, float prevFrame, float nextFrame)
				{
					if (_animation._events != null)
					{
						for (int i = 0; i < _animation._events.Length; i++)
						{
							float animationEventFrame = _animation._events[i].time * _animation._fps;

							if (prevFrame <= animationEventFrame && animationEventFrame < nextFrame)
							{
								AnimationUtils.TriggerAnimationEvent(_animation._events[i], gameObject);
							}
						}
					}
				}
				#endregion
			}
		}
    }
}