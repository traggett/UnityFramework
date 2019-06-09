using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public struct GPUAnimationPlayer
			{
				#region Public Data
				public float _speed;
				public WrapMode _wrapMode;
				#endregion

				#region Private Data
				private GPUAnimations.Animation _animation;
				private float _frame;
				private float _loops;
				private GameObject _gameObject;
				#endregion

				#region Public Interface
				public void Play(GameObject gameObject, GPUAnimations.Animation animation, WrapMode wrapMode = WrapMode.Default, float speed = 1.0f)
				{
					_gameObject = gameObject;
					_animation = animation;
					_wrapMode = wrapMode;
					_speed = speed;
					_frame = 0f;
					_loops = 0f;
				}

				public void Stop()
				{
					_gameObject = null;
				}

				public void Update(float deltaTime)
				{
					if (_gameObject != null && deltaTime > 0f && _speed > 0f)
					{
						float prevFrame = _frame;

						_frame += deltaTime * _animation._fps * _speed;

						GPUAnimations.CheckForEvents(_gameObject, _animation, prevFrame, _frame);

						if (_frame > _animation._totalFrames - 1)
						{
							switch (_wrapMode)
							{
								case WrapMode.Clamp:
								case WrapMode.ClampForever:
									{
										_frame = _animation._totalFrames - 1;
									}
									break;

								case WrapMode.PingPong:
									{
										//TO DO! speed should reverse
										_loops += 1.0f;
									}
									break;

								case WrapMode.Loop:
								case WrapMode.Default:
								default:
									{
										_frame -= (_animation._totalFrames - 1);
										_loops += 1.0f;
									}
									break;
							}
						}
					}
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

						if (nextSampleFrame > _animation._totalFrames - 1)
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

				public float GetCurrentTime()
				{
					return _loops + _frame * _animation._fps;
				}

				public void SetCurrentTime(float time)
				{
					float normalised = time / _animation.GetLength();
					_loops = Mathf.Floor(normalised);
					_frame = (normalised - _loops) * _animation._totalFrames;
				}

				public float GetNormalizedTime()
				{
					return GetCurrentTime() / _animation.GetLength();
				}

				public void SetNormalizedTime(float normalizedTime, bool checkForEvents = false)
				{
					_loops = Mathf.Floor(normalizedTime);

					float prevFrame = _frame;
					_frame = (normalizedTime - _loops) * _animation._totalFrames;
					
					if (checkForEvents && _gameObject != null)
						GPUAnimations.CheckForEvents(_gameObject, _animation, prevFrame, _frame);
				}

				public float GetNormalizedSpeed()
				{
					return 1.0f / (_speed * _animation.GetLength());
				}

				public void SetNormalizedSpeed(float normalizedSpeed)
				{
					_speed = normalizedSpeed / _animation.GetLength();
				}
				#endregion
			}
		}
    }
}