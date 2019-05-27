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
				private float[] _currentFrames;
				private float[] _blendedFrames;
				private float[] _blends;
				#endregion

				#region MonoBehaviour
				private void Awake()
				{
					_propertyBlock = new MaterialPropertyBlock();

					for (int i = 0; i < _materials.Length; i++)
					{
						_animationTexture.SetMaterialProperties(_materials[i]);
					}
				}

				private void Update()
				{
#if UNITY_EDITOR
					for (int i = 0; i < _materials.Length; i++)
					{
						_animationTexture.SetMaterialProperties(_materials[i]);
					}
#endif

					UpdateProperties();
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
					_currentFrames = new float[kMaxMeshes];
					_blendedFrames = new float[kMaxMeshes];
					_blends = new float[kMaxMeshes];
				}

				protected override void UpdateProperties()
				{
					//Update transforms and animation data
					int index = 0;
					foreach (RenderData renderData in _renderedObjects)
					{
						int instanceIndex = renderData._index;
						_currentFrames[index] = _instanceData[index]._animator.GetCurrentAnimationFrame();
						_blendedFrames[index] = _instanceData[index]._animator.GetBlendedAnimationFrame();
						_blends[index] = _instanceData[index]._animator.GetAnimationBlend();
						index++;
					}

					_propertyBlock.SetFloatArray("_animationFrame", _currentFrames);
					_propertyBlock.SetFloatArray("_blendedAnimationFrame", _blendedFrames);
					_propertyBlock.SetFloatArray("_animationBlend", _blends);
				}
				#endregion
			}
		}
    }
}