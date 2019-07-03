using UnityEngine;
using System.Collections;

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
				public bool isPlaying
				{
					get
					{
						for (int i = 0; i < _animationStates.Length; i++)
						{
							if (_animationStates[i].Enabled)
								return true;
						}

						return false;
					}
				}
				#endregion

				#region Private Data
				private sealed class Enumerator : IEnumerator
				{
					private GPUAnimation _outer;

					private int _currentIndex = -1;

					public object Current => _outer.GetStateAtIndex(_currentIndex);

					internal Enumerator(GPUAnimation outer)
					{
						_outer = outer;
					}

					public bool MoveNext()
					{
						int stateCount = _outer.GetStateCount();
						_currentIndex++;
						return _currentIndex < stateCount;
					}

					public void Reset()
					{
						_currentIndex = -1;
					}
				}

				private SkinnedMeshRenderer _skinnedMeshRenderer;
				private GPUAnimationState _primaryAnimationState;
				private GPUAnimationState _secondaryAnimationState;
				
				private GPUAnimationState[] _animationStates;
				private GPUAnimationState _crossFadedAnimation;

				private string _queuedAnimation;
				private QueueMode _queuedAnimationMode;
				private PlayMode _queuedAnimationPlayMode;
				private float _queuedAnimationCrossFadeLength;
				#endregion

				#region MonoBehaviour
				private void Awake()
				{
					_skinnedMeshRenderer = GameObjectUtils.GetComponent<SkinnedMeshRenderer>(this.gameObject, true);
					_animationStates = new GPUAnimationState[0];
					_primaryAnimationState = null;
					_secondaryAnimationState = null;
					
					_onInitialise += Initialise;

					CachedTransformData(this.transform);
				}

				private void Update()
				{
					UpdateAnimations();
				}

				private void LateUpdate()
				{
					UpdatePlayers();
				}
				#endregion

				#region Public Interface
				public bool Play(PlayMode mode = PlayMode.StopSameLayer, WrapMode wrapMode = WrapMode.Default)
				{
					return Play(_defaultAnimation, mode, wrapMode);
				}

				public bool Play(string animation, PlayMode mode = PlayMode.StopSameLayer, WrapMode wrapMode = WrapMode.Default)
				{
					Stop();

					GPUAnimationState animState = GetAnimationState(animation);

					if (animState != null)
					{
						animState.Enabled = true;
						animState.Time = 0.0f;
						animState.Weight = 1.0f;

						if (wrapMode != WrapMode.Default)
						{
							animState.WrapMode = wrapMode;
						}
						else if (_wrapMode != WrapMode.Default)
						{
							animState.WrapMode = _wrapMode;
						}

						UpdatePlayers();

						return true;
					}
					else
					{
						return false;
					}
				}
				
				public void Stop()
				{
					CancelCrossFade();
					CancelQueue();

					for (int i = 0; i < _animationStates.Length; i++)
					{
						_animationStates[i].Enabled = false;
					}
				}

				public GPUAnimationState PlayQueued(string animation, QueueMode queue = QueueMode.CompleteOthers, PlayMode mode = PlayMode.StopSameLayer)
				{
					CancelCrossFade();

					_queuedAnimation = animation;
					_queuedAnimationMode = queue;
					_queuedAnimationPlayMode = mode;
					_queuedAnimationCrossFadeLength = -1.0f;

					return GetAnimationState(animation);
				}

				public GPUAnimationState CrossFade(string animation, float fadeLength = 0.3f, PlayMode mode = PlayMode.StopSameLayer)
				{
					if (fadeLength > 0.0f)
					{
						CancelQueue();

						GPUAnimationState animState = GetAnimationState(animation);

						if (animState != null)
						{
							GPUAnimations animations = _renderer._animationTexture.GetAnimations();

							_crossFadedAnimation = new GPUAnimationState(animState.GetAnimation())
							{
								Enabled = true,
								Weight = 0.0f
							};

							if (_wrapMode != WrapMode.Default)
								_crossFadedAnimation.WrapMode = _wrapMode;
							
							_crossFadedAnimation.FadeWeightTo(1.0f, fadeLength);
						}
						else
						{
							_crossFadedAnimation = null;
						}
					}
					else
					{
						Play(animation, mode);
					}

					return _crossFadedAnimation;
				}

				public GPUAnimationState CrossFadeQueued(string animation, float fadeLength = 0.3f, QueueMode queue = QueueMode.CompleteOthers, PlayMode mode = PlayMode.StopSameLayer)
				{
					CancelQueue();

					_queuedAnimation = animation;
					_queuedAnimationMode = queue;
					_queuedAnimationPlayMode = mode;
					_queuedAnimationCrossFadeLength = fadeLength;

					_crossFadedAnimation = null;

					//TO DO! return valid state??
					return _crossFadedAnimation;
				}

				public void Blend(string animation, float targetWeight = 1.0f, float fadeLength = 0.3f)
				{
					GPUAnimationState state = GetAnimationState(animation);

					if (state != null)
					{
						state.FadeWeightTo(targetWeight, fadeLength);
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
				public override float GetMainAnimationFrame()
				{
					return _primaryAnimationState != null ? _primaryAnimationState.GetCurrentTexureFrame() : 0.0f;
				}

				public override float GetMainAnimationWeight()
				{
					return _primaryAnimationState != null ? _primaryAnimationState.Weight : 1.0f;
				}

				public override float GetBackgroundAnimationFrame()
				{
					return _secondaryAnimationState != null ? _secondaryAnimationState.GetCurrentTexureFrame() : 0.0f;
				}

				public override Bounds GetBounds()
				{
					if (_skinnedMeshRenderer != null)
						return _skinnedMeshRenderer.bounds;

					return new Bounds();
				}

				public IEnumerator GetEnumerator()
				{
					return new Enumerator(this);
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

						if (_wrapMode != WrapMode.Default)
							_primaryAnimationState.WrapMode = _wrapMode;
					}
				}

				private void UpdateAnimations()
				{
					if (_crossFadedAnimation != null)
					{
						_crossFadedAnimation.Update(Time.deltaTime, true, this.gameObject);
					}

					for (int i = 0; i < _animationStates.Length; i++)
					{
						_animationStates[i].Update(Time.deltaTime, true, this.gameObject);
					}

					UpdateQueuedAnimation();
					UpdateCrossFadedAnimation();
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
							if (_crossFadedAnimation == null && (_primaryAnimationState == null || _animationStates[i].Weight > _primaryAnimationState.Weight))
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
					if (_crossFadedAnimation != null && _crossFadedAnimation.Name == animationName)
						return _crossFadedAnimation;

					for (int i = 0; i < _animationStates.Length; i++)
					{
						if (_animationStates[i].Name == animationName)
							return _animationStates[i];
					}

					return null;
				}
				
				private void Stop(int animIndex)
				{
					_animationStates[animIndex].Enabled = false;
				}

				private bool AreAnimationsFinished(QueueMode queueMode, float fadeLength)
				{
					if (queueMode == QueueMode.CompleteOthers)
					{
						for (int i = 0; i < _animationStates.Length; i++)
						{
							if (_animationStates[i].Enabled)
							{
								if (fadeLength > 0.0f)
								{
									if (_animationStates[i].Length - _animationStates[i].Time > fadeLength)
										return false;
								}
								else if (_animationStates[i].NormalizedTime < 1.0f)
								{
									return false;
								}
							}
						}
					}

					return true;
				}

				private void UpdateQueuedAnimation()
				{
					if (!string.IsNullOrEmpty(_queuedAnimation) && AreAnimationsFinished(_queuedAnimationMode, _queuedAnimationCrossFadeLength))
					{
						if (_queuedAnimationCrossFadeLength > 0.0f)
						{
							CrossFade(_queuedAnimation, _queuedAnimationCrossFadeLength);
						}
						else
						{
							Play(_queuedAnimation, _queuedAnimationPlayMode);
						}
					}
				}

				private void UpdateCrossFadedAnimation()
				{
					if (_crossFadedAnimation != null && _crossFadedAnimation.Enabled && _crossFadedAnimation.Weight >= 1.0f)
					{
						for (int i=0; i<_animationStates.Length; i++)
						{
							if (_animationStates[i].Name == _crossFadedAnimation.Name)
							{
								_animationStates[i].Time = _crossFadedAnimation.Time;
								_animationStates[i].Speed = _crossFadedAnimation.Speed;
								_animationStates[i].WrapMode = _crossFadedAnimation.WrapMode;
								_animationStates[i].Weight = 1.0f;
								_animationStates[i].Enabled = true;
							}
							else
							{
								_animationStates[i].Enabled = false;
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
				}

				private GPUAnimationState GetStateAtIndex(int index)
				{
					if (_crossFadedAnimation != null)
					{
						return index == 0 ? _crossFadedAnimation : _animationStates[index - 1];
					}

					return _animationStates[index];
				}

				private int GetStateCount()
				{
					return _crossFadedAnimation != null ? _animationStates.Length + 1 : _animationStates.Length;
				}
				#endregion
			}
		}
    }
}