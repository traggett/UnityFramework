using System;
using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			[Serializable]
			public struct GPUAnimationsRef
			{
				#region Serialized Data
				[SerializeField]
				private TextAsset _asset;
				#endregion

				#region Private Data
				private GPUAnimations _animationTexture;
				#endregion

				#region Public Interface
				public GPUAnimations GetAnimations()
				{
					LoadIfNeeded();
					return _animationTexture;
				}

				public void SetMaterialProperties(Material material)
				{
					LoadIfNeeded();

					//Set material constants
					if (_animationTexture != null)
					{
						material.SetFloat("_boneCount", _animationTexture._bones.Length);
						material.SetTexture("_animationTexture", _animationTexture._texture);
						material.SetFloat("_animationTextureWidth", _animationTexture._texture.width);
						material.SetFloat("_animationTextureHeight", _animationTexture._texture.height);
					}
				}

				public bool IsValid()
				{
					return _asset != null;
				}

#if UNITY_EDITOR
				public void UnloadTexture()
				{
					_animationTexture = null;
				}
#endif
				#endregion

				#region Private Functions
				private void LoadIfNeeded()
				{
					if (_animationTexture == null && _asset != null)
					{
						_animationTexture = GPUAnimations.LoadFromFile(_asset);
					}
				}
				#endregion
			}
		}
	}
}
