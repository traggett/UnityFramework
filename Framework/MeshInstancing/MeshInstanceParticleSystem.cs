using System;
using System.Collections.Generic;
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
			protected class ParticleData
			{
				public int _index;
				public Matrix4x4 _transform;
				public float _distToCamera;
			}
			protected ParticleSystem _particleSystem;
			protected ParticleSystem.Particle[] _particles;
			private ParticleData[] _particleData;
			private List<ParticleData> _renderedParticles;
			private Matrix4x4[] _particleTransforms;
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
					_particleData = new ParticleData[_particleTransforms.Length];
					for (int i = 0; i < _particleTransforms.Length; i++)
						_particleData[i] = new ParticleData();
					_renderedParticles = new List<ParticleData>(_particleTransforms.Length);
				}
			}

			protected void Render(Camera camera)
			{
				InitialiseIfNeeded();

				if (_mesh == null || _material == null)
					return;

				int numAlive = _particleSystem.GetParticles(_particles);
				_renderedParticles.Clear();

				if (numAlive > 0)
				{
					Plane[] planes = null;

					if (_frustrumCull)
						planes = GeometryUtility.CalculateFrustumPlanes(camera);

					int numParticles = Math.Min(numAlive, _particleTransforms.Length);
					
					for (int i = 0; i < numParticles; i++)
					{
						Vector3 pos = _particles[i].position;

						bool rendered = true;

						//If frustum culling is enabled, check should draw this particle
						if (_frustrumCull)
						{
							rendered = IsSphereInFrustrum(ref planes, ref pos, _boundRadius, _frustrumPadding);
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
							
							_particleData[i]._index = i;
							_particleData[i]._transform.SetTRS(pos, rot, scale);

							if (_sortByDepth)
								_particleData[i]._distToCamera = (camera.transform.position - pos).sqrMagnitude;

							AddToSortedList(ref _particleData[i]);
						}
					}

					if (_renderedParticles.Count > 0)
					{
						UpdateProperties();
						FillTransformMatricies();
						Graphics.DrawMeshInstanced(_mesh, 0, _material, _particleTransforms, _renderedParticles.Count, _propertyBlock);
					}
				}
			}

			protected virtual void UpdateProperties()
			{

			}

			protected int GetNumRenderedParticles()
			{
				return _renderedParticles.Count;
			}

			protected int GetRenderedParticlesIndex(int i)
			{
				return _renderedParticles[i]._index;
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

			private void FillTransformMatricies()
			{
				for (int i = 0; i < _renderedParticles.Count; i++)
				{
					_particleTransforms[i] = _renderedParticles[i]._transform;
				}
			}

			private void AddToSortedList(ref ParticleData particleData)
			{
				int index = 0;

				if (_sortByDepth)
				{
					index = FindInsertIndex(particleData._distToCamera, 0, _renderedParticles.Count);
				}	

				_renderedParticles.Insert(index, particleData);
			}

			private static readonly int kSearchNodes = 8;

			private int FindInsertIndex(float dist, int startIndex, int endIndex)
			{
				int searchWidth = endIndex - startIndex;
				int numSearches = Mathf.Min(kSearchNodes, searchWidth);
				int nodesPerSearch = Mathf.FloorToInt(searchWidth / (float)numSearches);

				int currIndex = startIndex;
				int prevIndex = currIndex;

				for (int i =0; i<numSearches; i++)
				{
					//If this distance is greater than current node its between this and prev node
					if (dist > _renderedParticles[currIndex]._distToCamera)
					{
						//If first node or search one node at a time then found our index
						if (i == 0  || nodesPerSearch == 1)
						{
							return currIndex;
						}
						//Otherwise its between this and the previous index
						else
						{
							return FindInsertIndex(dist, prevIndex, currIndex);
						}
					}

					prevIndex = currIndex;
					currIndex = (i == numSearches - 1) ? endIndex : startIndex + ((i + 1) * nodesPerSearch);
				}

				return endIndex;
			}
			#endregion
		}
	}
}
