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

			protected override void OnPreRender()
			{
				_depthSortedObjects.Clear();
			}

			protected override void OnMeshShouldBeRendered(int index, Vector3 cameraPos, Vector3 cameraUp)
			{
				GetMeshRenderTransform(index, cameraPos, cameraUp, ref _sortedObjectData[index]._transform);
				_sortedObjectData[index]._index = index;
				_sortedObjectData[index]._zDist = (cameraPos - MathUtils.GetPosition(ref _sortedObjectData[index]._transform)).sqrMagnitude;

				AddToSortedList(ref _sortedObjectData[index]);
			}

			protected override void OnRenderMeshes()
			{
				_numRenderedInstances = _depthSortedObjects.Count;

				for (int i = 0; i < _numRenderedInstances; i++)
				{
					_renderedInstanceTransforms[i] = _depthSortedObjects[i]._transform;
					_onMeshInstanceWillBeRendered?.Invoke(i, _depthSortedObjects[i]._index);
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
