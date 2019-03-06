using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Framework
{
	namespace MeshInstancing
	{
		[RequireComponent(typeof(ParticleSystem))]
		public class MeshInstanceParticleSystem : MonoBehaviour
		{
			#region Public Data
			public Mesh _mesh;
			public Material[] _materials;
			public Vector3 _meshOffset = Vector3.zero;
			public Vector3 _meshScale = Vector3.one;
			public ShadowCastingMode _shadowCastingMode;
			public bool _billboard;
			public bool _sortByDepth;
			public bool _frustrumCull;
			public float _boundRadius;
			public float _frustrumPadding;
			public delegate void GetParticleTransform(ref ParticleSystem.Particle particle, Camera camera, out Vector3 position, out Quaternion rotation);
			public GetParticleTransform _calcParticleTransform;
			public delegate void ModifyParticlePosition(int index, ref ParticleSystem.Particle particle, ref Vector3 position);
			public ModifyParticlePosition _modifyParticlePosition;
			public delegate void ModifyParticleRotation(int index, ref ParticleSystem.Particle particle, ref Quaternion rotation);
			public ModifyParticleRotation _modifyParticleRotation;
			public delegate void ModifyParticleScale(int index, ref ParticleSystem.Particle particle, ref Vector3 scale);
			public ModifyParticleScale _modifyParticleScale;
			#endregion

			#region Private Data
			private class ParticleData
			{
				public int _index;
				public Matrix4x4 _transform;
				public float _zDist;
			}
			protected ParticleSystem _particleSystem;
			protected ParticleSystem.Particle[] _particles;
			protected MaterialPropertyBlock _propertyBlock;


			private ParticleData[] _particleData;
			private List<ParticleData> _renderedParticles;
			private Matrix4x4[] _particleTransforms;
			#endregion

			#region Monobehaviour
			private void Update()
			{
				InitialiseIfNeeded();
				Render(Camera.main);
			}
			#endregion
			
			#region Protected Functions
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

			protected void Render(Camera camera)
			{
				if (_mesh == null || _materials.Length < _mesh.subMeshCount)
					return;

				int numAlive = _particleSystem.GetParticles(_particles);
				_renderedParticles.Clear();

				if (numAlive > 0)
				{
					//Cache culling data if culling is enabled
					Plane[] planes = null;
					Vector3[] planeNormals = null;
					float[] planeDistances = null;

					if (_frustrumCull)
					{
						planes = GeometryUtility.CalculateFrustumPlanes(camera);
						planeNormals = new Vector3[planes.Length];
						planeDistances = new float[planes.Length];

						for (int i = 0; i < planes.Length; i++)
						{
							planeNormals[i] = planes[i].normal;
							planeDistances[i] = planes[i].distance;
						}
					}

					int numParticles = Math.Min(numAlive, _particleTransforms.Length);

					for (int i = 0; i < numParticles; i++)
					{
						Vector3 pos;
						Quaternion rot;

						if (_calcParticleTransform != null)
						{
							_calcParticleTransform.Invoke(ref _particles[i], camera, out pos, out rot);
						}
						else
						{
							pos = _particles[i].position;

							if (_billboard)
							{
								Vector3 forward = pos - camera.transform.position;
								rot = Quaternion.LookRotation(forward, Quaternion.AngleAxis(_particles[i].rotation, Vector3.forward) * Vector3.up);
							}
							else
							{
								rot = Quaternion.AngleAxis(_particles[i].rotation, _particles[i].axisOfRotation);
							}
						}

						pos += rot * _meshOffset;

						Vector3 scale = Vector3.Scale(_particles[i].GetCurrentSize3D(_particleSystem), _meshScale);

						_modifyParticlePosition?.Invoke(i, ref _particles[i], ref pos);
						_modifyParticleRotation?.Invoke(i, ref _particles[i], ref rot);
						_modifyParticleScale?.Invoke(i, ref _particles[i], ref scale);

						bool rendered = true;
						
						if (_frustrumCull)
						{
							rendered = IsSphereInFrustrum(ref planes, ref planeNormals, ref planeDistances, ref pos, _boundRadius * Mathf.Max(scale.x, scale.y, scale.z), _frustrumPadding);
						}

						if (rendered)
						{
							_particleData[i]._index = i;
							_particleData[i]._transform.SetTRS(pos, rot, scale);

							if (_sortByDepth)
							{
								_particleData[i]._zDist = (camera.transform.position - pos).sqrMagnitude;
							}

							AddToSortedList(ref _particleData[i]);
						}
					}

					if (_renderedParticles.Count > 0)
					{
						UpdateProperties();
						FillTransformMatricies();

						for (int i = 0; i < _mesh.subMeshCount; i++)
						{
							Graphics.DrawMeshInstanced(_mesh, i, _materials[i], _particleTransforms, _renderedParticles.Count, _propertyBlock, _shadowCastingMode);
						}
					}
				}
			}

			protected virtual Vector3 GetParticlePos(int index)
			{
				return _particles[index].position;
			}
			#endregion

			#region Private Functions
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
					index = FindInsertIndex(particleData._zDist, 0, _renderedParticles.Count);
				}	

				_renderedParticles.Insert(index, particleData);
			}

			private static readonly int kSearchNodes = 24;

			private int FindInsertIndex(float zDist, int startIndex, int endIndex)
			{
				int searchWidth = endIndex - startIndex;
				int numSearches = Mathf.Min(kSearchNodes, searchWidth);
				int nodesPerSearch = Mathf.FloorToInt(searchWidth / (float)numSearches);

				int currIndex = startIndex;
				int prevIndex = currIndex;

				for (int i =0; i<numSearches; i++)
				{
					//If this distance is greater than current node its between this and prev node
					if (zDist > _renderedParticles[currIndex]._zDist)
					{
						//If first node or search one node at a time then found our index
						if (i == 0  || nodesPerSearch == 1)
						{
							return currIndex;
						}
						//Otherwise its between this and the previous index
						else
						{
							return FindInsertIndex(zDist, prevIndex, currIndex);
						}
					}

					prevIndex = currIndex;
					currIndex = (i == numSearches - 1) ? endIndex : startIndex + ((i + 1) * nodesPerSearch);
				}

				return endIndex;
			}

			private static bool IsSphereInFrustrum(ref Plane[] frustrumPlanes, ref Vector3[] planeNormals, ref float[] planeDistances, ref Vector3 center, float radius, float frustumPadding = 0f)
			{
				for (int i = 0; i < frustrumPlanes.Length; i++)
				{
					float dist = planeNormals[i].x * center.x + planeNormals[i].y * center.y + planeNormals[i].z * center.z + planeDistances[i];

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
