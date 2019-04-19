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
			public AnimationTextureRef _animationTexture;

			[Serializable]
			public struct ParticleAnimation
			{
				public int _animationIndex; //Index of animation in binary file
				[Range(0,1)]
				public float _probability; //change of particles using this animation
				public FloatRange _speedRange;	//What random speed range can be used
			}

			//Info on what animations the particles can use
			[HideInInspector]
			public ParticleAnimation[] _animations;

			public ParticleSystemCustomData _customDataChannel;
			#endregion

			#region Private Data
			private List<Vector4> _particleCustomData;
			private float[] _particleCurrentFrame;
			
			//Particle custom data
			//X - animation id
			//y - frame
			//z - speed
			private static readonly Vector4 kDefaultData = new Vector4(-1.0f, 0.0f, 1.0f, 0.0f);
			#endregion

			#region Monobehaviour
			private void Update()
			{
				InitialiseIfNeeded();
				UpdateAnimations();
				Render(Camera.main);
			}
			#endregion

			#region MeshInstanceParticleSystem
			protected override void InitialiseIfNeeded()
			{
				base.InitialiseIfNeeded();
				
				if (_particleCustomData == null)
				{
					for (int i = 0; i < _materials.Length; i++)
					{
						_animationTexture.SetMaterialProperties(_materials[i]);
					}
					
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

			protected override void UpdateProperties()
			{
#if UNITY_EDITOR
				for (int i = 0; i < _materials.Length; i++)
				{
					_animationTexture.SetMaterialProperties(_materials[i]);
				}
#endif

				//Update property block
				for (int i = 0; i < GetNumRenderedParticles(); i++)
				{
					int index = GetRenderedParticlesIndex(i);
					AnimationTexture.Animation animation = GetAnimation(Mathf.RoundToInt(_particleCustomData[index].x));
					_particleCurrentFrame[i] = animation._startFrameOffset + _particleCustomData[index].y;
				}				

				//Update property block
				_propertyBlock.SetFloatArray("frameIndex", _particleCurrentFrame);
			}
			#endregion

			#region Private Functions
			private void UpdateAnimations()
			{
				int numParticles = _particleSystem.GetCustomParticleData(_particleCustomData, _customDataChannel);

				//Update particle frame progress
				for (int i = 0; i < numParticles; i++)
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

						//Is animation finished?
						if (Mathf.FloorToInt(customData.y) < animation._totalFrames - 2)
						{
							_particleCustomData[i] = customData;
						}
						else
						{
							_particleCustomData[i] = StartNewAnimation(customData, false);
						}
					}
				}

				//Update custom data
				_particleSystem.SetCustomParticleData(_particleCustomData, _customDataChannel);
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
				data.y = randomOffset ? Random.Range(0, animation._totalFrames - 2) : 0;
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
