using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public sealed class GPUAnimationState
			{
				#region Public Data
				public bool Enabled { get; set; }
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
				private GPUAnimationPlayer _player;
				private bool _fading;
				private float _targetWeight;
				private float _fromWeight;
				private float _fadeSpeed;
				private float _fadeLerp;
				private bool _disableAfterFade;
				#endregion

				#region Public Interface
				public GPUAnimationState(GPUAnimations.Animation animation)
				{
					WrapMode wrapMode = animation._wrapMode;

					if (wrapMode == WrapMode.Default)
						wrapMode = WrapMode.Once;

					_player = new GPUAnimationPlayer(animation, wrapMode);
				}

				public void Update(float deltaTime, bool checkForEvents = false, GameObject eventListener = null)
				{
					if (Enabled)
					{
						//If animation is finished, disable this state
						if (_player.Update(deltaTime, checkForEvents, eventListener))
						{
							Enabled = false;
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
								Enabled = false;
							}
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

				public void BlendWeightTo(float targetWeight = 1.0f, float fadeLength = 0.3f, bool disableOnFade = false)
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

				public void CancelBlend()
				{
					_fading = false;
				}

				public GPUAnimations.Animation GetAnimation()
				{
					return _player.GetAnimation();
				}
				#endregion
			}
		}
    }
}