using Framework.Maths;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		public class DepthSortedMeshInstancer : MeshInstancer 
		{
			#region Private Data
			private class SortedMeshData
			{
				public int _index;
				public Matrix4x4 _transform;
				public float _zDist;
			}
			private SortedMeshData[] _sortedObjectData;
			private List<SortedMeshData> _depthSortedObjects;
			#endregion

			#region MeshInstancer
			protected override void InitialiseIfNeeded()
			{
				base.InitialiseIfNeeded();

				if (_sortedObjectData == null)
				{
					_sortedObjectData = new SortedMeshData[kMaxInstances];
					for (int i = 0; i < kMaxInstances; i++)
						_sortedObjectData[i] = new SortedMeshData();

					_depthSortedObjects = new List<SortedMeshData>(kMaxInstances);
				}
			}

			protected override void Render(Camera camera)
			{
				if (_mesh == null || _materials.Length < _mesh.subMeshCount)
					return;

				_numRenderedInstances = 0;
				_depthSortedObjects.Clear();

				_frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);

				for (int i = 0; i < _frustrumPlanes.Length; i++)
				{
					_frustrumPlaneNormals[i] = _frustrumPlanes[i].normal;
					_frustrumPlaneDistances[i] = _frustrumPlanes[i].distance;
				}

				Vector3 cameraPos = camera.transform.position;

				for (int i = 0; i < kMaxInstances; i++)
				{
					if (_instanceActive[i])
					{
						if (IsMeshInFrustrum(i))
						{
							_sortedObjectData[i]._index = i;
							_sortedObjectData[i]._transform = GetModifiedTransform(i, cameraPos);
							_sortedObjectData[i]._zDist = (cameraPos - MathUtils.GetPosition(ref _sortedObjectData[i]._transform)).sqrMagnitude;

							AddToSortedList(ref _sortedObjectData[i]);
						}
					}
				}

				_numRenderedInstances = _depthSortedObjects.Count;

				if (_numRenderedInstances > 0)
				{
					for (int i = 0; i < _numRenderedInstances; i++)
					{
						_renderedInstanceTransforms[i] = _depthSortedObjects[i]._transform;
						_onMeshInstanceWillBeRendered?.Invoke(i, _depthSortedObjects[i]._index);
					}

					_onUpdateMaterialPropertyBlock?.Invoke(_propertyBlock);

					for (int i = 0; i < _mesh.subMeshCount; i++)
					{
						Graphics.DrawMeshInstanced(_mesh, i, _materials[i], _renderedInstanceTransforms, _numRenderedInstances, _propertyBlock, _shadowCastingMode);
					}
				}
			}
			#endregion

			#region Private Functions
			private void AddToSortedList(ref SortedMeshData particleData)
			{
				int index = FindInsertIndex(particleData._zDist, 0, _depthSortedObjects.Count);
				_depthSortedObjects.Insert(index, particleData);
			}

			private static readonly int kSearchNodes = 16;

			private int FindInsertIndex(float zDist, int startIndex, int endIndex)
			{
				int searchWidth = endIndex - startIndex;
				int numSearches = Mathf.Min(kSearchNodes, searchWidth);
				int nodesPerSearch = Mathf.FloorToInt(searchWidth / (float)numSearches);

				int currIndex = startIndex;
				int prevIndex = currIndex;

				for (int i = 0; i < numSearches; i++)
				{
					//If this distance is greater than current node its between this and prev node
					if (zDist > _depthSortedObjects[currIndex]._zDist)
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
			#endregion
		}
	}
}
