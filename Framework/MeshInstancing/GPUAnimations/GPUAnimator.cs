using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			[RequireComponent(typeof(Animator))]
			public class GPUAnimator : MonoBehaviour
			{
				#region Public Data
				public GPUAnimationsRef _animations;
				#endregion

				#region Private Data
				private Animator _animator;
				private GPUAnimationPlayer _currentAnimation;
				private GPUAnimationPlayer _blendedAnimation;
				private float _blend;
				private Matrix4x4 _renderMatrix;
				private Dictionary<AnimationClip, int> _animationLookUp;
				#endregion

				#region MonoBehaviour
				private void Awake()
				{
					_animator = GetComponent<Animator>();
					Prepare();
				}

				private void Start()
				{
					//For now start playing first animation
					_currentAnimation.Play(this.gameObject, _animations.GetAnimations()[0]);
					_blend = 0.0f;

					_renderMatrix = this.transform.localToWorldMatrix;
				}

				private void Update()
				{
					if (this.transform.hasChanged)
					{
						_renderMatrix = this.transform.localToWorldMatrix;
					}

					UpdateAnimator();
					_currentAnimation.Update(Time.deltaTime);
					_blendedAnimation.Update(Time.deltaTime);
				}
				#endregion

				#region Public Functions
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

				public Matrix4x4 GetRenderMatrix()
				{
					return _renderMatrix;
				}
				#endregion

				#region Private Functions
				private void Prepare()
				{
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

				private void UpdateAnimator()
				{
					//Check what state the animator is in, whats the corresponding clip and what speed its playing
					AnimatorClipInfo[] clipInfo = _animator.GetCurrentAnimatorClipInfo(0);

					//Find animation with highest blend
					//Find animation with second highest blend

					//if (clipInfo.Length > 0)
					//	_currentAnimation.Play(_animationLookUp[clipInfo[0].clip]);
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