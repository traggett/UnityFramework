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
				private float[] _mainAnimationFrames;
				private float[] _mainAnimationWeights;
				private float[] _backgroundAnimationFrames;
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
						ActivateInstance(instance);
						instance._animator.Initialise(this);
						return true;
					}

					return false;
				}
				#endregion

				#region MeshInstanceRenderer
				protected override bool Initialise()
				{
					if (base.Initialise())
					{
						_mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000000f);

						_mainAnimationFrames = new float[_maxMeshes];
						_backgroundAnimationFrames = new float[_maxMeshes];
						_mainAnimationWeights = new float[_maxMeshes];

						for (int i = 0; i < _materials.Length; i++)
						{
							_animationTexture.SetMaterialProperties(_materials[i]);
						}

						return true;
					}

					return false;
				}

				protected override void UpdateProperties()
				{
					int numRenderedInstances = GetNumRenderedInstances();

					int index = 0;
					for (int i =0; i< numRenderedInstances; i++)
					{
						int instanceIndex = GetRenderedInstanceIndex(i);
						GPUAnimatorBase animator = _instanceData[instanceIndex]._animator;
						_mainAnimationFrames[index] = animator.GetMainAnimationFrame();
						_mainAnimationWeights[index] = animator.GetMainAnimationWeight();
						_backgroundAnimationFrames[index] = animator.GetBackgroundAnimationFrame();
						
						index++;
					}

					_propertyBlock.SetFloatArray("_mainAnimationFrame", _mainAnimationFrames);
					_propertyBlock.SetFloatArray("_mainAnimationWeight", _mainAnimationWeights);
					_propertyBlock.SetFloatArray("_backgroundAnimationFrame", _backgroundAnimationFrames);
				}
				#endregion
			}
		}
    }
}