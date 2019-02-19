using Framework.Utils;
using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		public class GPUSkinnedMesh : MonoBehaviour
		{
			public TextAsset _animationTextureAsset;
			public MaterialRefProperty _material;
			public float _currentFrame;
			public Mesh _mesh;
			private AnimationTexture _animationTexture;

			private void Start()
			{
				_animationTexture = AnimationTexture.ReadAnimationTexture(_animationTextureAsset);

				_material.GetMaterial().SetInt("_numBones", _animationTexture._numBones);
				_material.GetMaterial().SetTexture("_animationTexture", _animationTexture._texture);
				_material.GetMaterial().SetInt("_animationTextureWidth", _animationTexture._texture.width);
				_material.GetMaterial().SetInt("_animationTextureHeight", _animationTexture._texture.height);
			}

			private void Update()
			{
				AnimationTexture.Animation animation = _animationTexture._animations[0];
				
				_currentFrame += Time.deltaTime * animation._fps;

				if (_currentFrame + 1 > animation._totalFrames)
				{
					_currentFrame = 0.0f;
				}
				
				//_material.GetMaterial().SetFloatArray("_frameIndex", new System.Collections.Generic.List<float>() { _currentFrame });
				//_material.GetMaterial().SetFloat("_transitionProgress", 0.0f);
				//_material.GetMaterial().SetFloat("_preFrameIndex", -1.0f);

				//MaterialPropertyBlock prop = new MaterialPropertyBlock();
				_material.GetMaterial().SetFloat("_frameIndex", _currentFrame);
				//Graphics.DrawMeshInstanced(_mesh, 0, _material, new Matrix4x4[] { this.transform.localToWorldMatrix }, 1, prop);
			}
		}
	}
}