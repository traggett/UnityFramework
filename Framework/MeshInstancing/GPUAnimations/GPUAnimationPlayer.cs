using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public struct GPUAnimationPlayer
			{
				public GPUAnimations.Animation _animation;
				public float _frame;
				public float _speed;
				private GameObject _gameObject;

				public void Play(GameObject gameObject, GPUAnimations.Animation animation, float speed = 1.0f)
				{
					_gameObject = gameObject;
					_animation = animation;
					_frame = 0;
					_speed = 1.0f;
				}

				public void Stop()
				{
					_gameObject = null;
				}

				public float GetCurrentTexureFrame()
				{
					return _animation._startFrameOffset + _frame;
				}

				public void Update(float deltaTime)
				{
					if (_gameObject != null && deltaTime > 0.0f)
					{
						float preFrame = _frame;

						_frame += deltaTime * _animation._fps * _speed;

						GPUAnimations.CheckForEvents(_gameObject, _animation, preFrame, _frame);

						if (Mathf.FloorToInt(_frame) >= _animation._totalFrames - 1)
						{
							_frame = 0;
						}
					}
				}
			}
		}
    }
}