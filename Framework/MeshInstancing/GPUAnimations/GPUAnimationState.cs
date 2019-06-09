using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public sealed class GPUAnimationState : TrackedReference
			{
				#region Public Data
				public bool Enabled { get; set; }
				public float Weight { get; set; }
				public WrapMode WrapMode
				{
					get
					{
						return _player._wrapMode;
					}
					set
					{
						_player._wrapMode = value;
					}
				}
				public string Name
				{
					get
					{
						return _player.GetAnimation()._name;
					}
				}
				public float Length
				{
					get
					{
						return _player.GetAnimation().GetLength();
					}
				}
				public float Time
				{
					get
					{
						return _player.GetCurrentTime();
					}
					set
					{
						_player.SetCurrentTime(value);
					}
				}
				public float NormalizedTime
				{
					get
					{
						return _player.GetNormalizedTime();
					}
					set
					{
						_player.SetNormalizedTime(value);
					}
				}
				public float Speed
				{
					get
					{
						return _player._speed;
					}
					set
					{
						_player._speed = value;
					}
				}
				public float NormalizedSpeed
				{
					get
					{
						return _player.GetNormalizedSpeed();
					}
					set
					{
						_player.SetNormalizedSpeed(value);
					}
				}
				#endregion

				#region Private Data
				private GPUAnimationPlayer _player;
				private bool _fading;
				private float _targetWeight;
				private float _fromWeight;
				private float _fadeSpeed;
				private float _fadeLerp;
				#endregion

				#region Public Interface
				public GPUAnimationState(GPUAnimation animationComponent, GPUAnimations.Animation animation)
				{
					_player.Play(animationComponent.gameObject, animation, animation._wrapMode);
				}

				public void Update(float deltaTime)
				{
					if (Enabled)
					{
						_player.Update(deltaTime);
					}

					if (_fading)
					{
						_fadeLerp += _fadeSpeed * deltaTime;

						if (_fadeLerp >= 1.0f)
						{
							Weight = _targetWeight;
							_fading = false;
						}
						else
						{
							Weight = Mathf.Lerp(_fromWeight, _targetWeight, _fadeLerp);
						}
					}
				}

				public float GetCurrentTexureFrame()
				{
					return _player.GetCurrentTexureFrame();
				}

				public void BlendWeightTo(float targetWeight = 1.0f, float fadeLength = 0.3f)
				{
					_fading = fadeLength > 0.0f;

					if (_fading)
					{
						_targetWeight = targetWeight;
						_fromWeight = Weight;
						_fadeSpeed = 1.0f / fadeLength;
						_fadeLerp = 0.0f;
					}
					else
					{
						Weight = targetWeight;
					}
				}

				public void CancelBlend()
				{
					_fading = false;
				}
				#endregion
			}
		}
    }
}