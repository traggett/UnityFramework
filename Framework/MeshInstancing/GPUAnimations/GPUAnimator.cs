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
			public class GPUAnimator : MonoBehaviour, IGPUAnimatorInstance
			{
				#region Public Data
				public GPUAnimationsRef _animations;
				public float _sphericalBoundsRadius;
				[HideInInspector]
				public float _animatedValue;
				#endregion

				#region Private Data
				private SkinnedMeshRenderer _skinnedMeshRenderer;
				private Animator _animator;
				private GPUAnimationPlayer _currentAnimation;
				private int _currentAnimationState;
				private GPUAnimationPlayer _previousAnimation;
				private int _blendedAnimationState;
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
				}
				#endregion
				
				#region IGPUAnimator
				public float GetCurrentAnimationFrame()
				{
					return _currentAnimation.GetCurrentTexureFrame();
				}

				public float GetCurrentAnimationWeight()
				{
					return _currentAnimationWeight;
				}
				public float GetPreviousAnimationFrame()
				{
					return _previousAnimation.GetCurrentTexureFrame();
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
					_skinnedMeshRenderer = GameObjectUtils.GetComponent<SkinnedMeshRenderer>(this.gameObject, true);

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

				private static AnimationClip CreateOverrideClip(AnimationClip origClip)
				{
					AnimationClip overrideClip = new AnimationClip
					{
						name = origClip.name,
						wrapMode = origClip.wrapMode
					};
					overrideClip.SetCurve("", typeof(GPUAnimator), "_animatedValue", new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(origClip.length, 1f)));
					return overrideClip;
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
						{
							AnimationClip clip = clips[0].clip;
							int animationIndex = _animationLookUp[clip];
							GPUAnimations.Animation animation = _animations.GetAnimations()[animationIndex];
							animationPlayer.Play(this.gameObject, animation, clip.wrapMode, 0f);
						}
						else
						{
							animationPlayer.Stop();
						}
							
					}

					animationPlayer.SetNormalizedTime(state.normalizedTime);
				}

				private void UpdateAnimator()
				{
					AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
					AnimatorClipInfo[] currentClips = _animator.GetCurrentAnimatorClipInfo(0);

					//Check if we're transitioning - TO DO! transition from state to same state seem broken / cause pops
					if (_animator.IsInTransition(0))
					{
						AnimatorTransitionInfo transitionInfo = _animator.GetAnimatorTransitionInfo(0);
						AnimatorStateInfo nextState = _animator.GetNextAnimatorStateInfo(0);
						AnimatorClipInfo[] nextClips = _animator.GetNextAnimatorClipInfo(0);

						UpdateAnimation(nextState, nextClips, ref _currentAnimation, ref _currentAnimationState);
						UpdateAnimation(currentState, currentClips, ref _previousAnimation, ref _blendedAnimationState);

						_currentAnimationWeight = nextClips[0].weight;
					}
					//Otherwise just update current animation
					else
					{
						UpdateAnimation(currentState, currentClips, ref _currentAnimation, ref _currentAnimationState);
						_previousAnimation.Stop();
						_currentAnimationWeight = 1.0f;
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