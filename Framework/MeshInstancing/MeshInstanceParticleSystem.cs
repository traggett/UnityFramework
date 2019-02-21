using System;
using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		[RequireComponent(typeof(ParticleSystem))]
		public class MeshInstanceParticleSystem : MonoBehaviour
		{
			#region Public Data
			public Mesh _mesh;
			public Material _material;
			public bool _alignWithVelocity;
			public bool _sortByDepth;
			public bool _frustrumCull;
			public float _boundRadius;
			public float _frustrumPadding;
			#endregion

			#region Private Data
			protected struct ParticleData : IComparable
			{
				public int _index;
				public float _distToCamera;

				public int CompareTo(object obj)
				{
					ParticleData other = (ParticleData)obj;

					return other._distToCamera.CompareTo(_distToCamera);
				}
			}
			protected ParticleSystem _particleSystem;
			protected ParticleSystem.Particle[] _particles;
			protected int _numRenderedParticles;
			protected ParticleData[] _renderedParticles;
			protected Matrix4x4[] _particleTransforms;
			protected MaterialPropertyBlock _propertyBlock;
			#endregion

			#region Monobehaviour
			void Update()
			{
				Render(Camera.main);
			}
			#endregion

			#region Public Interface

			#endregion

			#region Private Functions
			protected virtual void InitialiseIfNeeded()
			{
				if (_propertyBlock == null)
				{
					_propertyBlock = new MaterialPropertyBlock();
				}

				if (_particleSystem == null || _particles == null)
				{
					_particleSystem = GetComponent<ParticleSystem>();
					_particles = new ParticleSystem.Particle[_particleSystem.main.maxParticles];
					_particleTransforms = new Matrix4x4[Math.Min(_particleSystem.main.maxParticles, 1023)];
					_renderedParticles = new ParticleData[_particleTransforms.Length];
				}
			}

			protected void Render(Camera camera)
			{
				InitialiseIfNeeded();

				if (_mesh == null || _material == null)
					return;

				int numAlive = _particleSystem.GetParticles(_particles);

				if (numAlive > 0)
				{
					Plane[] planes = null;

					if (_frustrumCull)
						planes = GeometryUtility.CalculateFrustumPlanes(camera);

					_numRenderedParticles = 0;

					int numParticles = Math.Min(numAlive, _particleTransforms.Length);
					
					for (int i = 0; i < numParticles; i++)
					{
						Vector3 pos = _particles[i].position;

						bool rendered = true;

						//If frustum culling is enabled, check should draw this particle
						if (_frustrumCull)
						{
							if (!IsSphereInFrustrum(ref planes, ref pos, _boundRadius, _frustrumPadding))
							{
								rendered = false;
							}
						}

						if (rendered)
						{
							Quaternion rot;

							if (_alignWithVelocity)
							{
								Vector3 foward = _particles[i].velocity;
								rot = Quaternion.LookRotation(foward);
							}
							else
							{
								rot = Quaternion.AngleAxis(_particles[i].rotation, _particles[i].axisOfRotation);
							}

							Vector3 scale = _particles[i].GetCurrentSize3D(_particleSystem);

							_particleTransforms[_numRenderedParticles].SetTRS(pos, rot, scale);
							_renderedParticles[_numRenderedParticles]._index = i;

							if (_sortByDepth)
								_renderedParticles[_numRenderedParticles]._distToCamera = (camera.transform.position - pos).sqrMagnitude;

							_numRenderedParticles++;
						}
					}

					if (_numRenderedParticles > 0)
					{
						if (_sortByDepth)
							Array.Sort(_renderedParticles, _particleTransforms);

						UpdateProperties();

						Graphics.DrawMeshInstanced(_mesh, 0, _material, _particleTransforms, _numRenderedParticles, _propertyBlock);
					}
				}
			}

			protected virtual void UpdateProperties()
			{

			}
			
			private bool IsSphereInFrustrum(ref Plane[] frustrumPlanes, ref Vector3 center, float radius, float frustumPadding = 0)
			{
				for (int i = 0; i < frustrumPlanes.Length; i++)
				{
					var normal = frustrumPlanes[i].normal;
					var distance = frustrumPlanes[i].distance;

					float dist = normal.x * center.x + normal.y * center.y + normal.z * center.z + distance;

					if (dist < -radius - frustumPadding)
					{
						return false;
					}
				}

				return true;
			}

			#endregion
		}
	}
}
