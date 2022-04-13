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
				[RequireComponent(typeof(GPUAnimatorRenderer))]
				public class GPUAnimatorRendererControllerOverrider : MonoBehaviour
				{
					#region Public Data
					public RuntimeAnimatorController[] _controllers;
					#endregion

					#region Private Data
					private GPUAnimatorRenderer _renderer;
					private Dictionary<RuntimeAnimatorController, GPUAnimatorOverrideController> _overrideControllers = new Dictionary<RuntimeAnimatorController, GPUAnimatorOverrideController>();

					[Serializable]
					private struct AnimationClipData
					{
						public AnimationClip _originalClip;
						public AnimationClip _overrideClip;
					}
					[SerializeField]
					private AnimationClipData[] _overrideClips;
					#endregion

					#region Public Functions
					public static GPUAnimatorOverrideController GetOverrideController(GPUAnimatorRenderer renderer, Animator animator)
					{
						GPUAnimatorRendererControllerOverrider animatorRendererOverrideController = renderer.GetComponent<GPUAnimatorRendererControllerOverrider>();

						if (animatorRendererOverrideController != null)
						{
							return animatorRendererOverrideController.GetOverrideController(animator);
						}
						else
						{
							RuntimeAnimatorController runtimeAnimatorController = GetRuntimeAnimatorController(animator);
							return new GPUAnimatorOverrideController(runtimeAnimatorController, renderer._animations.GetAnimations());
						}
					}

#if UNITY_EDITOR
				public void CacheOverrideClips()
				{
					List<AnimationClip> clips = new List<AnimationClip>();

					for (int i = 0; i < _controllers.Length; i++)
					{
						if (_controllers[i] != null)
						{
							foreach (AnimationClip clip in _controllers[i].animationClips)
							{
								if (clip != null && !clips.Contains(clip))
								{
									clips.Add(clip);
								}
							}
						}
					}

					_overrideClips = new AnimationClipData[clips.Count];

					for (int i = 0; i < _overrideClips.Length; i++)
					{
						_overrideClips[i] = new AnimationClipData()
						{
							_originalClip = clips[i],
							_overrideClip = GPUAnimatorOverrideController.CreateOverrideClip(clips[i])
						};
					}
				}
#endif
					#endregion

					#region Private Functions
					private GPUAnimatorOverrideController GetOverrideController(Animator animator)
					{
						RuntimeAnimatorController runtimeAnimatorController = GetRuntimeAnimatorController(animator);

						if (_renderer == null)
							_renderer = GetComponent<GPUAnimatorRenderer>();

						if (!_overrideControllers.TryGetValue(runtimeAnimatorController, out GPUAnimatorOverrideController overrideController))
						{
							List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();

							foreach (AnimationClip origClip in runtimeAnimatorController.animationClips)
							{
								for (int i = 0; i < _overrideClips.Length; i++)
								{
									if (_overrideClips[i]._originalClip == origClip)
									{
										overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(origClip, _overrideClips[i]._overrideClip));
									}
								}
							}
							overrideController = new GPUAnimatorOverrideController(runtimeAnimatorController, _renderer._animations.GetAnimations(), overrides);
							_overrideControllers[runtimeAnimatorController] = overrideController;
						}

						return overrideController;
					}

					private static RuntimeAnimatorController GetRuntimeAnimatorController(Animator animator)
					{
						RuntimeAnimatorController runtimeAnimatorController = animator.runtimeAnimatorController;

						if (runtimeAnimatorController is AnimatorOverrideController)
						{
							runtimeAnimatorController = ((AnimatorOverrideController)runtimeAnimatorController).runtimeAnimatorController;
						}

						return runtimeAnimatorController;
					}
					#endregion
				}
			}
		}
	}
}