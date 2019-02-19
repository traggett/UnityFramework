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
			public bool _sortByDepth;
			#endregion

			#region Private Data
			private ParticleSystem _particleSystem;
			private ParticleSystem.Particle[] _particles;
			private Matrix4x4[] _particleTransforms;
			private MaterialPropertyBlock _propertyBlock;
			#endregion

			#region Monobehaviour
			protected virtual void Update()
			{
				InitialiseIfNeeded();

				if (_mesh == null || _material == null)
					return;

				int numAlive = _particleSystem.GetParticles(_particles);

				if (numAlive > 0)
				{
					int numMeshes = Math.Min(numAlive, _particleTransforms.Length);
					Vector4[] batchedColorArray = new Vector4[numMeshes];

					for (int i = 0; i < numMeshes; i++)
					{
						Vector3 pos = _particles[i].position;
						Quaternion rot = Quaternion.AngleAxis(_particles[i].rotation, _particles[i].axisOfRotation);
						Vector3 scale = _particles[i].GetCurrentSize3D(_particleSystem);
						_particleTransforms[i].SetTRS(pos, rot, scale);
					}
					
					if (_sortByDepth)
						Array.Sort(_particleTransforms, SortByDistance);

					UpdateProperties();

					Graphics.DrawMeshInstanced(_mesh, 0, _material, _particleTransforms, numMeshes, _propertyBlock);
				}
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
				}
			}

			protected virtual void UpdateProperties()
			{

			}

			private int SortByDistance(Matrix4x4 a, Matrix4x4 b)
			{
				Vector3 camPos = Camera.main.transform.position;

				float aDist = (camPos - new Vector3(a.m03, a.m13, a.m23)).sqrMagnitude;
				float bDist = (camPos - new Vector3(b.m03, b.m13, b.m23)).sqrMagnitude;

				return bDist.CompareTo(aDist);
			}

			private static Vector4 ColorToVector(Color32 color)
			{
				return new Vector4(color.r, color.g, color.b, color.a);
			}
			#endregion
		}
	}
}
