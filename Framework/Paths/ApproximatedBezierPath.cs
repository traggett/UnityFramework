using UnityEngine;


namespace Framework
{
	using Utils;

	namespace Paths
	{
		public class ApproximatedBezierPath : BezierPath
		{			
			public int _numPathSamples = 16;
			public LinearPath _approximatedPath;
			
			private void MakeSureApproximatedPathExists()
			{
				if (_approximatedPath == null)
				{
					UpdateApproximatedPath();
				}
			}

			public void UpdateApproximatedPath()
			{
				if (_approximatedPath == null)
				{
					GameObject pathObj = new GameObject("ApproximatedPath");
					pathObj.transform.parent = this.transform;
					GameObjectUtils.ResetTransform(pathObj.transform);
					_approximatedPath = pathObj.AddComponent<LinearPath>();
				}

				//The first and last node should be taken from this path.
				//Actually no all the path nodes from this path should be included in the linear path - extra ones are added in between

				//The number of new nodes need is _numPathSamples per line on path?
				//Query path T per 

				int numPathLines = _nodes.Length - 1;
				int numberOfApproxNodes = numPathLines * _numPathSamples;

				if (_approximatedPath._nodes == null || _approximatedPath._nodes.Length != numberOfApproxNodes + _nodes.Length)
				{
					if (_approximatedPath._nodes != null)
					{
						foreach (PathNodeData node in _approximatedPath._nodes)
							if (node._node != null)
								Destroy(node._node.gameObject);
					}

					_approximatedPath._nodes = new PathNodeData[numberOfApproxNodes + _nodes.Length];
					
					//For each line
					for (int n = 0; n < _nodes.Length-1; n++)
					{
						int pathIndex = (n * (_numPathSamples + 1));

						_approximatedPath._nodes[pathIndex] = _nodes[n];

						for (int i = 0; i < _numPathSamples; i++)
						{
							GameObject pathNodeObj = new GameObject("PathNode" + (pathIndex + i));
							pathNodeObj.transform.parent = _approximatedPath.transform;
							_approximatedPath._nodes[pathIndex + i + 1] = new PathNodeData();
							_approximatedPath._nodes[pathIndex + i + 1]._node = pathNodeObj.AddComponent<PathNode>();
						}
					}

					_approximatedPath._nodes[_approximatedPath._nodes.Length - 1] = _nodes[_nodes.Length - 1];

					_approximatedPath.RefreshNodes(_approximatedPath._nodes);
				}

				float pathT = 1.0f / (_numPathSamples + _nodes.Length - 1);

				//The line end is (1.0f / _nodes.Length)
				int numLines = _isLooping ? _nodes.Length : _nodes.Length - 1;
				float linePathT = 1.0f / (float)(numLines);
				float nodeT = linePathT / _numPathSamples;

				//For each line
				for (int n = 0; n < _nodes.Length - 1; n++)
				{
					int pathIndex = (n * (_numPathSamples + 1));

					for (int i = 0; i < _numPathSamples; i++)
					{
						//Set position, orientation and width based from bezier curve
						float t = linePathT * n + nodeT * i;
						PathPosition pathPos = base.GetPoint(t);
						_approximatedPath._nodes[pathIndex + i + 1]._node.transform.position = pathPos._pathPosition;
						_approximatedPath._nodes[pathIndex+ i + 1]._node.transform.rotation = Quaternion.LookRotation(pathPos._pathForward, pathPos._pathUp);
						_approximatedPath._nodes[pathIndex+ i + 1]._node.transform.localScale = Vector3.one;
						_approximatedPath._nodes[pathIndex+ i + 1]._width = pathPos._pathWidth;
						t += pathT;
					}
				}

				//Keep the approximated path disabled so its never picked by path finding - its only used as a proxy
				_approximatedPath.gameObject.SetActive(false); 
			
			}

			#region Path
			protected override PathPosition GetPointAt(float pathT)
			{
				MakeSureApproximatedPathExists();

				return _approximatedPath.GetPoint(pathT);
			}

			public override PathPosition GetClosestPoint(Vector3 position, out float distanceSqr)
			{
				MakeSureApproximatedPathExists();

				return _approximatedPath.GetClosestPoint(position, out distanceSqr);
			}

			public override PathPosition GetClosestPoint(Ray ray, out float distanceSqr)
			{
				MakeSureApproximatedPathExists();

				return _approximatedPath.GetClosestPoint(ray, out distanceSqr);
			}

			public override float GetDistanceBetween(float fromT, float toT)
			{
				MakeSureApproximatedPathExists();

				return _approximatedPath.GetDistanceBetween(fromT, toT);
			}

			public override PathPosition TravelPath(float fromT, float toT, float distance)
			{
				MakeSureApproximatedPathExists();

				return _approximatedPath.TravelPath(fromT, toT, distance);
			}
			#endregion
		}
	}
}