using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
	namespace Graphics
	{
		namespace MeshInstancing
		{
			namespace GPUAnimations
			{
				public sealed class GPUAnimatorOverrideController : AnimatorOverrideController
				{
					#region Private Data
					private readonly Dictionary<AnimationClip, int> _animationLookUp;
					private readonly GPUAnimations _animations;
					#endregion

					#region Public Interface
					public GPUAnimatorOverrideController(RuntimeAnimatorController controller, GPUAnimations animations) : base(controller)
					{
						this.name = controller.name + " (GPU Animated)";

						_animations = animations;
						_animationLookUp = new Dictionary<AnimationClip, int>();

						List<KeyValuePair<AnimationClip, AnimationClip>> overrideClips = new List<KeyValuePair<AnimationClip, AnimationClip>>();

						foreach (AnimationClip origClip in controller.animationClips)
						{
							//Override original animation
							AnimationClip overrideClip = CreateOverrideClip(origClip);
							overrideClips.Add(new KeyValuePair<AnimationClip, AnimationClip>(origClip, overrideClip));
							//Cache what gpu animation it corresponds too
							_animationLookUp[overrideClip] = GetAnimationIndex(origClip);
						}

						ApplyOverrides(overrideClips);
					}

					public GPUAnimatorOverrideController(RuntimeAnimatorController controller, GPUAnimations animations, List<KeyValuePair<AnimationClip, AnimationClip>> overrideClips) : base(controller)
					{
						this.name = controller.name + " (GPU Animated)";

						_animations = animations;
						_animationLookUp = new Dictionary<AnimationClip, int>();

						foreach (KeyValuePair<AnimationClip, AnimationClip> clipPair in overrideClips)
						{
							_animationLookUp[clipPair.Value] = GetAnimationIndex(clipPair.Key);
						}

						ApplyOverrides(overrideClips);
					}

					public GPUAnimations.Animation GetAnimation(AnimationClip clip)
					{
						if (_animationLookUp.TryGetValue(clip, out int animationIndex) && 0 <= animationIndex && animationIndex < _animations._animations.Length)
						{
							return _animations._animations[animationIndex];
						}

						return GPUAnimations.Animation.kInvalid;
					}

					public static AnimationClip CreateOverrideClip(AnimationClip origClip)
					{
						AnimationClip overrideClip = new AnimationClip
						{
							name = origClip.name,
							wrapMode = origClip.wrapMode,
							legacy = true,
							frameRate = origClip.frameRate,
							localBounds = origClip.localBounds,
						};

						overrideClip.SetCurve("", typeof(GPUAnimator), "_animatedValue", new AnimationCurve(new Keyframe(origClip.length, 0f)));
						overrideClip.legacy = false;

						return overrideClip;
					}
					#endregion

					#region Private Functions
					private int GetAnimationIndex(AnimationClip clip)
					{
						for (int i = 0; i < _animations._animations.Length; i++)
						{
							if (_animations._animations[i]._name == clip.name)
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
}