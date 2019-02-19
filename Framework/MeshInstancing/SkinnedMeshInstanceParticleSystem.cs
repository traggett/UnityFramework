using Framework.Maths;
using System.IO;
using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		[RequireComponent(typeof(ParticleSystem))]
		public class SkinnedMeshInstanceParticleSystem : MeshInstanceParticleSystem
		{
			#region Public Data
			//Binary file containing all animation textures as header info
			public TextAsset _animationTextureAsset;

			public struct ParticleAnimation
			{
				public int _animationIndex; //Index of animation in binary file
				public float _probabilty; //change of particles using this animation
				public FloatRange _speedRange;	//What random speed range can be used
				public bool _loop; //should particles play this anim indefinitely.
			}

			//Info on what animations the particles can use
			public ParticleAnimation[] _animations;
			#endregion

			#region Private Data
			private AnimationTexture _animationTexture;
			#endregion

			#region Monobehaviour

			#endregion

			#region Public Interface

			#endregion

			#region MeshInstanceParticleSystem
			protected override void InitialiseIfNeeded()
			{
				base.InitialiseIfNeeded();

				//Load animation texture data.
				if (_animationTexture == null)
				{
					_animationTexture = AnimationTexture.ReadAnimationTexture(_animationTextureAsset);
				}
			}

			protected override void UpdateProperties()
			{
				//To do update particle properties based on animations / particle custom data
			}
			#endregion
		}
	}
}
