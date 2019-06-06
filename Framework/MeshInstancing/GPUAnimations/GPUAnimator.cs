using System.Collections.Generic;
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
				private SkinnedMeshRenderer _skinnedMeshRenderer;
				private Animator _animator;

				private int _currentPlayerIndex;
				private GPUAnimationPlayer[] _clipPlayers;
				private int[] _clipPlayerStates;

				private float _currentAnimationWeight;
				private Matrix4x4 _worldMatrix;
				private Vector3 _worldPos;
				private Vector3 _worldScale;
				private float _worldBoundsRadius;
				private Dictionary<AnimationClip, int> _animationLookUp;
				#endregion

				#region MonoBehaviour
				private void Awake()
				{
					_animator = GetComponent<Animator>();
					_skinnedMeshRenderer = GameObjectUtils.GetComponent<SkinnedMeshRenderer>(this.gameObject, true);
					_clipPlayers = new GPUAnimationPlayer[2];
					_clipPlayerStates = new int[2];

					UpdateCachedTransform();
				}

				private void Update()
				{
					if (this.transform.hasChanged)
						UpdateCachedTransform();

					UpdateAnimator();
					UpdateRootMotion();
				}
				#endregion

				#region GPUAnimatorBase
				public override void Initialise(GPUAnimatorRenderer renderer)
				{
					_renderer = renderer;
					_clipPlayers[0].Stop();
					_clipPlayers[1].Stop();
					_currentPlayerIndex = 0;
					_currentAnimationWeight = 1.0f;

					AnimatorOverrideController overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);

					_animationLookUp = new Dictionary<AnimationClip, int>();

					List<KeyValuePair<AnimationClip, AnimationClip>> anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();

					foreach (AnimationClip origClip in overrideController.animationClips)
					{
						//Override original animation
						AnimationClip overrideClip = CreateOverrideClip(origClip);
						anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(origClip, overrideClip));
						//Cache what gpu animation it corresponds too
						_animationLookUp[overrideClip] = GetAnimationIndex(origClip);
					}

					overrideController.ApplyOverrides(anims);
					_animator.runtimeAnimatorController = overrideController;
				}

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

				public override float GetSphericalBoundsRadius()
				{
					return _worldBoundsRadius;
				}

				public override Matrix4x4 GetWorldMatrix()
				{
					return _worldMatrix;
				}

				public override Vector3 GetWorldPos()
				{
					return _worldPos;
				}

				public override Vector3 GetWorldScale()
				{
					return _worldScale;
				}

				public override SkinnedMeshRenderer GetSkinnedMeshRenderer()
				{
					return _skinnedMeshRenderer;
				}
				#endregion

				#region Private Functions
				private static AnimationClip CreateOverrideClip(AnimationClip origClip)
				{
					AnimationClip overrideClip = new AnimationClip
					{
						name = origClip.name,
						wrapMode = origClip.wrapMode,
						legacy = true
					};

					overrideClip.SetCurve("", typeof(GPUAnimator), "_animatedValue", new AnimationCurve(new Keyframe(origClip.length, 0f)));
					overrideClip.legacy = false;

					return overrideClip;
				}

				private int GetAnimationIndex(AnimationClip clip)
				{
					GPUAnimations.Animation[] animations = _renderer._animationTexture.GetAnimations();

					for (int i = 0; i < animations.Length; i++)
					{
						if (animations[i]._name == clip.name)
						{
							return i;
						}
					}

					return -1;
				}

				private void UpdateCachedTransform()
				{
					_worldMatrix = this.transform.localToWorldMatrix;
					_worldPos = this.transform.position;
					_worldScale = this.transform.lossyScale;
					_worldBoundsRadius = Mathf.Max(Mathf.Max(_worldScale.x, _worldScale.y), _worldScale.z) * _sphericalBoundsRadius;
				}
				
				private void PlayAnimation(AnimatorStateInfo state, AnimatorClipInfo[] clips, int playerIndex)
				{
					if (clips.Length > 0)
					{
						AnimationClip clip = clips[0].clip;
						int animationIndex = _animationLookUp[clip];
						GPUAnimations.Animation animation = _renderer._animationTexture.GetAnimations()[animationIndex];
						_clipPlayers[playerIndex].Play(this.gameObject, animation, clip.wrapMode, 0f);
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
				#endregion
			}
		}
    }
}