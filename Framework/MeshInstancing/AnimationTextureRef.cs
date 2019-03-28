using System;
using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		[Serializable]
		public struct AnimationTextureRef
		{
			#region Serialized Data
			[SerializeField]
			private TextAsset _asset;
			#endregion

			#region Private Data
			private AnimationTexture _animationTexture;
			#endregion

			public void SetMaterialProperties(Material material)
			{
				LoadIfNeeded();

				//Set material constants
				if (_animationTexture != null)
				{
					material.SetFloat("_boneCount", _animationTexture._numBones);
					material.SetTexture("_animationTexture", _animationTexture._texture);
					material.SetFloat("_animationTextureWidth", _animationTexture._texture.width);
					material.SetFloat("_animationTextureHeight", _animationTexture._texture.height);
				}
			}

			public bool IsValid()
			{
				return _asset != null;
			}

			public AnimationTexture.Animation[] GetAnimations()
			{
				LoadIfNeeded();

				if (_animationTexture != null)
					return _animationTexture._animations;

				return null;
			}

#if UNITY_EDITOR
			public void UnloadTexture()
			{
				_animationTexture = null;
			}
#endif

			private void LoadIfNeeded()
			{
				if (_animationTexture == null && _asset != null)
				{
					_animationTexture = AnimationTexture.LoadFromFile(_asset);
				}
			}
		}
	}
}
