using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Framework
{
	namespace MeshInstancing
	{
		public interface IMeshInstance
		{
			bool IsActive();
			bool IsValid();
			bool AreBoundsInFrustrum(Plane[] cameraFrustrumPlanes);
			Vector3 GetWorldPos();
			Vector3 GetWorldScale();
			Matrix4x4 GetWorldMatrix();
			float GetSphericalBoundsRadius();
		}

		public abstract class MeshInstanceRenderer<T> : MonoBehaviour where T : IMeshInstance
		{
			#region Public Data
			public Mesh _mesh;
			public Material[] _materials;
			public ShadowCastingMode _shadowCastingMode;
			public bool _recieveShadows;
			public bool _stripInstanceChildren;
			public bool _sortByDepth;
			public enum eFrustrumCulling
			{
				Off,
				Sphere,
				Bounds
			}
			public eFrustrumCulling _frustrumCulling;
			public float _sphereCullingRadius;
			#endregion

			#region Protected Data
			protected static readonly int kMaxMeshes = 1023;
			protected T[] _instanceData;
			protected MaterialPropertyBlock _propertyBlock;
			protected class RenderData
			{
				public int _index;
				public Matrix4x4 _transform;
				public float _zDist;
			}
			protected List<RenderData> _renderedObjects;
			protected Matrix4x4[] _renderedObjectTransforms;
			#endregion

			#region Private Data
			private static readonly int kDepthSortSearchNodes = 8;
			private RenderData[] _renderData;
			#endregion

			#region Monobehaviour
			private void Awake()
			{
				Initialise();
			}
			#endregion

			#region Public Interface
			public int GetNumberInstances()
			{
				int count = 0;

				for (int i = 0; i < _instanceData.Length; i++)
				{
					if (_instanceData[i].IsValid())
					{
						count++;
					}
				}

				return count;
			}
			#endregion

			#region Protected Functions
			protected virtual void Initialise()
			{
				//Init data
				_renderedObjects = new List<RenderData>(kMaxMeshes);
				_instanceData = new T[kMaxMeshes];
				_renderData = new RenderData[kMaxMeshes];
				for (int i = 0; i < _renderData.Length; i++)
					_renderData[i] = new RenderData();
				_renderedObjectTransforms = new Matrix4x4[kMaxMeshes];

				_propertyBlock = new MaterialPropertyBlock();
			}

			protected void Render(Camera camera)
			{
				if (camera == null || _instanceData == null)
					return;

				//Find list of rendered objects
				_renderedObjects.Clear();

				Plane[] planes = null;
				Vector3[] planeNormals = null;
				float[] planeDistances = null;

				Vector3 cameraPos = camera.transform.position;

				if (_frustrumCulling != eFrustrumCulling.Off)
				{
					planes = GeometryUtility.CalculateFrustumPlanes(camera);

					if (_frustrumCulling == eFrustrumCulling.Sphere)
					{
						planeNormals = new Vector3[planes.Length];
						planeDistances = new float[planes.Length];

						for (int i = 0; i < planes.Length; i++)
						{
							planeNormals[i] = planes[i].normal;
							planeDistances[i] = planes[i].distance;
						}
					}
				}

				for (int i = 0; i < _instanceData.Length; i++)
				{
					bool rendered = _instanceData[i].IsValid() && _instanceData[i].IsActive();

					//If frustum culling is enabled, check should draw this game object
					if (rendered && _frustrumCulling == eFrustrumCulling.Bounds)
					{
						rendered = _instanceData[i].AreBoundsInFrustrum(planes);
					}
					else if (rendered && _frustrumCulling == eFrustrumCulling.Sphere)
					{
						Vector3 position = _instanceData[i].GetWorldPos();
						float radius = _instanceData[i].GetSphericalBoundsRadius();
						rendered = IsSphereInFrustrum(ref planes, ref planeNormals, ref planeDistances, position, radius);
					}

					if (rendered)
					{
						_renderData[i]._index = i;
						_renderData[i]._transform = _instanceData[i].GetWorldMatrix();

						if (_sortByDepth)
						{
							Vector3 position = new Vector3(_renderData[i]._transform.m03, _renderData[i]._transform.m13, _renderData[i]._transform.m23);
							_renderData[i]._zDist = (cameraPos - position).sqrMagnitude;
						}

						AddToSortedList(ref _renderData[i]);
					}
				}

				if (_renderedObjects.Count > 0)
				{
					FillTransformMatricies();
					UpdateProperties();
					RenderInstances();
				}
			}

			protected virtual void RenderInstances()
			{
				for (int i = 0; i < _mesh.subMeshCount; i++)
				{
					Graphics.DrawMeshInstanced(_mesh, i, _materials[i], _renderedObjectTransforms, _renderedObjects.Count, _propertyBlock, _shadowCastingMode, _recieveShadows, this.gameObject.layer);
				}
			}

			protected virtual void UpdateProperties()
			{
			}

			protected void ActivateInstance(T instance)
			{
				for (int i = 0; i < _instanceData.Length; i++)
				{
					if (!_instanceData[i].IsValid())
					{
						_instanceData[i] = instance;
						break;
					}
				}
			}
			#endregion

			#region Private Functions
			private bool AreBoundsInFrustrum(Plane[] cameraFrustrumPlanes, ref Bounds bounds)
			{
				return GeometryUtility.TestPlanesAABB(cameraFrustrumPlanes, bounds);
			}
			
			private void FillTransformMatricies()
			{
				for (int i = 0; i < _renderedObjects.Count; i++)
				{
					_renderedObjectTransforms[i] = _renderedObjects[i]._transform;
				}
			}

			private void AddToSortedList(ref RenderData renderData)
			{
				int index = 0;

				if (_sortByDepth)
				{
					index = FindInsertIndex(renderData._zDist, 0, _renderedObjects.Count);
				}

				_renderedObjects.Insert(index, renderData);
			}

			private int FindInsertIndex(float zDist, int startIndex, int endIndex)
			{
				int searchWidth = endIndex - startIndex;
				int numSearches = Mathf.Min(kDepthSortSearchNodes, searchWidth);
				int nodesPerSearch = Mathf.FloorToInt(searchWidth / (float)numSearches);

				int currIndex = startIndex;
				int prevIndex = currIndex;

				for (int i = 0; i < numSearches; i++)
				{
					//If this distance is greater than current node its between this and prev node
					if (zDist > _renderedObjects[currIndex]._zDist)
					{
						//If first node or search one node at a time then found our index
						if (i == 0 || nodesPerSearch == 1)
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

			private static bool IsSphereInFrustrum(ref Plane[] frustrumPlanes, ref Vector3[] planeNormals, ref float[] planeDistances, Vector3 center, float radius, float frustumPadding = 0f)
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
