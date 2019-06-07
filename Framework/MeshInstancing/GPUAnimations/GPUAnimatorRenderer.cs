using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public class GPUAnimatorRenderer : MeshInstanceRenderer<GPUAnimatedMeshInstance>
			{
				#region Public Data
				public GPUAnimationsRef _animationTexture;
				#endregion

				#region Private Data
				private float[] _currentAnimationFrames;
				private float[] _previousAnimationFrames;
				private float[] _currentAnimationWeights;

				private Dictionary<RuntimeAnimatorController, GPUAnimatorOverrideController> _animatorOverrideControllers = new Dictionary<RuntimeAnimatorController, GPUAnimatorOverrideController>();
				#endregion

				#region MonoBehaviour
				private void Update()
				{
#if UNITY_EDITOR
					for (int i = 0; i < _materials.Length; i++)
					{
						_animationTexture.SetMaterialProperties(_materials[i]);
					}
#endif
					
					Render(Camera.main);
				}
				#endregion

				#region Public Functions
				public bool ActivateInstance(GameObject instanceGameObject)
				{
					GPUAnimatedMeshInstance instance = new GPUAnimatedMeshInstance(instanceGameObject);

					if (instance._animator != null)
					{
						instance._animator.Initialise(this);
						ActivateInstance(instance);
						return true;
					}

					return false;
				}

				public GPUAnimatorOverrideController GetOverrideControllerForAnimator(Animator animator)
				{
					GPUAnimatorOverrideController overrideController;

					if (!_animatorOverrideControllers.TryGetValue(animator.runtimeAnimatorController, out overrideController))
					{
						overrideController = new GPUAnimatorOverrideController(animator.runtimeAnimatorController, _animationTexture.GetAnimations());
						_animatorOverrideControllers[animator.runtimeAnimatorController] = overrideController;
					}

					return overrideController;
				}
				#endregion

				#region MeshInstanceRenderer
				protected override void Initialise()
				{
					base.Initialise();

					_currentAnimationFrames = new float[kMaxMeshes];
					_previousAnimationFrames = new float[kMaxMeshes];
					_currentAnimationWeights = new float[kMaxMeshes];

					for (int i = 0; i < _materials.Length; i++)
					{
						_animationTexture.SetMaterialProperties(_materials[i]);
					}
				}

				protected override void UpdateProperties()
				{
					//Update transforms and animation data
					int index = 0;
					foreach (RenderData renderData in _renderedObjects)
					{
						GPUAnimatorBase animator = _instanceData[renderData._index]._animator;
						_currentAnimationFrames[index] = animator.GetCurrentAnimationFrame();
						_currentAnimationWeights[index] = animator.GetCurrentAnimationWeight();
						_previousAnimationFrames[index] = animator.GetPreviousAnimationFrame();
						
						index++;
					}

					_propertyBlock.SetFloatArray("_currentAnimationFrame", _currentAnimationFrames);
					_propertyBlock.SetFloatArray("_currentAnimationWeight", _currentAnimationWeights);
					_propertyBlock.SetFloatArray("_previousAnimationFrame", _previousAnimationFrames);
				}
				#endregion
			}
		}
    }
}