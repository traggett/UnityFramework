using UnityEngine;

namespace Framework
{
	using Utils;

	namespace Graphics
	{
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
						bool animationFinished = false;

						if (_animation._totalFrames > 0 && deltaTime > 0f && _speed > 0f)
						{
							float prevFrame = _frame;
							int prevLoops = _loops;

							float speed = _speed;

							if (_wrapMode == WrapMode.PingPong)
							{
								speed *= _loops % 2 == (_loops > 0 ? 1 : 0) ? -1f : 1f;
							}

							_frame += deltaTime * _animation._fps * speed;

							if (_frame > _animation._totalFrames || _frame < 0)
							{
								switch (_wrapMode)
								{
									case WrapMode.PingPong:
									case WrapMode.Loop:

										{
											_frame %= _animation._totalFrames;
											_loops += _speed > 0 ? 1 : -1;
											break;
										}
									case WrapMode.ClampForever:
										{
											_frame = _frame > _animation._totalFrames ? _animation._totalFrames : 0;
											break;
										}
									case WrapMode.Once:
									case WrapMode.Default:
										{
											_frame = 0;
											animationFinished = true;
											break;
										}
								}
							}

							if (eventListener != null)
							{
								CheckForEvents(eventListener, prevFrame, _frame, prevLoops, _loops);
							}
						}

						return animationFinished;
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
						int prevLoops = _loops;

						_loops = Mathf.FloorToInt(normalizedTime);
						float fraction = normalizedTime - _loops;
						_frame = fraction * _animation._totalFrames;

						switch (_wrapMode)
						{
							case WrapMode.PingPong:
								{
									if (_loops % 2 == (_loops > 0 ? 1 : 0))
									{
										_frame = _animation._totalFrames - _frame;
									}
								}
								break;
							case WrapMode.Loop:
								{
									break;
								}
							case WrapMode.ClampForever:
								{
									if (_loops != 0)
									{
										_frame = _loops > 0 ? _animation._totalFrames : 0;
										prevLoops = _loops;
									}
									break;
								}
							case WrapMode.Once:
							case WrapMode.Default:
								{
									if (_loops != 0)
									{
										_frame = 0;
										prevLoops = _loops;
									}
									break;
								}
						}

						if (eventListener != null)
						{
							CheckForEvents(eventListener, prevFrame, _frame, prevLoops, _loops);
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
					private void CheckForEvents(GameObject gameObject, float prevFrame, float currFrame, int prevLoops, int currLoops)
					{
						if (_animation._events != null && _animation._events.Length > 0)
						{
							bool differentLoops = prevLoops != currLoops;
							float prevLoopframes = 0f;
							float currLoopframes = 0f;

							if (differentLoops)
							{
								prevLoopframes = prevLoops * _animation._totalFrames;
								currLoopframes = currLoops * _animation._totalFrames;

								prevFrame += prevLoopframes;
								currFrame += currLoopframes;
							}

							for (int i = 0; i < _animation._events.Length; i++)
							{
								float animationEventFrame = _animation._events[i].time * _animation._fps;
								bool triggerEvent = false;

								if (prevFrame < currFrame)
								{
									triggerEvent = ((prevFrame <= prevLoopframes + animationEventFrame && prevLoopframes + animationEventFrame < currFrame)
												|| (differentLoops && (prevFrame <= currLoopframes + animationEventFrame && currLoopframes + animationEventFrame < currFrame)));
								}
								else if (prevFrame > currFrame)
								{
									triggerEvent = ((currFrame < prevLoopframes + animationEventFrame && prevLoopframes + animationEventFrame <= prevFrame)
												|| (differentLoops && (currFrame < currLoopframes + animationEventFrame && currLoopframes + animationEventFrame <= prevFrame)));
								}

								if (triggerEvent)
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
}