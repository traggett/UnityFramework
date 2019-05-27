using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			[RequireComponent(typeof(Animator))]
			public class GPUAnimator : MonoBehaviour, IGPUAnimatorInstance
			{
				#region Public Data
				public GPUAnimationsRef _animations;
				public float _sphericalBoundsRadius;
				#endregion

				#region Private Data
				private SkinnedMeshRenderer _skinnedMeshRenderer;
				private Animator _animator;
				private GPUAnimationPlayer _currentAnimation;
				private int _currentAnimationState;
				private GPUAnimationPlayer _blendedAnimation;
				private int _blendedAnimationState;
				private float _blend;
				private Matrix4x4 _worldMatrix;
				private Vector3 _worldPos;
				private Vector3 _worldScale;
				private float _worldBoundsRadius;
				private Dictionary<AnimationClip, int> _animationLookUp;
				#endregion

				#region MonoBehaviour
				private void Awake()
				{
					Initialise();
				}

				private void Start()
				{
					UpdateCachedTransform();
				}

				private void Update()
				{
					if (this.transform.hasChanged)
						UpdateCachedTransform();

					UpdateAnimator();
					_currentAnimation.Update(Time.deltaTime);
					_blendedAnimation.Update(Time.deltaTime);
				}
				#endregion
				
				#region IGPUAnimator
				public float GetCurrentAnimationFrame()
				{
					return _currentAnimation.GetCurrentFrame();
				}

				public float GetBlendedAnimationFrame()
				{
					return _blendedAnimation.GetCurrentFrame();
				}

				public float GetAnimationBlend()
				{
					return _blend;
				}

				public float GetSphericalBoundsRadius()
				{
					return _worldBoundsRadius;
				}

				public Matrix4x4 GetWorldMatrix()
				{
					return _worldMatrix;
				}

				public Vector3 GetWorldPos()
				{
					return _worldPos;
				}

				public Vector3 GetWorldScale()
				{
					return _worldScale;
				}

				public SkinnedMeshRenderer GetSkinnedMeshRenderer()
				{
					return _skinnedMeshRenderer;
				}
				#endregion

				#region Private Functions
				private void Initialise()
				{
					_animator = GetComponent<Animator>();
					_skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

					AnimatorOverrideController overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);

					_animationLookUp = new Dictionary<AnimationClip, int>();

					List<KeyValuePair<AnimationClip, AnimationClip>> anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();

					foreach (AnimationClip origClip in overrideController.animationClips)
					{
						AnimationClip overrideClip = new AnimationClip();
						overrideClip.name = origClip.name;
						//Override original animation
						anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(origClip, overrideClip));
						//Cache what gpu animation it corresponds too
						_animationLookUp[overrideClip] = GetAnimationIndex(origClip);
					}

					overrideController.ApplyOverrides(anims);
					_animator.runtimeAnimatorController = overrideController;
				}

				private void UpdateCachedTransform()
				{
					_worldMatrix = this.transform.localToWorldMatrix;
					_worldPos = this.transform.position;
					_worldScale = this.transform.lossyScale;
					_worldBoundsRadius = Mathf.Max(_worldScale.x, _worldScale.y, _worldScale.x) * _sphericalBoundsRadius;
				}

				private void UpdateAnimation(AnimatorStateInfo state, AnimatorClipInfo[] clips, ref GPUAnimationPlayer animationPlayer, ref int stateHash)
				{
					if (stateHash != state.shortNameHash)
					{
						stateHash = state.shortNameHash;

						if (clips.Length > 0)
							animationPlayer.Play(this.gameObject, _animations.GetAnimations()[_animationLookUp[clips[0].clip]]);
						else
							animationPlayer.Stop();
					}

					animationPlayer._speed = state.speed * state.speedMultiplier;
				}

				private void UpdateAnimator()
				{
					AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
					AnimatorClipInfo[] currentClip = _animator.GetCurrentAnimatorClipInfo(0);

					//Check if we're transitioning
					if (_animator.IsInTransition(0))
					{
						AnimatorTransitionInfo transitionInfo = _animator.GetAnimatorTransitionInfo(0);
						AnimatorStateInfo nextState = _animator.GetNextAnimatorStateInfo(0);
						AnimatorClipInfo[] nextClip = _animator.GetCurrentAnimatorClipInfo(0);

						UpdateAnimation(nextState, nextClip, ref _currentAnimation, ref _currentAnimationState);
						UpdateAnimation(currentState, currentClip, ref _blendedAnimation, ref _blendedAnimationState);

						_blend = transitionInfo.normalizedTime;
					}
					//Otherwise just update current animation
					else
					{
						UpdateAnimation(currentState, currentClip, ref _currentAnimation, ref _currentAnimationState);
						_blendedAnimation.Stop();
						_blend = 1.0f;
					}
				}

				private int GetAnimationIndex(AnimationClip clip)
				{
					GPUAnimations.Animation[] animations = _animations.GetAnimations();

					for (int i=0; i< animations.Length; i++)
					{
						if (animations[i]._name == clip.name)
						{
							return i;
						}
					}

					return -1;
				}
				#endregion
			}
		}
    }
}