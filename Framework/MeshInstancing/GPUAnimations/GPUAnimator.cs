using UnityEngine;

namespace Framework
{
	using Utils;

	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			[RequireComponent(typeof(Animator))]
			public class GPUAnimator : GPUAnimatorBase
			{
				#region Public Data
				[HideInInspector]
				public float _animatedValue;
				#endregion

				#region Private Data
				private static Avatar _dummyAvatar;
				private Animator _animator;
				private SkinnedMeshRenderer _skinnedMeshRenderer;			

				private int _currentPlayerIndex;
				private GPUAnimationPlayer[] _clipPlayers;
				private int[] _clipPlayerStates;
				private float _currentAnimationWeight;
				#endregion

				#region MonoBehaviour
				private void Awake()
				{
					_animator = GetComponent<Animator>();
					_skinnedMeshRenderer = GameObjectUtils.GetComponent<SkinnedMeshRenderer>(this.gameObject, true);
					_clipPlayers = new GPUAnimationPlayer[2];
					_clipPlayerStates = new int[2];
					_currentPlayerIndex = 0;
					_currentAnimationWeight = 1.0f;

					_onInitialise += Initialise;

					UpdateCachedTransform();
				}

				private void Update()
				{
					if (_renderer != null)
					{
						UpdateAnimator();
						UpdateRootMotion();
					}
				}
				#endregion

				#region GPUAnimatorBase
				public override float GetCurrentAnimationFrame()
				{
					return _clipPlayers[_currentPlayerIndex].GetCurrentTexureFrame();
				}

				public override float GetCurrentAnimationWeight()
				{
					return _currentAnimationWeight;
				}

				public override float GetPreviousAnimationFrame()
				{
					return _clipPlayers[1 - _currentPlayerIndex].GetCurrentTexureFrame();
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
					_currentPlayerIndex = 0;
					_currentAnimationWeight = 1.0f;

					_animator.runtimeAnimatorController = _renderer.GetOverrideControllerForAnimator(_animator);
					_animator.avatar = GetDummyAvatar();
				}
				
				private void PlayAnimation(AnimatorStateInfo state, AnimatorClipInfo[] clips, int playerIndex)
				{
					if (clips.Length > 0)
					{
						AnimationClip clip = clips[0].clip;
						GPUAnimations.Animation animation = ((GPUAnimatorOverrideController)_animator.runtimeAnimatorController).GetAnimation(clip);
						_clipPlayers[playerIndex].Play(this.gameObject, animation, 0f);
					}
					else
					{
						_clipPlayers[playerIndex].Stop();
					}
				}

				private void UpdateAnimator()
				{
					//Check if we're transitioning - TO DO! transition from state to same state seem broken / cause pops
					if (_animator.IsInTransition(0))
					{
						AnimatorStateInfo previousState = _animator.GetCurrentAnimatorStateInfo(0);
						AnimatorStateInfo nextState = _animator.GetNextAnimatorStateInfo(0);
						AnimatorTransitionInfo transitionInfo = _animator.GetAnimatorTransitionInfo(0);

						//Check current player is playing the next state
						if (_clipPlayerStates[_currentPlayerIndex] != nextState.fullPathHash)
						{
							//If not switch current player index and start animation on player
							_currentPlayerIndex = 1 - _currentPlayerIndex;
							_clipPlayerStates[_currentPlayerIndex] = nextState.fullPathHash;

							AnimatorClipInfo[] nextClips = _animator.GetNextAnimatorClipInfo(0);
							PlayAnimation(nextState, nextClips, _currentPlayerIndex);
						}

						//Then check previous player is playing the previous state
						if (_clipPlayerStates[1 - _currentPlayerIndex] != previousState.fullPathHash)
						{
							_clipPlayerStates[1 - _currentPlayerIndex] = previousState.fullPathHash;

							AnimatorClipInfo[] previousClips = _animator.GetCurrentAnimatorClipInfo(0);
							PlayAnimation(previousState, previousClips, 1 - _currentPlayerIndex);
						}

						//Update times
						_clipPlayers[_currentPlayerIndex].SetNormalizedTime(nextState.normalizedTime);
						_clipPlayers[1 - _currentPlayerIndex].SetNormalizedTime(previousState.normalizedTime);
						_currentAnimationWeight = transitionInfo.normalizedTime;
					}
					//Otherwise just update current animation
					else
					{
						AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);

						if (_clipPlayerStates[_currentPlayerIndex] != currentState.fullPathHash)
						{
							_clipPlayerStates[_currentPlayerIndex] = currentState.fullPathHash;
							AnimatorClipInfo[] currentClips = _animator.GetCurrentAnimatorClipInfo(0);
							PlayAnimation(currentState, currentClips, _currentPlayerIndex);

							//Stop other player
							_clipPlayers[1 - _currentPlayerIndex].Stop();
						}
						
						//Update time
						_clipPlayers[_currentPlayerIndex].SetNormalizedTime(currentState.normalizedTime);
						_currentAnimationWeight = 1.0f;
					}
				}

				private void UpdateRootMotion()
				{
					if (_animator.applyRootMotion)
					{
						_clipPlayers[_currentPlayerIndex].GetRootMotionVelocities(out Vector3 velocity, out Vector3 angularVelocity);

						if (_currentAnimationWeight < 1.0f)
						{
							_clipPlayers[1 - _currentPlayerIndex].GetRootMotionVelocities(out Vector3 previousPlayerVelocity, out Vector3 previousPlayerAngularVelocity);

							velocity = Vector3.Lerp(previousPlayerVelocity, velocity, _currentAnimationWeight);
							velocity = Vector3.Lerp(previousPlayerAngularVelocity, angularVelocity, _currentAnimationWeight);
						}

						Quaternion rotation = this.transform.localRotation;
						Quaternion delta = Quaternion.Euler(angularVelocity * Time.deltaTime);
						rotation *= delta;

						Vector3 offset = velocity * Time.deltaTime;
						offset = rotation * offset;

						Vector3 position = this.transform.localPosition;
						position += offset;

						this.transform.localPosition = position;
						this.transform.localRotation = rotation;
					}
				}

				private static Avatar GetDummyAvatar()
				{
					if (_dummyAvatar == null)
					{
						GameObject temp = new GameObject();
						_dummyAvatar = AvatarBuilder.BuildGenericAvatar(temp, "");
						_dummyAvatar.name = "GPU Animated Avatar";
						Destroy(temp);
					}
				
					return _dummyAvatar;
				}
				#endregion
			}
		}
    }
}