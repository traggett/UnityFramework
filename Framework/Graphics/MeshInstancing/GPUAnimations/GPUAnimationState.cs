using UnityEngine;

namespace Framework
{
	namespace Graphics
	{
		namespace MeshInstancing
		{
			namespace GPUAnimations
			{
				public sealed class GPUAnimationState
				{
					#region Public Data
					public bool Enabled
					{
						get
						{
							return _enabled;
						}
						set
						{
							if (_enabled != value)
							{
								_enabled = value;
								_owner.OnStateEnabledChanged(this, value);

								if (!_enabled)
									CancelFading();
							}
						}
					}

					public float Weight { get; set; }

					public WrapMode WrapMode
					{
						get
						{
							return _player.GetWrapMode();
						}
						set
						{
							_player.SetWrapMode(value);
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
							return _player.GetAnimation()._length;
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
							return _player.GetSpeed();
						}
						set
						{
							_player.SetSpeed(value);
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
					private GPUAnimation _owner;
					private GPUAnimationPlayer _player;
					private bool _enabled;
					private bool _fading;
					private float _targetWeight;
					private float _fromWeight;
					private float _fadeSpeed;
					private float _fadeLerp;
					private bool _disableAfterFade;
					#endregion

					#region Internal Interface
					internal GPUAnimationState(GPUAnimation owner, GPUAnimations.Animation animation)
					{
						_owner = owner;

						WrapMode wrapMode = animation._wrapMode;

						if (wrapMode == WrapMode.Default)
							wrapMode = WrapMode.Once;

						_player = new GPUAnimationPlayer(animation, wrapMode);
					}

					internal void Update(float deltaTime, GameObject eventListener = null)
					{
						if (_enabled)
						{
							if (_player.Update(deltaTime, eventListener))
							{
								_enabled = false;
							}
						}

						if (_fading)
						{
							_fadeLerp += _fadeSpeed * deltaTime;

							if (_fadeLerp >= 1.0f)
							{
								Weight = _targetWeight;
								_fading = false;

								if (_disableAfterFade)
								{
									_enabled = false;
								}
							}
							else
							{
								Weight = Mathf.Lerp(_fromWeight, _targetWeight, _fadeLerp);
							}
						}
					}

					internal float GetCurrentTexureFrame()
					{
						return _player.GetCurrentTexureFrame();
					}

					internal void FadeWeightTo(float targetWeight = 1.0f, float fadeLength = 0.3f, bool disableOnFade = false)
					{
						_fading = fadeLength > 0.0f;

						if (_fading)
						{
							_targetWeight = targetWeight;
							_fromWeight = Weight;
							_fadeSpeed = 1.0f / fadeLength;
							_fadeLerp = 0.0f;
							_disableAfterFade = disableOnFade;
						}
						else
						{
							Weight = targetWeight;
						}
					}

					internal void CancelFading()
					{
						_fading = false;
					}

					internal GPUAnimations.Animation GetAnimation()
					{
						return _player.GetAnimation();
					}
					#endregion
				}
			}
		}
	}
}