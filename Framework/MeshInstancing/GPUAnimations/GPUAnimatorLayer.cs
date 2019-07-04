using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public struct GPUAnimatorLayer
			{
				#region Private Data	
				private readonly Animator _animator;
				private readonly int _layer;
				private readonly GPUAnimationPlayer[] _clipPlayers;
				private readonly int[] _clipPlayerStates;
				private float _weight;
				private int _mainPlayerIndex;
				private float _mainAnimationWeight;
				#endregion

				#region Public Interface
				public GPUAnimatorLayer(Animator animator, int layer)
				{
					_animator = animator;
					_layer = layer;
					_clipPlayers = new GPUAnimationPlayer[2];
					_clipPlayerStates = new int[2];
					_weight = 1.0f;
					_mainPlayerIndex = 0;
					_mainAnimationWeight = 1.0f;
				}

				public float GetMainAnimationFrame()
				{
					return _clipPlayers[_mainPlayerIndex].GetCurrentTexureFrame();
				}

				public float GetMainAnimationWeight()
				{
					return _mainAnimationWeight;
				}

				public float GetBackgroundAnimationFrame()
				{
					return _clipPlayers[1 - _mainPlayerIndex].GetCurrentTexureFrame();
				}

				public float GetWeight()
				{
					return _weight;
				}
				
				public void Update()
				{
					_weight = _animator.GetLayerWeight(_layer);

					GameObject eventListener = _animator.gameObject;

					//Check if we're transitioning - TO DO! transition from state to same state seem broken / cause pops
					if (_animator.IsInTransition(_layer))
					{
						AnimatorStateInfo previousState = _animator.GetCurrentAnimatorStateInfo(_layer);
						AnimatorStateInfo nextState = _animator.GetNextAnimatorStateInfo(_layer);
						AnimatorTransitionInfo transitionInfo = _animator.GetAnimatorTransitionInfo(_layer);

						//Check current player is playing the next state
						if (_clipPlayerStates[_mainPlayerIndex] != nextState.shortNameHash)
						{
							//If not switch current player index and start animation on player
							_mainPlayerIndex = 1 - _mainPlayerIndex;
							_clipPlayerStates[_mainPlayerIndex] = nextState.shortNameHash;

							AnimatorClipInfo[] nextClips = _animator.GetNextAnimatorClipInfo(_layer);
							PlayAnimation(nextState, nextClips, _mainPlayerIndex);
						}

						//Then check previous player is playing the previous state
						if (_clipPlayerStates[1 - _mainPlayerIndex] != previousState.shortNameHash)
						{
							_clipPlayerStates[1 - _mainPlayerIndex] = previousState.shortNameHash;

							AnimatorClipInfo[] previousClips = _animator.GetCurrentAnimatorClipInfo(_layer);
							PlayAnimation(previousState, previousClips, 1 - _mainPlayerIndex);
						}

						//Update times
						_clipPlayers[_mainPlayerIndex].SetNormalizedTime(nextState.normalizedTime, eventListener);
						_clipPlayers[1 - _mainPlayerIndex].SetNormalizedTime(previousState.normalizedTime, eventListener);
						_mainAnimationWeight = transitionInfo.normalizedTime;
					}
					//Otherwise just update current animation
					else
					{
						AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(_layer);

						if (_clipPlayerStates[_mainPlayerIndex] != currentState.shortNameHash)
						{
							_clipPlayerStates[_mainPlayerIndex] = currentState.shortNameHash;
							AnimatorClipInfo[] currentClips = _animator.GetCurrentAnimatorClipInfo(_layer);
							PlayAnimation(currentState, currentClips, _mainPlayerIndex);

							//Stop other player
							_clipPlayers[1 - _mainPlayerIndex] = new GPUAnimationPlayer();
						}

						//Update time
						_clipPlayers[_mainPlayerIndex].SetNormalizedTime(currentState.normalizedTime, eventListener);
						_mainAnimationWeight = 1.0f;
					}
				}
				
				public void GetRootMotionVelocities(out Vector3 velocity, out Vector3 angularVelocity)
				{
					_clipPlayers[_mainPlayerIndex].GetRootMotionVelocities(out velocity, out angularVelocity);

					if (_mainAnimationWeight < 1.0f)
					{
						_clipPlayers[1 - _mainPlayerIndex].GetRootMotionVelocities(out Vector3 previousPlayerVelocity, out Vector3 previousPlayerAngularVelocity);

						velocity = Vector3.Lerp(previousPlayerVelocity, velocity, _mainAnimationWeight);
						angularVelocity = Vector3.Lerp(previousPlayerAngularVelocity, angularVelocity, _mainAnimationWeight);
					}
				}
				#endregion

				#region Private Functions
				private void PlayAnimation(AnimatorStateInfo state, AnimatorClipInfo[] clips, int playerIndex)
				{
					if (clips.Length > 0)
					{
						AnimationClip clip = clips[0].clip;
						GPUAnimations.Animation animation = ((GPUAnimatorOverrideController)_animator.runtimeAnimatorController).GetAnimation(clip);
						_clipPlayers[playerIndex] = new GPUAnimationPlayer(animation, animation._wrapMode, 0.0f);
					}
					else
					{
						_clipPlayers[playerIndex] = new GPUAnimationPlayer();
					}
				}
				#endregion
			}
		}
    }
}