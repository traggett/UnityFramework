using UnityEngine;

namespace Framework
{
	using Utils;

	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public class GPUAnimation : GPUAnimatorBase
			{
				#region Public Data
				public string _defaultAnimation;
				public bool _playAutomatically;
				public WrapMode _wrapMode;
				public GPUAnimationState this[string name]
				{
					get
					{
						return GetAnimationState(name);
					}
				}
				#endregion

				#region Private Data
				private SkinnedMeshRenderer _skinnedMeshRenderer;
				private GPUAnimationState _primaryAnimationState;
				private GPUAnimationState _secondaryAnimationState;
				
				private GPUAnimationState[] _animationStates;

				private string _queuedAnimation;
				private QueueMode _queuedAnimationMode;

				private GPUAnimationState _crossFadedAnimation;
				private float _crossFadeLength;
				private string _crossFadedQueuedAnimation;
				private QueueMode _crossFadedAnimationQueueMode;
				#endregion

				#region MonoBehaviour
				private void Awake()
				{
					_skinnedMeshRenderer = GameObjectUtils.GetComponent<SkinnedMeshRenderer>(this.gameObject, true);
					_animationStates = new GPUAnimationState[0];
					_primaryAnimationState = null;
					_secondaryAnimationState = null;
					
					_onInitialise += Initialise;

					UpdateCachedTransform();
				}

				private void Update()
				{
					UpdateAnimations();
				}
				#endregion

				#region Public Interface
				public bool Play(PlayMode mode = PlayMode.StopSameLayer)
				{
					return Play(_defaultAnimation, mode);
				}

				public bool Play(string animation, PlayMode mode = PlayMode.StopSameLayer)
				{
					CancelQueue();
					CancelCrossFade();
					StopAll();

					GPUAnimationState animState = GetAnimationState(animation);

					if (animState != null)
					{
						animState.Enabled = true;
						animState.Time = 0.0f;
						animState.Weight = 1.0f;

						if (_wrapMode != WrapMode.Default)
							animState.WrapMode = _wrapMode;

						return true;
					}
					else
					{
						return false;
					}
				}
				
				public GPUAnimationState PlayQueued(string animation, QueueMode queue = QueueMode.CompleteOthers, PlayMode mode = PlayMode.StopSameLayer)
				{
					CancelCrossFade();

					_queuedAnimation = animation;
					_queuedAnimationMode = queue;

					return GetAnimationState(animation);
				}

				public GPUAnimationState CrossFade(string animation, float fadeLength = 0.3f, PlayMode mode = PlayMode.StopSameLayer)
				{
					CancelQueue();

					GPUAnimationState animState = GetAnimationState(animation);
					_crossFadedQueuedAnimation = string.Empty;

					if (animState != null)
					{
						GPUAnimations animations = _renderer._animationTexture.GetAnimations();
						_crossFadedAnimation = new GPUAnimationState(animState.GetAnimation())
						{
							Enabled = true,
							Weight = 0.0f
						};
						_crossFadeLength = fadeLength;

						if (_wrapMode != WrapMode.Default)
							_crossFadedAnimation.WrapMode = _wrapMode;

						_crossFadedAnimation.BlendWeightTo(1.0f, fadeLength);
						
						for (int i = 0; i < _animationStates.Length; i++)
						{
							_animationStates[i].BlendWeightTo(0.0f, fadeLength);
						}
					}
					else
					{
						_crossFadedAnimation = null;
					}

					return _crossFadedAnimation;
				}

				public GPUAnimationState CrossFadeQueued(string animation, float fadeLength = 0.3f, QueueMode queue = QueueMode.CompleteOthers, PlayMode mode = PlayMode.StopSameLayer)
				{
					CancelQueue();

					_crossFadedQueuedAnimation = animation;
					_crossFadedAnimationQueueMode = queue;
					_crossFadedAnimation = null;

					//TO DO! return valid state??
					return _crossFadedAnimation;
				}

				public void Blend(string animation, float targetWeight = 1.0f, float fadeLength = 0.3f)
				{
					GPUAnimationState state = GetAnimationState(animation);

					if (state != null)
					{
						state.BlendWeightTo(targetWeight, fadeLength);
					}
				}

				public bool IsPlaying(string animation)
				{
					GPUAnimationState state = GetAnimationState(animation);

					if (state != null)
					{
						return state.Enabled;
					}

					return false;
				}

				public void Rewind(string animation)
				{
					GPUAnimationState state = GetAnimationState(animation);

					if (state != null)
					{
						state.Time = 0.0f;
					}
				}


				public void Rewind()
				{
					for (int i = 0; i < _animationStates.Length; i++)
					{
						_animationStates[i].Time = 0.0f;
					}
				}
				#endregion

				#region GPUAnimatorBase
				public override float GetCurrentAnimationFrame()
				{
					return _primaryAnimationState != null ? _primaryAnimationState.GetCurrentTexureFrame() : 0.0f;
				}

				public override float GetCurrentAnimationWeight()
				{
					return _primaryAnimationState != null ? _primaryAnimationState.Weight : 1.0f;
				}

				public override float GetPreviousAnimationFrame()
				{
					return _secondaryAnimationState != null ? _secondaryAnimationState.GetCurrentTexureFrame() : 0.0f;
				}

				public override Bounds GetBounds()
				{
					if (_skinnedMeshRenderer != null)
						return _skinnedMeshRenderer.bounds;

					return new Bounds();
				}
				#endregion

				#region Private Functions
				private void Initialise()
				{
					GPUAnimations animations = _renderer._animationTexture.GetAnimations();
					_animationStates = new GPUAnimationState[animations._animations.Length];
					
					for (int i=0; i< animations._animations.Length; i++)
					{
						_animationStates[i] = new GPUAnimationState(animations._animations[i]);
					}

					_primaryAnimationState = GetAnimationState(_defaultAnimation);
					_secondaryAnimationState = null;

					if (_primaryAnimationState != null && _playAutomatically)
					{
						_primaryAnimationState.Enabled = true;
						_primaryAnimationState.Weight = 1.0f;
					}
				}

				private void UpdateAnimations()
				{
					for (int i = 0; i < _animationStates.Length; i++)
					{
						_animationStates[i].Update(Time.deltaTime, true, this.gameObject);
					}

					UpdateQueuedAnimation();
					UpdateCrossFadedAnimation();
					UpdatePlayers();
				}

				private void UpdatePlayers()
				{
					//Work out what animation state has the highest and second highest weight
					_primaryAnimationState = _crossFadedAnimation;
					_secondaryAnimationState = null;

					for (int i = 0; i < _animationStates.Length; i++)
					{
						if (_animationStates[i].Enabled)
						{
							if (_primaryAnimationState == null || _animationStates[i].Weight > _primaryAnimationState.Weight)
							{
								_secondaryAnimationState = _primaryAnimationState;
								_primaryAnimationState = _animationStates[i];
							}
							else if (_secondaryAnimationState == null || _animationStates[i].Weight > _secondaryAnimationState.Weight)
							{
								_secondaryAnimationState = _animationStates[i];
							}
						}
					}
				}

				private GPUAnimationState GetAnimationState(string animationName)
				{
					for (int i = 0; i < _animationStates.Length; i++)
					{
						if (_animationStates[i].Name == animationName)
							return _animationStates[i];
					}

					return null;
				}
				
				private void StopAll()
				{
					for (int i = 0; i < _animationStates.Length; i++)
					{
						Stop(i);
					}
				}

				private void Stop(int animIndex)
				{
					_animationStates[animIndex].Enabled = false;
				}

				private bool AreAnimationsFinished(QueueMode queueMode)
				{
					if (queueMode == QueueMode.CompleteOthers)
					{
						for (int i = 0; i < _animationStates.Length; i++)
						{
							if (_animationStates[i].Enabled && _animationStates[i].NormalizedTime < 0.0f)
							{
								return false;
							}
						}
					}

					return true;
				}

				private void UpdateQueuedAnimation()
				{
					if (!string.IsNullOrEmpty(_queuedAnimation) && AreAnimationsFinished(_queuedAnimationMode))
					{
						Play(_queuedAnimation);
					}
				}

				private void UpdateCrossFadedAnimation()
				{
					if (!string.IsNullOrEmpty(_crossFadedQueuedAnimation) && AreAnimationsFinished(_crossFadedAnimationQueueMode))
					{
						CrossFade(_crossFadedQueuedAnimation, _crossFadeLength);
					}

					if (_crossFadedAnimation != null && _crossFadedAnimation.Enabled && _crossFadedAnimation.Weight >= 1.0f)
					{
						for (int i=0; i<_animationStates.Length; i++)
						{
							if (_animationStates[i].Name == _crossFadedAnimation.Name)
							{
								_animationStates[i].Time = _crossFadedAnimation.Time;
								_animationStates[i].Speed = _crossFadedAnimation.Speed;
								_animationStates[i].Weight = 1.0f;
								break;
							}
						}

						_crossFadedAnimation = null;
					}
				}

				private void CancelQueue()
				{
					_queuedAnimation = string.Empty;
				}

				private void CancelCrossFade()
				{
					_crossFadedAnimation = null;
					_crossFadedQueuedAnimation = string.Empty;
				}
				#endregion
			}
		}
    }
}