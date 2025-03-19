using UnityEngine;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Framework
{
	using Maths;
	using Utils;

	namespace Paths
	{
		[Serializable]
		public class PathNodeData
		{
			public PathNode _node;
			public Vector3 _up;
			public float _width;
		}

		[ExecuteInEditMode]
		public abstract class Path : MonoBehaviour
		{
			public bool _isLooping = false;
			public Color _pathColor = Color.cyan;
			public PathNodeData[] _nodes;
			public int _debugDrawPointCount = 32;


			private void Awake()
			{
				RefreshNodes(_nodes);
			}

			protected virtual void ValidateNodes()
			{
				if (_nodes != null)
				{
					for (int i = 0; i < _nodes.Length; )
					{
						if (_nodes[i]._node == null)
						{
							OnNodeRemoved(i);
							ArrayUtils.RemoveAt(ref _nodes, i);
						}
						else
						{
							i++;
						}
					}
				}
			}

			protected void OnValidate()
			{
				ValidateNodes();
			}


#if UNITY_EDITOR
			private void Update()
			{
				if (_nodes == null)
				{
					ValidateNodes();
					RefreshNodes(_nodes);
				}
			}

			private void OnDrawGizmos()
			{
				if (enabled)
				{
					ValidateNodes();
					DebugDraw();
				}
			}
#endif

			public void RefreshNodes(params PathNodeData[] pathNodes)
			{
				if (_nodes != null)
				{
					for (int i = 0; i < _nodes.Length; i++)
					{
						if (_nodes[i]._node != null)
							_nodes[i]._node.ClearParent(this);
					}
				}

				_nodes = pathNodes;

				if (_nodes != null)
				{
					for (int i = 0; i < _nodes.Length; i++)
					{
						if (_nodes[i] != null)
							_nodes[i]._node.AddParent(this);
					}
				}
			}

			public virtual bool IsActive()
			{
				return this.gameObject.activeInHierarchy;
			}

			public PathNode GetFirstNode()
			{
				return _nodes[0]._node;
			}

			public PathNode GetLastNode()
			{
				return _nodes[_nodes.Length-1]._node;
			}

			public int GetNodeIndex(PathNode node)
			{
				for (int i = 0; i < _nodes.Length; i++)
				{
					if (node == _nodes[i]._node)
					{
						return i;
					}
				}

				return -1;
			}

			public float GetPathT(PathNode node)
			{
				//should cache path node t's ???
				for (int i = 0; i < _nodes.Length; i++)
				{
					if (node == _nodes[i]._node)
					{
						int numSections = _isLooping ? _nodes.Length : _nodes.Length - 1;
						float t = i / (float)numSections;
						return t;
					}
				}

				return 0.0f;
			}

			public PathNode GetNextNode(PathNode node, Direction1D direction)
			{
				for (int i = 0; i < _nodes.Length; i++)
				{
					if (_nodes[i]._node == node)
					{
						if (direction == Direction1D.Forwards)
						{
							if (i + 1 < _nodes.Length)
								return _nodes[i + 1]._node;
							else if (_isLooping)
								return _nodes[0]._node;
						}
						else
						{
							if (i > 0)
								return _nodes[i - 1]._node;
							else if (_isLooping)
								return _nodes[_nodes.Length - 1]._node;
						}

						break;
					}
				}

				return null;
			}

			public PathPosition GetPoint(float pathT)
			{
				ClampPathT(ref pathT);

				return GetPointAt(pathT);
			}
			
			public abstract PathPosition GetClosestPoint(Vector3 position, out float distanceSqr);
			public abstract PathPosition GetClosestPoint(Ray ray, out float distanceSqr);
			public abstract float GetDistanceBetween(float fromT, float toT);
			public abstract PathPosition TravelPath(float fromT, float toT, float distance);
			protected abstract PathPosition GetPointAt(float pathT);

			public void GetNodeSection(float pathT, out int startNode, out int endNode, out float nodeSectionT)
			{
				int numLines = _isLooping ? _nodes.Length : _nodes.Length - 1;
				float linePathT = 1.0f / (float)(numLines);

				startNode = Mathf.FloorToInt(pathT / linePathT);
				endNode = startNode < _nodes.Length - 1 ? startNode + 1 : 0;

				float startNodeT = (float)startNode * linePathT;
				nodeSectionT = (pathT - startNodeT) / linePathT;
			}

#if UNITY_EDITOR
			public void DebugDraw()
			{
				Color origColor = Handles.color;
				Handles.color = _pathColor;

				if (_nodes != null && _nodes.Length > 0)
				{
					float sampleT = 1.0f / (float)(_debugDrawPointCount - 1);
					float pathT = 0.0f;

					Vector3 prevPointA = Vector3.zero;
					Vector3 prevPointB = Vector3.zero;

					for (int i = 0; i < _debugDrawPointCount; i++)
					{
						PathPosition pos = GetPoint(pathT);

						Vector3 pathRight = Vector3.Cross(pos._pathUp, pos._pathForward).normalized;
						Vector3 pathHoriz = pathRight * (pos._pathWidth * 0.5f);

						Vector3 pointA = pos._pathPosition - pathHoriz;
						Vector3 pointB = pos._pathPosition + pathHoriz;

						Handles.DrawLine(pointA, pointB);
						
						if (i > 0)
						{
							Handles.DrawLine(prevPointA, pointA);
							Handles.DrawLine(prevPointB, pointB);
						}

						prevPointA = pointA;
						prevPointB = pointB;

						pathT += sampleT;
					}

					//Draw node up directions
					for (int i=0; i<_nodes.Length; i++)
					{
						Quaternion nodeUpRotation = Quaternion.FromToRotation(Vector3.up, _nodes[i]._up);
						Vector3 nodePos = GetNodePosition(i);
						Vector3 controlPos = nodePos + nodeUpRotation * (Vector3.up * _nodes[i]._width);

						Handles.DrawLine(nodePos, controlPos);
						Handles.SphereHandleCap(0, controlPos, Quaternion.identity, _nodes[i]._width * 0.166f, EventType.Repaint);
					}
					
					Handles.color = origColor;

				}
			}

			public virtual void OnNodeSceneGUI(PathNode node)
			{

			}
#endif

			protected virtual void OnNodeRemoved(int index)
			{

			}

			protected void ClampPathT(ref float pathT)
			{
				//Clamp T to 0-1 range
				if (_isLooping)
				{
					pathT = pathT - Mathf.Floor(pathT);
				}
				else
				{
					pathT = Mathf.Clamp(pathT, 0.0f, 1.0f);
				}
			}

			protected void ClampPathT(ref float fromT, ref float toT)
			{
				ClampPathT(ref fromT);
				ClampPathT(ref toT);

				if (fromT > toT)
				{
					float temp = fromT;
					fromT = toT;
					toT = temp;
				}
			}

			public Vector3 GetNodePosition(int index)
			{
				return _nodes[index]._node.transform.position;
			}

			public Vector3 GetNodeUp(int node, Vector3 forward)
			{
				//LHR (up X forward == right)
				Vector3 right = Vector3.Cross(_nodes[node]._up, forward);

				//LHR (forward X left == up)
				return Vector3.Cross(forward, right);
			}

			//Get up by passing a forward and a roll 
		}
	}
}