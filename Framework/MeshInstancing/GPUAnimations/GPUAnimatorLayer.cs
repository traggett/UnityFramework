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
				private int _currentPlayerIndex;
				private float _currentAnimationWeight;
				#endregion

				#region Public Interface
				public GPUAnimatorLayer(Animator animator, int layer)
				{
					_animator = animator;
					_layer = layer;
					_clipPlayers = new GPUAnimationPlayer[2];
					_clipPlayerStates = new int[2];
					_currentPlayerIndex = 0;
					_currentAnimationWeight = 1.0f;
				}

				public float GetCurrentAnimationFrame()
				{
					return _clipPlayers[_currentPlayerIndex].GetCurrentTexureFrame();
				}

				public float GetCurrentAnimationWeight()
				{
					return _currentAnimationWeight;
				}

				public float GetPreviousAnimationFrame()
				{
					return _clipPlayers[1 - _currentPlayerIndex].GetCurrentTexureFrame();
				}
				
				public void Update()
				{
					//Check if we're transitioning - TO DO! transition from state to same state seem broken / cause pops
					if (_animator.IsInTransition(_layer))
					{
						AnimatorStateInfo previousState = _animator.GetCurrentAnimatorStateInfo(_layer);
						AnimatorStateInfo nextState = _animator.GetNextAnimatorStateInfo(_layer);
						AnimatorTransitionInfo transitionInfo = _animator.GetAnimatorTransitionInfo(_layer);

						//Check current player is playing the next state
						if (_clipPlayerStates[_currentPlayerIndex] != nextState.shortNameHash)
						{
							//If not switch current player index and start animation on player
							_currentPlayerIndex = 1 - _currentPlayerIndex;
							_clipPlayerStates[_currentPlayerIndex] = nextState.shortNameHash;

							AnimatorClipInfo[] nextClips = _animator.GetNextAnimatorClipInfo(_layer);
							PlayAnimation(nextState, nextClips, _currentPlayerIndex);
						}

						//Then check previous player is playing the previous state
						if (_clipPlayerStates[1 - _currentPlayerIndex] != previousState.shortNameHash)
						{
							_clipPlayerStates[1 - _currentPlayerIndex] = previousState.shortNameHash;

							AnimatorClipInfo[] previousClips = _animator.GetCurrentAnimatorClipInfo(_layer);
							PlayAnimation(previousState, previousClips, 1 - _currentPlayerIndex);
						}

						//Update times
						_clipPlayers[_currentPlayerIndex].SetNormalizedTime(nextState.normalizedTime, true);
						_clipPlayers[1 - _currentPlayerIndex].SetNormalizedTime(previousState.normalizedTime, true);
						_currentAnimationWeight = transitionInfo.normalizedTime;
					}
					//Otherwise just update current animation
					else
					{
						AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(_layer);

						if (_clipPlayerStates[_currentPlayerIndex] != currentState.shortNameHash)
						{
							_clipPlayerStates[_currentPlayerIndex] = currentState.shortNameHash;
							AnimatorClipInfo[] currentClips = _animator.GetCurrentAnimatorClipInfo(_layer);
							PlayAnimation(currentState, currentClips, _currentPlayerIndex);

							//Stop other player
							_clipPlayers[1 - _currentPlayerIndex] = new GPUAnimationPlayer();
						}

						//Update time
						_clipPlayers[_currentPlayerIndex].SetNormalizedTime(currentState.normalizedTime, true);
						_currentAnimationWeight = 1.0f;
					}
				}
				
				public void GetRootMotionVelocities(out Vector3 velocity, out Vector3 angularVelocity)
				{
					_clipPlayers[_currentPlayerIndex].GetRootMotionVelocities(out velocity, out angularVelocity);

					if (_currentAnimationWeight < 1.0f)
					{
						_clipPlayers[1 - _currentPlayerIndex].GetRootMotionVelocities(out Vector3 previousPlayerVelocity, out Vector3 previousPlayerAngularVelocity);

						velocity = Vector3.Lerp(previousPlayerVelocity, velocity, _currentAnimationWeight);
						angularVelocity = Vector3.Lerp(previousPlayerAngularVelocity, angularVelocity, _currentAnimationWeight);
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