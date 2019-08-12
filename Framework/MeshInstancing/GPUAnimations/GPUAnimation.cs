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
						if (_crossFadedAnimation != null && _crossFadedAnimation.Name == name)
							return _crossFadedAnimation;

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
				private void Update()
				{
					UpdateAnimations(Time.deltaTime);
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
					ClearCrossFadedAnimation();
					ClearQueuedAnimation();

					for (int i = 0; i < _animationStates.Length; i++)
					{
						_animationStates[i].Enabled = false;
					}

					_primaryAnimationState = null;
					_secondaryAnimationState = null;
				}

				public void Stop(string animation)
				{
					GPUAnimationState state = GetAnimationState(animation);

					if (state != null)
					{
						state.Enabled = false;
						UpdatePlayers();
					}
				}

				public GPUAnimationState PlayQueued(string animation, QueueMode queue = QueueMode.CompleteOthers, PlayMode mode = PlayMode.StopSameLayer)
				{
					ClearCrossFadedAnimation();

					_queuedAnimation = animation;
					_queuedAnimationMode = queue;
					_queuedAnimationPlayMode = mode;
					_queuedAnimationCrossFadeLength = -1.0f;

					//TO DO! return valid state??
					return GetAnimationState(animation);
				}

				public GPUAnimationState CrossFade(string animation, float fadeLength = 0.3f, PlayMode mode = PlayMode.StopSameLayer)
				{
					if (fadeLength > 0.0f)
					{
						ClearQueuedAnimation();

						GPUAnimationState animState = GetAnimationState(animation);

						if (animState != null)
						{
							_crossFadedAnimation = new GPUAnimationState(animState.GetAnimation())
							{
								Enabled = true,
								Weight = 0.0f,
								Speed = animState.Speed,
								WrapMode = animState.WrapMode
							};
							
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
					ClearQueuedAnimation();

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
					GPUAnimationState state = this[animation];

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
				public override void Initialise(GPUAnimatorRenderer renderer)
				{
					base.Initialise(renderer);

					_skinnedMeshRenderer = GameObjectUtils.GetComponent<SkinnedMeshRenderer>(this.gameObject, true);
					_animationStates = new GPUAnimationState[0];
					_primaryAnimationState = null;
					_secondaryAnimationState = null;

					GPUAnimations animations = _renderer._animations.GetAnimations();
					_animationStates = new GPUAnimationState[animations._animations.Length];

					for (int i = 0; i < animations._animations.Length; i++)
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
				private void UpdateAnimations(float deltaTime)
				{
					if (_crossFadedAnimation != null)
					{
						_crossFadedAnimation.Update(deltaTime, this.gameObject);
					}

					for (int i = 0; i < _animationStates.Length; i++)
					{
						_animationStates[i].Update(deltaTime, this.gameObject);
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
					for (int i = 0; i < _animationStates.Length; i++)
					{
						if (_animationStates[i].Name == animationName)
							return _animationStates[i];
					}

					return null;
				}
				
				private void UpdateQueuedAnimation()
				{
					if (!string.IsNullOrEmpty(_queuedAnimation))
					{
						bool queuedAnimationReady;
						bool queuedAnimationCrossFaded = _queuedAnimationCrossFadeLength > 0.0f;
						float timeRemaining = 0.0f;

						switch (_queuedAnimationMode)
						{
							case QueueMode.CompleteOthers:
								{
									queuedAnimationReady = true;

									for (int i = 0; i < _animationStates.Length; i++)
									{
										if (_animationStates[i].Enabled)
										{
											if (queuedAnimationCrossFaded)
											{
												timeRemaining = Mathf.Max(_animationStates[i].Length - _animationStates[i].Time, timeRemaining);

												if (timeRemaining > _queuedAnimationCrossFadeLength)
												{
													queuedAnimationReady = false;
													break;
												}
											}
											else if (_animationStates[i].NormalizedTime < 1.0f)
											{
												queuedAnimationReady = false;
												break;
											}
										}
									}
								}
								break;
							case QueueMode.PlayNow:
							default:
								{
									queuedAnimationReady = true;
								}
								break;
						}

						if (queuedAnimationReady)
						{
							//Note - _queuedAnimation will get cleared by the CrossFade or Play call
							if (queuedAnimationCrossFaded)
							{
								CrossFade(_queuedAnimation, timeRemaining);
							}
							else
							{
								Play(_queuedAnimation, _queuedAnimationPlayMode);
							}
						}
					}
				}

				private void UpdateCrossFadedAnimation()
				{
					if (_crossFadedAnimation != null && _crossFadedAnimation.Weight >= 1.0f)
					{
						for (int i=0; i<_animationStates.Length; i++)
						{
							if (_animationStates[i].Name == _crossFadedAnimation.Name)
							{
								_animationStates[i].Time = _crossFadedAnimation.Time;
								_animationStates[i].Speed = _crossFadedAnimation.Speed;
								_animationStates[i].WrapMode = _crossFadedAnimation.WrapMode;
								_animationStates[i].Enabled = _crossFadedAnimation.Enabled;
								_animationStates[i].Weight = 1.0f;
							}
							else
							{
								_animationStates[i].Enabled = false;
							}
						}

						_crossFadedAnimation = null;
					}
				}

				private void ClearQueuedAnimation()
				{
					_queuedAnimation = string.Empty;
				}

				private void ClearCrossFadedAnimation()
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
					return _crossFadedAnimation != null ? 1 + _animationStates.Length : _animationStates.Length;
				}
				#endregion
			}
		}
    }
}