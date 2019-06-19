using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Framework
{
	using Utils;

	namespace MeshInstancing
	{
		public interface IMeshInstance
		{
			bool IsActive();
			bool IsValid();
			Vector3 GetWorldPos();
			Vector3 GetWorldScale();
			void GetWorldMatrix(out Matrix4x4 matrix);
			float GetWorldBoundingSphereRadius();
			Vector3 GetWorldBoundingSphereCentre();
			Bounds GetBounds();
		}

		public abstract class MeshInstanceRenderer<T> : MonoBehaviour where T : IMeshInstance
		{
			#region Public Data
			[Range(1, 1023)]
			public int _maxMeshes = 1023;
			public Mesh _mesh;
			public Material[] _materials;
			public ShadowCastingMode _shadowCastingMode;
			public bool _recieveShadows;
			public bool _sortByDepth;
			public LayerProperty _layer;
			public enum eFrustrumCulling
			{
				Off,
				Sphere,
				Bounds
			}
			public eFrustrumCulling _frustrumCulling;
			#endregion

			#region Protected Data
			protected T[] _instanceData;
			protected MaterialPropertyBlock _propertyBlock;		
			#endregion

			#region Private Data
			private static readonly int kDepthSortSearchNodes = 8;
			private int _numRenderedObjects;
			private struct RenderData
			{
				public int _index;
				public float _zDist;
			}
			private RenderData[] _renderedObjects;
			private List<int> _renderedObjectIndexes;
			private Matrix4x4[] _renderedObjectTransforms;
			private Plane[] _frustumPlanes;
			private Vector3[] _frustumPlaneNormals;
			private float[] _frustumPlaneDistances;
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
				_instanceData = new T[_maxMeshes];
				_renderedObjects = new RenderData[_maxMeshes];
				for (int i = 0; i < _renderedObjects.Length; i++)
					_renderedObjects[i] = new RenderData();
				_renderedObjectIndexes = new List<int>(_maxMeshes);
				_renderedObjectTransforms = new Matrix4x4[_maxMeshes];

				_propertyBlock = new MaterialPropertyBlock();

				_frustumPlanes = new Plane[6];
				_frustumPlaneNormals = new Vector3[6];
				_frustumPlaneDistances = new float[6];
			}

			protected void Render(Camera camera)
			{
				if (camera == null || _instanceData == null)
					return;

				//Find list of rendered objects
				Vector3 cameraPos = camera.transform.position;

				if (_frustrumCulling != eFrustrumCulling.Off)
				{
					GeometryUtility.CalculateFrustumPlanes(camera, _frustumPlanes);

					if (_frustrumCulling == eFrustrumCulling.Sphere)
					{
						for (int i = 0; i < _frustumPlanes.Length; i++)
						{
							_frustumPlaneNormals[i] = _frustumPlanes[i].normal;
							_frustumPlaneDistances[i] = _frustumPlanes[i].distance;
						}
					}
				}

				_renderedObjectIndexes.Clear();

				for (int i = 0; i < _instanceData.Length; i++)
				{
					if (ShouldInstanceBeRendered(ref _instanceData[i]))
					{
						_renderedObjects[i]._index = i;
						
						if (_sortByDepth)
						{
							Vector3 position = _instanceData[i].GetWorldPos();
							_renderedObjects[i]._zDist = (cameraPos - position).sqrMagnitude;
						}

						AddToRenderedList(i);
					}
				}

				_numRenderedObjects = _renderedObjectIndexes.Count;

				if (_numRenderedObjects > 0)
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
					Graphics.DrawMeshInstanced(_mesh, i, _materials[i], _renderedObjectTransforms, _numRenderedObjects, _propertyBlock, _shadowCastingMode, _recieveShadows, _layer);
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

			protected int GetNumRenderedInstances()
			{
				return _numRenderedObjects;
			}

			protected int GetRenderedInstanceIndex(int renderedObjectIndex)
			{
				return _renderedObjects[_renderedObjectIndexes[renderedObjectIndex]]._index;
			}
			#endregion

			#region Private Functions
			private bool ShouldInstanceBeRendered(ref T instance)
			{
				bool rendered = instance.IsValid() && instance.IsActive();

				//If frustum culling is enabled, check should draw this game object
				if (rendered && _frustrumCulling == eFrustrumCulling.Bounds)
				{
					rendered = GeometryUtility.TestPlanesAABB(_frustumPlanes, instance.GetBounds());
				}
				else if (rendered && _frustrumCulling == eFrustrumCulling.Sphere)
				{
					Vector3 position = instance.GetWorldBoundingSphereCentre();
					float radius = instance.GetWorldBoundingSphereRadius();
					rendered = IsSphereInFrustrum(ref _frustumPlanes, ref _frustumPlaneNormals, ref _frustumPlaneDistances, position, radius);
				}

				return rendered;
			}

			private bool AreBoundsInFrustrum(Plane[] cameraFrustrumPlanes, ref Bounds bounds)
			{
				return GeometryUtility.TestPlanesAABB(cameraFrustrumPlanes, bounds);
			}
			
			private void FillTransformMatricies()
			{
				for (int i = 0; i < _numRenderedObjects; i++)
				{
					_instanceData[_renderedObjects[_renderedObjectIndexes[i]]._index].GetWorldMatrix(out _renderedObjectTransforms[i]);
				}
			}

			private void AddToRenderedList(int renderDataIndex)
			{
				if (_sortByDepth)
				{
					int index = FindDepthIndex(_renderedObjects[renderDataIndex]._zDist, 0, _renderedObjectIndexes.Count);
					_renderedObjectIndexes.Insert(index, renderDataIndex);
				}
				else
				{
					_renderedObjectIndexes.Add(renderDataIndex);
				}
			}

			private int FindDepthIndex(float zDist, int startIndex, int endIndex)
			{
				int searchWidth = endIndex - startIndex;
				int numSearches = Mathf.Min(kDepthSortSearchNodes, searchWidth);
				int nodesPerSearch = Mathf.FloorToInt(searchWidth / (float)numSearches);

				int currIndex = startIndex;
				int prevIndex = currIndex;

				for (int i = 0; i < numSearches; i++)
				{
					//If this distance is greater than current node its between this and prev node
					if (zDist > _renderedObjects[_renderedObjectIndexes[currIndex]]._zDist)
					{
						//If first node or search one node at a time then found our index
						if (i == 0 || nodesPerSearch == 1)
						{
							return currIndex;
						}
						//Otherwise its between this and the previous index
						else
						{
							return FindDepthIndex(zDist, prevIndex, currIndex);
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
