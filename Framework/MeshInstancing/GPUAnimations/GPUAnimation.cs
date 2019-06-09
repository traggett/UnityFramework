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
				public WrapMode _wrapMode;
				public string _clip;
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
				private int _crossFadeAnimationIndex;
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
					return Play(_clip, mode);
				}

				public bool Play(string animation, PlayMode mode = PlayMode.StopSameLayer)
				{
					CancelQueue();
					CancelCrossFade();
					StopAll();

					GPUAnimationState animState = GetAnimationState(animation);

					if (animState != null)
					{
						animState.CancelBlend();
						animState.Enabled = true;
						animState.Time = 0.0f;
						animState.Weight = 1.0f;
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

					_crossFadeAnimationIndex = GetAnimationIndex(animation);
					_crossFadedQueuedAnimation = string.Empty;

					if (_crossFadeAnimationIndex != -1)
					{
						GPUAnimations animations = _renderer._animationTexture.GetAnimations();
						_crossFadedAnimation = new GPUAnimationState(this, animations._animations[_crossFadeAnimationIndex])
						{
							Enabled = true,
							Time = 0.0f,
							Weight = 0.0f
						};
						_crossFadeLength = fadeLength;

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
						_animationStates[i] = new GPUAnimationState(this, animations._animations[i]);
					}

					_primaryAnimationState = GetAnimationState(_clip);
					_secondaryAnimationState = null;

					if (_primaryAnimationState != null)
					{
						_primaryAnimationState.Enabled = true;
						_primaryAnimationState.Time = 0.0f;
						_primaryAnimationState.Weight = 1.0f;
					}
				}

				private void UpdateAnimations()
				{
					for (int i = 0; i < _animationStates.Length; i++)
					{
						_animationStates[i].Update(Time.deltaTime);
					}

					UpdateQueuedAnimation();
					UpdateCrossFadedAnimation();
					UpdatePlayers();
				}

				private void UpdatePlayers()
				{
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

				private int GetAnimationIndex(string animationName)
				{
					GPUAnimations animations = _renderer._animationTexture.GetAnimations();

					for (int i=0; i< animations._animations.Length; i++)
					{
						if (animations._animations[i]._name == animationName)
						{
							return i;
						}
					}
					
					return -1;
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
					//Wait for queued animations
					if (!string.IsNullOrEmpty(_crossFadedQueuedAnimation) && AreAnimationsFinished(_crossFadedAnimationQueueMode))
					{
						CrossFade(_crossFadedQueuedAnimation, _crossFadeLength);
					}

					if (_crossFadedAnimation != null && _crossFadedAnimation.Enabled && _crossFadedAnimation.Weight >= 1.0f)
					{
						//Once faded in set crossfaded state data on orig state
						_animationStates[_crossFadeAnimationIndex].Time = _crossFadedAnimation.Time;
						_animationStates[_crossFadeAnimationIndex].Speed = _crossFadedAnimation.Speed;
						_animationStates[_crossFadeAnimationIndex].Weight = 1.0f;

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