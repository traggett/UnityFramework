using Framework.Utils;
using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		public class GPUSkinnedMesh : MonoBehaviour
		{
			public AnimationTextureRef _animationTexture;
			public MaterialRefProperty _material;
			public float _currentFrame;
			public SkinnedMeshRenderer _skinnedMesh;

			private void Start()
			{
				_skinnedMesh.sharedMesh = AnimationTexture.AddExtraMeshData(_skinnedMesh.sharedMesh, 4);

				_animationTexture.SetMaterialProperties(_material);
			}

			private void Update()
			{
				AnimationTexture.Animation animation = _animationTexture.GetAnimations()[0];
				
				_currentFrame += Time.deltaTime * animation._fps;

				if (_currentFrame + 1 > animation._totalFrames)
				{
					_currentFrame = 0.0f;
				}
				
				_material.GetMaterial().SetFloat("frameIndex", _currentFrame);
				_material.GetMaterial().SetFloat("preFrameIndex", -1.0f);
				_material.GetMaterial().SetFloat("transitionProgress", -1.0f);

				Graphics.DrawMeshInstanced(_skinnedMesh.sharedMesh, 0, _material, new Matrix4x4[] { this.transform.localToWorldMatrix }, 1);
			}
		}
	}
}