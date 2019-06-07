using UnityEngine;

namespace Framework
{
	using Utils;

	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public sealed class GPUAnimationState : TrackedReference
			{
				public readonly GPUAnimations.Animation _animation;
				
				private float _time;
				private float _normalizedTime;

				public bool Enabled { get; set; }
				public float Weight { get; set; }
				public WrapMode WrapMode { get; set; }
				public float Length
				{
					get
					{
						return _animation._totalFrames * _animation._fps;
					}
				}
				public float Time
				{
					get
					{
						return _time;
					}
					set
					{
						_time = value;
						_normalizedTime = _time / this.Length;
					}
				}
				public float NormalizedTime
				{
					get
					{
						return _normalizedTime;
					}
					set
					{
						_normalizedTime = value;
						_time = _normalizedTime * this.Length;
					}
				}
				public float Speed { get; set; }
				public float NormalizedSpeed
				{
					get
					{
						return 1.0f / (this.Speed * this.Length);
					}
					set
					{
						Speed = value / this.Length;
					}
				}
				
				public GPUAnimationState(GPUAnimations.Animation animation)
				{
					_animation = animation;
					Time = 0.0f;
					Speed = 1.0f;
					WrapMode = _animation._wrapMode;
				}
			}
		}
    }
}