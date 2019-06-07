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
				//public AnimationState this[string name] { get; }
				#endregion

				#region Private Data
				private SkinnedMeshRenderer _skinnedMeshRenderer;
				private float _currentAnimationWeight;
				private GPUAnimationPlayer[] _clipPlayers;

				private GPUAnimationState[] _animationStates;

				private string _queuedAnimation;
				private QueueMode _queuedAnimationMode;

				private GPUAnimationState _crossFadedAnimation;
				private int _crossFadeAnimationIndex;
				private float _crossFadeLength;
				private QueueMode _crossFadedAnimationQueueMode;
				#endregion

				#region MonoBehaviour
				private void Awake()
				{
					_skinnedMeshRenderer = GameObjectUtils.GetComponent<SkinnedMeshRenderer>(this.gameObject, true);
					_clipPlayers = new GPUAnimationPlayer[2];
					_animationStates = new GPUAnimationState[0];
					_currentAnimationWeight = 1.0f;

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

					int animIndex = GetAnimationIndex(animation);

					if (animIndex != -1)
					{
						_animationStates[animIndex].Enabled = true;
						_animationStates[animIndex].Time = 0.0f;
						_animationStates[animIndex].Weight = 1.0f;

						_currentAnimationWeight = 1.0f;
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

					if (_crossFadeAnimationIndex != -1)
					{
						GPUAnimations animations = _renderer._animationTexture.GetAnimations();
						_crossFadedAnimation = new GPUAnimationState(animations._animations[_crossFadeAnimationIndex])
						{
							Enabled = true,
							Time = 0.0f,
							Weight = 0.0f
						};
						_crossFadeLength = fadeLength;
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

					_crossFadeAnimationIndex = GetAnimationIndex(animation);

					if (_crossFadeAnimationIndex != -1)
					{
						GPUAnimations animations = _renderer._animationTexture.GetAnimations();
						_crossFadedAnimation = new GPUAnimationState(animations._animations[_crossFadeAnimationIndex])
						{
							Enabled = false,
							Time = 0.0f,
							Weight = 0.0f
						};
						_crossFadeLength = fadeLength;
						_crossFadedAnimationQueueMode = queue;
					}
					else
					{
						_crossFadedAnimation = null;
					}

					return _crossFadedAnimation;
				}

				public void Blend(string animation, float targetWeight = 1.0f, float fadeLength = 0.3f)
				{
					//blends anim weight to target over time
				}

				public bool IsPlaying(string animation)
				{
					return true;
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
					return _clipPlayers[0].GetCurrentTexureFrame();
				}

				public override float GetCurrentAnimationWeight()
				{
					return _currentAnimationWeight;
				}

				public override float GetPreviousAnimationFrame()
				{
					return _clipPlayers[1].GetCurrentTexureFrame();
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
					_clipPlayers[0].Stop();
					_clipPlayers[1].Stop();
					_currentAnimationWeight = 1.0f;

					//Create animation states for all animations
					GPUAnimations animations = _renderer._animationTexture.GetAnimations();
					_animationStates = new GPUAnimationState[animations._animations.Length];
					
					for (int i=0; i< animations._animations.Length; i++)
					{
						_animationStates[i] = new GPUAnimationState(animations._animations[i]);
					}
				}

				private void UpdateAnimations()
				{
					for (int i = 0; i < _animationStates.Length; i++)
					{
						if (_animationStates[i].Enabled)
						{
							_animationStates[i].Time += Time.deltaTime * _animationStates[i].Speed;
						}
					}

					if (_crossFadedAnimation.Enabled)
					{
						_crossFadedAnimation.Time += Time.deltaTime * _crossFadedAnimation.Speed;
					}

					UpdateQueuedAnimation();
					UpdateCrossFadedAnimation();
					UpdatePlayers();
				}

				private void UpdatePlayers()
				{
					GPUAnimationState primaryState = _crossFadedAnimation;
					GPUAnimationState secondaryState = null;

					for (int i = 0; i < _animationStates.Length; i++)
					{
						if (_animationStates[i].Enabled)
						{
							if (primaryState == null || _animationStates[i].Weight > primaryState.Weight)
							{
								secondaryState = primaryState;
								primaryState = _animationStates[i];
							}
							else if (secondaryState == null || _animationStates[i].Weight > secondaryState.Weight)
							{
								secondaryState = _animationStates[i];
							}
						}
					}

					//TO DO! should only trigger animation events on players that were playing same anim last frame???
					//Or have players for all states? so anim events happen???
					//Then return two?
					if (primaryState != null)
					{
						_clipPlayers[0].Play(this.gameObject, primaryState._animation, 0.0f);
						_clipPlayers[0].SetNormalizedTime(primaryState.NormalizedTime);
						_currentAnimationWeight = primaryState.Weight;
					}
					else
					{
						_clipPlayers[0].Stop();
						_currentAnimationWeight = 1.0f;
					}

					if (secondaryState != null)
					{
						_clipPlayers[1].Play(this.gameObject, secondaryState._animation, 0.0f);
						_clipPlayers[1].SetNormalizedTime(secondaryState.NormalizedTime);
					}
					else
					{
						_clipPlayers[1].Stop();
					}
				}

				private GPUAnimationState GetAnimationState(string animationName)
				{
					for (int i = 0; i < _animationStates.Length; i++)
					{
						if (_animationStates[i]._animation._name == animationName)
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
					if (_crossFadedAnimation != null)
					{
						if (!_crossFadedAnimation.Enabled && AreAnimationsFinished(_crossFadedAnimationQueueMode))
						{
							_crossFadedAnimation.Enabled = true;
						}

						if (_crossFadedAnimation.Enabled)
						{
							//Fade up cross fade animation
							if (_crossFadeLength > 0.0f)
							{
								_crossFadedAnimation.Weight += Time.deltaTime / _crossFadeLength;
							}
							else
							{
								_crossFadedAnimation.Weight = 1.0f;
							}

							//Lerp down others
							for (int i = 0; i < _animationStates.Length; i++)
							{
								if (_animationStates[i].Enabled)
								{
									_animationStates[i].Weight = Mathf.Lerp(_animationStates[i].Weight, 0.0f, _crossFadedAnimation.Weight);
								}
							}

							//Once faded in set crossfaded state data on orig state
							if (_crossFadedAnimation.Weight >= 1.0f)
							{
								_animationStates[_crossFadeAnimationIndex].Time = _crossFadedAnimation.Time;
								_animationStates[_crossFadeAnimationIndex].Speed = _crossFadedAnimation.Speed;
								_animationStates[_crossFadeAnimationIndex].Weight = 1.0f;

								_crossFadedAnimation = null;
							}
						}
					}
				}

				private void CancelQueue()
				{
					_queuedAnimation = string.Empty;
				}

				private void CancelCrossFade()
				{
					_crossFadedAnimation = null;
				}
				#endregion
			}
		}
    }
}