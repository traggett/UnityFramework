using Framework.Maths;
using System.Collections.Generic;
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
			public AnimationTextureRef _animationTexture;

			public struct ParticleAnimation
			{
				public int _animationIndex; //Index of animation in binary file
				public float _probabilty; //change of particles using this animation
				public FloatRange _speedRange;	//What random speed range can be used
				public bool _loop; //should particles play this anim indefinitely.
			}

			//Info on what animations the particles can use
			public ParticleAnimation[] _animations;

			public ParticleSystemCustomData _customDataChannel;
			#endregion

			#region Private Data
			private List<Vector4> _particleCustomData;
			private float[] _particleCurrentFrame;

			private static readonly Vector4 kDefaultData = new Vector4(-1.0f, 0.0f, 1.0f, 0.0f);
			#endregion

			#region Monobehaviour

			#endregion

			#region Public Interface

			#endregion

			//Particle custom data
			//X - animation id
			//y - frame
			//z - speed

			#region MeshInstanceParticleSystem
			protected override void InitialiseIfNeeded()
			{
				base.InitialiseIfNeeded();
				
				if (_particleCustomData == null)
				{
					_animationTexture.SetMaterialProperties(_material);

					_mesh = AnimationTexture.AddExtraMeshData(_mesh, 4);

					_particleCurrentFrame = new float[_particles.Length];

					_particleCustomData = new List<Vector4>(_particles.Length);

					ParticleSystem.CustomDataModule customData = _particleSystem.customData;
					customData.SetMode(_customDataChannel, ParticleSystemCustomDataMode.Vector);
					customData.SetVector(_customDataChannel, 0, new ParticleSystem.MinMaxCurve(kDefaultData.x));
					customData.SetVector(_customDataChannel, 1, new ParticleSystem.MinMaxCurve(kDefaultData.y));
					customData.SetVector(_customDataChannel, 2, new ParticleSystem.MinMaxCurve(kDefaultData.z));
					customData.SetVector(_customDataChannel, 3, new ParticleSystem.MinMaxCurve(kDefaultData.w));
				}
			}

			protected override void UpdateProperties(int numMeshes)
			{
				_particleSystem.GetCustomParticleData(_particleCustomData, _customDataChannel);
			
#if UNITY_EDITOR
				_animationTexture.SetMaterialProperties(_material);
#endif

				//Update particle frame progress
				for (int i = 0; i < numMeshes; i++)
				{
					Vector4 customData = _particleCustomData[i];

					float prevFrame = customData.y;

					//New particle
					if (customData.w == 0.0f)
					{
						_particleCustomData[i] = StartNewAnimation(customData);
					}
					//Progress current animation
					else
					{
						AnimationTexture.Animation animation = GetAnimation(customData);

						//Update current frame
						customData.y += Time.deltaTime * animation._fps * customData.z;

						//Check animation is finished, To do, check loops
						if (customData.y + 1 > animation._totalFrames)
						{
							_particleCustomData[i] = StartNewAnimation(customData);
						}
						else
						{
							_particleCustomData[i] = customData;
						}
					}

					//Update properties
					_particleCurrentFrame[i] = customData.y;
				}

				//Update custom data
				_particleSystem.SetCustomParticleData(_particleCustomData, _customDataChannel);

				//Update property block
				_propertyBlock.SetFloatArray("frameIndex", _particleCurrentFrame);
			}

			private Vector4 StartNewAnimation(Vector4 oldData)
			{
				Vector4 data;

				//TO DO!
				data.x = 0.0f;
				data.y = 0.0f;
				data.z = 1.0f;
				data.w = 1.0f;

				return data;
			}

			private AnimationTexture.Animation GetAnimation(Vector4 data)
			{
				int index = Mathf.RoundToInt(data.x);
				AnimationTexture.Animation animation = _animationTexture.GetAnimations()[index];
				return animation;
			}
			
			#endregion
		}
	}
}
