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
				_skinnedMesh.sharedMesh = AnimationTexture.AddExtraMeshData(_skinnedMesh.sharedMesh);

				_animationTexture.SetMaterialProperties(_material);
			}

			private void Update()
			{
				AnimationTexture.Animation animation = _animationTexture.GetAnimations()[0];
				
				_currentFrame += Time.deltaTime * animation._fps;

				if (Mathf.FloorToInt(_currentFrame - animation._startFrameOffset) >= animation._totalFrames - 1)
				{
					_currentFrame = animation._startFrameOffset;
				}
				
				_material.GetMaterial().SetFloat("frameIndex", _currentFrame);
				_material.GetMaterial().SetFloat("preFrameIndex", -1.0f);
				_material.GetMaterial().SetFloat("transitionProgress", -1.0f);

				Graphics.DrawMeshInstanced(_skinnedMesh.sharedMesh, 0, _material, new Matrix4x4[] { this.transform.localToWorldMatrix }, 1);
			}
		}
	}
}