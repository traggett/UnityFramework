using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
	using Maths;

	namespace Paths
	{
		public static class PathRouteFinding
		{
			public struct PathRouteWaypoint
			{
				public PathNode _pathNode;
				public Path _path;
			}

			public class PathRoute
			{
				public PathPosition _startPosition;
				public PathPosition _endPosition;
				public PathRouteWaypoint[] _waypoints;
				public float _distance;

				public PathPosition GetPointAtDistance(float distance)
				{
					PathPosition pathPosition = new PathPosition(_startPosition);
					float targetT = _endPosition._pathT;

					//If the path has waypoints, travel along them
					for (int i = 0; i < _waypoints.Length; i++)
					{
						float waypointPathT = pathPosition._path.GetPathT(_waypoints[i]._pathNode);
						float distToWaypoint = pathPosition._path.GetDistanceBetween(pathPosition._pathT, waypointPathT);

						pathPosition._path = _waypoints[i]._path;
						pathPosition._pathT = _waypoints[i]._path.GetPathT(_waypoints[i]._pathNode);

						//if next waypoint is less than distance away, move towards this waypoint instead of path end
						if (distance < distToWaypoint)
						{
							targetT = waypointPathT;
							break;
						}
						else
						{
							distance -= distToWaypoint;
							i++;
						}
					}

					//Find new position
					pathPosition = pathPosition._path.TravelPath(pathPosition._pathT, targetT, distance);

					return pathPosition;
				}
			}

			public static PathPosition GetClosestPoint(Path startPath, Vector3 position)
			{
				PathPosition closestPoint = default;

				if (startPath != null)
				{
					//Traverse path network and work out nearest point
					List<Path> paths = new List<Path>();
					paths.Add(startPath);
					FindPaths(startPath, ref paths);
					
					float bestDist = 0.0f;

					//for each path find closest point and get connected paths
					foreach (Path path in paths)
					{
						float distanceSqr;
						PathPosition pathClosestPoint = path.GetClosestPoint(position, out distanceSqr);

						if (!closestPoint.IsValid() || distanceSqr < bestDist)
						{
							closestPoint = pathClosestPoint;
							bestDist = distanceSqr;
						}
					}
				}			

				return closestPoint;
			}

			public static PathPosition GetClosestPoint(Path startPath, Ray ray)
			{
				PathPosition closestPoint = default;

				if (startPath != null)
				{
					//Traverse path network and work out nearest point
					List<Path> paths = new List<Path>();
					paths.Add(startPath);
					FindPaths(startPath, ref paths);
					
					float bestDist = 0.0f;

					//for each path find closest point and get connected paths
					foreach (Path path in paths)
					{
						float distanceSqr;
						PathPosition pathClosestPoint = path.GetClosestPoint(ray, out distanceSqr);

						if (!closestPoint.IsValid() || distanceSqr < bestDist)
						{
							closestPoint = pathClosestPoint;
							bestDist = distanceSqr;
						}
					}
				}

				return closestPoint;
			}

			public static PathRoute FindRoute(PathPosition startPos, PathPosition endPos)
			{
				if (!startPos.IsValid() || !endPos.IsValid())
					return null;

				PathRoute bestRoute = null;

				List<PathNode> linkedNodes = new List<PathNode>();
				PathNode nodeA = GetNextNode(startPos, Direction1D.Forwards);
				if (nodeA != null && nodeA.isActiveAndEnabled)
				{
					linkedNodes.Add(nodeA);
				}

				PathNode nodeB = GetNextNode(startPos, Direction1D.Backwards);
				if (nodeB != null && nodeA != nodeB && nodeB.isActiveAndEnabled)
				{
					linkedNodes.Add(nodeB);
				}

				List<PathNode> traversedNodes = new List<PathNode>();

				FindRoute(startPos, endPos, startPos._path, startPos._pathT, linkedNodes, 0.0f, new List<PathRouteWaypoint>(), ref bestRoute, ref traversedNodes);

				return bestRoute;
			}
			private static List<PathNode> GetLinkedPathNodes(PathNode node, Path path)
			{
				List<PathNode> linkedNodes = new List<PathNode>();
				
				//find connecting nodes in path to node
				PathNode nodeA = path.GetNextNode(node, Direction1D.Forwards);
				if (nodeA != null && nodeA.isActiveAndEnabled)
				{
					linkedNodes.Add(nodeA);
				}

				PathNode nodeB = path.GetNextNode(node, Direction1D.Forwards);

				if (nodeB != null && nodeA != nodeB && nodeB.isActiveAndEnabled)
				{
					linkedNodes.Add(nodeB);
				}

				return linkedNodes;
			}

			private static void FindRoute(PathPosition startPos, PathPosition endPos, Path currentPath, float currentPosT, List<PathNode> nextNodes, float currentDistance, List<PathRouteWaypoint> currentRoute, ref PathRoute bestRoute, ref List<PathNode> traversedNodes)
			{
				//Check if the end position is on the current path.
				bool endPosOnCurrentPath = false;
				
				//If end position is a path node
				if (endPos._pathNode != null)
				{
					foreach (Path path in endPos._pathNode.GetPaths())
					{
						if (path == currentPath)
						{
							endPos._path = currentPath;
							endPos._pathT = currentPath.GetPathT(endPos._pathNode);
							endPosOnCurrentPath = true;
							break;
						}
					}
				}
				else if (currentPath == endPos._path)
				{
					endPosOnCurrentPath = true;
				}

				//If current path is the same as the end pos path, check direct route to end pos
				if (endPosOnCurrentPath)
				{
					float routeDistance = currentDistance + currentPath.GetDistanceBetween(currentPosT, endPos._pathT);

					if (bestRoute == null || routeDistance < bestRoute._distance)
					{
						bestRoute = new PathRoute();
						bestRoute._startPosition = startPos;
						bestRoute._endPosition = endPos;
						bestRoute._waypoints = currentRoute.ToArray();
						bestRoute._distance = routeDistance;
					}
				}

				//loop through all possible routes from this node
				foreach (PathNode node in nextNodes)
				{
					//Need to check if already searched this node (to stop loops)
					if (node != null && !traversedNodes.Contains(node))
					{
						//First work out the rating to this path node. only consider a route to this node if the rating is still better than the best route rating
						float nodeT = currentPath.GetPathT(node);
						float routeDistance = currentDistance + currentPath.GetDistanceBetween(currentPosT, nodeT);

						if (bestRoute == null || routeDistance < bestRoute._distance)
						{
							traversedNodes.Add(node);

							foreach (Path path in node.GetPaths())
							{
								if (path.IsActive())
								{
									List<PathRouteWaypoint> route = new List<PathRouteWaypoint>(currentRoute);
									PathRouteWaypoint waypoint = new PathRouteWaypoint();
									waypoint._path = path;
									waypoint._pathNode = node;
									route.Add(waypoint);

									FindRoute(startPos, endPos, path, path.GetPathT(node), GetLinkedPathNodes(node, path), routeDistance, route, ref bestRoute, ref traversedNodes);
								}
							}
						}
					}
				}
			}

			private static PathNode GetNextNode(PathPosition position, Direction1D direction)
			{
				if (position._pathNode != null)
				{
					return position._path.GetNextNode(position._pathNode, direction);
				}
				else
				{
					if (direction == Direction1D.Forwards)
					{
						for (int i = 0; i < position._path._nodes.Length; i++)
						{
							PathNode node = position._path._nodes[i]._node;

							if (position._path.GetPathT(node) >= position._pathT)
							{
								return node;
							}
						}
					}
					else
					{
						for (int i = 0; i < position._path._nodes.Length; i++)
						{
							PathNode node = position._path._nodes[i]._node;

							if (position._path.GetPathT(node) <= position._pathT)
							{
								return node;
							}
						}
					}

					return null;
				}
			}

			private static void FindPaths(Path path, ref List<Path> paths)
			{
				if (path != null)
				{
					foreach (PathNodeData node in path._nodes)
					{
						if (node._node == null || node._node.GetPaths() == null)
							throw new System.Exception();

						foreach (Path nodePath in node._node.GetPaths())
						{
							if (nodePath == null)
								throw new System.Exception();

							if (nodePath.IsActive() && !paths.Contains(nodePath))
							{
								paths.Add(nodePath);
								FindPaths(nodePath, ref paths);
							}
						}
					}
				}
			}
		}
	}
}