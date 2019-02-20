using Framework.Maths;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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

			[Serializable]
			public struct ParticleAnimation
			{
				public int _animationIndex; //Index of animation in binary file
				[Range(0,1)]
				public float _probability; //change of particles using this animation
				public FloatRange _speedRange;	//What random speed range can be used
				public bool _loop; //should particles play this anim indefinitely.
			}

			//Info on what animations the particles can use
			[HideInInspector]
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
						_particleCustomData[i] = StartNewAnimation(customData, true);
					}
					//Progress current animation
					else
					{
						AnimationTexture.Animation animation = GetAnimation(Mathf.RoundToInt(customData.x));

						//Update current frame
						customData.y += Time.deltaTime * animation._fps * customData.z;

						//Check animation is finished, To do, check loops
						if (Mathf.FloorToInt(customData.y) < animation._totalFrames - 2)
						{
							_particleCustomData[i] = customData;
						}
						else
						{
							_particleCustomData[i] = StartNewAnimation(customData, false);
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

			private ParticleAnimation PickRandomAnimation()
			{
				float totalWeights = 0.0f;

				for (int i=0; i<_animations.Length; i++)
				{
					totalWeights += _animations[i]._probability;
				}

				float chosen = Random.Range(0.0f, totalWeights);
				totalWeights = 0.0f;

				for (int i = 0; i < _animations.Length; i++)
				{
					totalWeights += _animations[i]._probability;

					if (chosen <= totalWeights)
						return _animations[i];
				}

				return _animations[0];
			}

			private Vector4 StartNewAnimation(Vector4 oldData, bool randomOffset)
			{
				Vector4 data;

				ParticleAnimation anim = PickRandomAnimation();

				AnimationTexture.Animation animation = GetAnimation(anim._animationIndex);

				data.x = anim._animationIndex;
				data.y = randomOffset ? Random.Range(0, animation._totalFrames) : 0;
				data.z = anim._speedRange.GetRandomValue();
				data.w = 1.0f;

				return data;
			}

			private AnimationTexture.Animation GetAnimation(int index)
			{
				AnimationTexture.Animation[] animations =_animationTexture.GetAnimations();
				AnimationTexture.Animation animation = animations[Mathf.Clamp(index, 0, animations.Length)];
				return animation;
			}			
			#endregion
		}
	}
}
