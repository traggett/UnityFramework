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
				#endregion

				#region Private Data
				private GPUAnimations.Animation _animation;
				private WrapMode _wrapMode;
				private float _frame;			
				private GameObject _gameObject;
				#endregion

				#region Public Interface
				public void Play(GameObject gameObject, GPUAnimations.Animation animation, WrapMode wrapMode = WrapMode.Default, float speed = 1.0f)
				{
					_gameObject = gameObject;
					_animation = animation;
					_frame = 0f;
					_speed = speed;
					_wrapMode = wrapMode;
				}

				public void Stop()
				{
					_gameObject = null;
				}

				public float GetCurrentTexureFrame()
				{
					return _animation._startFrameOffset + _frame;
				}

				public void SetNormalizedTime(float normalizedTime)
				{
					if (_gameObject != null)
					{
						//TO DO! use wrap mode to clamp, loop or ping-pong


						float prevFrame = _frame;
						_frame = (_animation._totalFrames - 1) * (normalizedTime - Mathf.Floor(normalizedTime));

						GPUAnimations.CheckForEvents(_gameObject, _animation, prevFrame, _frame);
					}
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
										//TO DO! speed should reverese
									}
									break;

								case WrapMode.Loop:
								case WrapMode.Default:
								default:
									{
										_frame -= (_animation._totalFrames - 1);
									}
									break;
							}
						}
					}
				}
				#endregion
			}
		}
    }
}