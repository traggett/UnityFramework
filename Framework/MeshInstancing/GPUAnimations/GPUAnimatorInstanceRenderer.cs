using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public class GPUAnimatorInstanceRenderer : MeshInstanceRenderer<GPUAnimatorInstance>
			{
				#region Public Data
				public GPUAnimationsRef _animationTexture;
				public GameObject _prefab;
				#endregion

				#region Private Data
				private float[] _currentAnimationFrames;
				private float[] _previousAnimationFrames;
				private float[] _currentAnimationWeights;
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
				public GameObject SpawnInstance()
				{
					GameObject instanceGameObject = Instantiate(_prefab);
					GPUAnimatorInstance instance = new GPUAnimatorInstance(instanceGameObject);
					if (instance._animator != null)
					{
						//Disable skinned mesh renderer
						instance._animator.GetSkinnedMeshRenderer().enabled = false;
						ActivateInstance(instance);
					}
					else
					{
						Destroy(instanceGameObject);
					}

					return instanceGameObject;
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
						IGPUAnimatorInstance animator = _instanceData[renderData._index]._animator;
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