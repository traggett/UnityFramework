using UnityEngine;
using Framework.Maths;

namespace Framework
{
	namespace Paths
	{
		public class LinearPath : Path
		{	
			#region IPath 
			public override float GetDistanceBetween(float fromT, float toT)
			{
				ClampPathT(ref fromT, ref toT);

				int numLines = _isLooping ? _nodes.Length : _nodes.Length - 1;
				float linePathT = 1.0f / (float)(numLines);

				//Loop through path sections, starting with section fromT is on
				int startNode = Mathf.FloorToInt(fromT / linePathT);
				int endNode = Mathf.CeilToInt(toT / linePathT);

				float distance = 0.0f;

				for (int i = startNode; i < endNode; i++)
				{
					int toNode = (i < _nodes.Length - 1) ? i + 1 : 0;
					Vector3 startNodePos = GetNodePosition(startNode);
					Vector3 endNodePos = GetNodePosition(toNode);

					Vector3 line = endNodePos - startNodePos;
					float lineDist = line.magnitude;

					//Last line
					if (i == endNode-1)
					{
						//Start and end on same line
						if (i == startNode)
						{
							distance = lineDist * (toT - fromT);
						}
						else
						{
							distance += lineDist * toT;
						}

						break;
					}
					//First line
					else if(i == startNode)
					{
						distance = lineDist * (1.0f - fromT);
					}
					else
					{
						distance += lineDist;
					}
				}

				return distance;
			}

			protected override PathPosition GetPointAt(float pathT)
			{
				GetNodeSection(pathT, out int startNode, out int endNode, out float lineLerp);

				Vector3 startNodePos = GetNodePosition(startNode);
				Vector3 endNodePos = GetNodePosition(endNode);

				Vector3 lineDirection = (endNodePos - startNodePos).normalized;

				Vector3 startNodeUp = GetNodeUp(startNode, lineDirection);
				Vector3 endNodeUp = GetNodeUp(endNode, lineDirection);

				return new PathPosition(this, pathT,
					Vector3.Lerp(startNodePos, endNodePos, lineLerp),
					lineDirection,
					Vector3.Lerp(startNodeUp, endNodeUp, lineLerp),
					Mathf.Lerp(_nodes[startNode]._width, _nodes[endNode]._width, lineLerp));
			}

			public override PathPosition TravelPath(float fromT, float toT, float distance)
			{
				ClampPathT(ref fromT);
				ClampPathT(ref toT);

				Direction1D direction = toT >= fromT ? Direction1D.Forwards : Direction1D.Backwards;

				int numLines = _isLooping ? _nodes.Length : _nodes.Length - 1;
				float linePathT = 1.0f / (float)(numLines);

				PathPosition pathPosition = GetPoint(fromT);
				
				int startNode = Mathf.FloorToInt(fromT / linePathT);
				int endNode = Mathf.FloorToInt(toT / linePathT);

				//Work out current position
				{
					int toNode = startNode < _nodes.Length - 1 ? startNode + 1 : 0;
					Vector3 startNodePos = GetNodePosition(startNode);
					Vector3 endNodePos = GetNodePosition(toNode);

					Vector3 line = endNodePos - startNodePos;
					float startNodeT = (float)startNode * linePathT;
					float lineLerp = (fromT - startNodeT) / linePathT;
					
					//Move along this line by distance
					//if start and end are on the same line
					float lineLeft = direction == Direction1D.Forwards ? 1.0f - lineLerp : lineLerp;
					float lineDist = line.magnitude;
					float lineDistLeft = lineDist * lineLeft;

					//if on same line, find distance to that point
					if (startNode == endNode)
					{
						float distanceToTarget = Mathf.Abs(toT - fromT) * lineDist;
						distance = Mathf.Min(distanceToTarget, distance);
					}

					if (distance <= lineDistLeft)
					{
						float distanceFraction = distance / lineDist;

						if (direction == Direction1D.Forwards)
						{
							pathPosition._pathPosition += line.normalized * distance;
							pathPosition._pathT += distanceFraction * linePathT;
						}
						else
						{
							pathPosition._pathPosition -= line.normalized * distance;
							pathPosition._pathT -= distanceFraction * linePathT;
						}

						return pathPosition;
					}
					else
					{
						distance -= lineDistLeft;

						if (direction == Direction1D.Forwards)
							startNode++;
					}
				}

				//Loop through remaining lines
				if (direction == Direction1D.Forwards)
				{
					for (int i = startNode; i < numLines; i++)
					{
						int toNode = (i < _nodes.Length - 1) ? i + 1 : 0;
						Vector3 fromNodePos = GetNodePosition(i);
						Vector3 toNodePos = GetNodePosition(toNode);

						Vector3 line = toNodePos - fromNodePos;
						float lineDist = line.magnitude;
						
						if (i == endNode)
						{
							float distanceToTarget = Mathf.Abs(toT - fromT) * lineDist;
							distance = Mathf.Min(distanceToTarget, distance);
						}

						if (distance <= lineDist)
						{
							float lineLerp = (distance / lineDist);
							pathPosition._pathPosition = fromNodePos + lineLerp * line.normalized;
							pathPosition._pathT = ((float)i + lineLerp) * linePathT;
							break;
						}
						else
						{
							distance -= lineDist;
						}
					}
				}
				else
				{
					for (int i = startNode; i > (_isLooping ? -1 : 0); i--)
					{
						int toNode = (i > 0) ? i - 1 : _nodes.Length-1;
						Vector3 fromNodePos = GetNodePosition(i);
						Vector3 toNodePos = GetNodePosition(toNode);
						Vector3 line = toNodePos - fromNodePos;
						float lineDist = line.magnitude;

						if (i == endNode)
						{
							float distanceToTarget = Mathf.Abs(toT - fromT) * lineDist;
							distance = Mathf.Min(distanceToTarget, distance);
						}

						if (distance <= lineDist)
						{
							float lineLerp = distance / lineDist;
							pathPosition._pathPosition = fromNodePos + lineLerp * line.normalized;
							pathPosition._pathT = ((float)i - lineLerp) * linePathT;
							break;
						}
						else
						{
							distance -= lineDist;
						}
					}
				}

				return pathPosition;
			}

			public override PathPosition GetClosestPoint(Vector3 position, out float distanceSqr)
			{
				PathPosition closestPosition = default;

				int numLines = _isLooping ? _nodes.Length : _nodes.Length - 1;
				float linePathT = 1.0f / (float)(numLines);

				distanceSqr = 0.0f;

				for (int i = 0; i < numLines; i++)
				{
					//Get closest point on line
					int toNode = (i < _nodes.Length - 1) ? i + 1 : 0;
					float lineLerp = 0.0f;
					float dist = 0.0f;
					float width;

					Vector3 up, forward;
					Vector3 closestPoint = GetClosestPointToLine(i, toNode, position, out dist, out lineLerp, out forward, out up, out width);
					
					if (!closestPosition.IsValid() || dist < distanceSqr)
					{
						closestPosition = new PathPosition(this, ((float)i + lineLerp) * linePathT, closestPoint, forward, up, width);
						distanceSqr = dist;
					}
				}

				return closestPosition;
			}

			public override PathPosition GetClosestPoint(Ray ray, out float distanceSqr)
			{
				PathPosition closestPosition = default;

				int numLines = _isLooping ? _nodes.Length : _nodes.Length - 1;
				float linePathT = 1.0f / (float)(numLines);

				distanceSqr = 0.0f;

				for (int i = 0; i < numLines; i++)
				{
					//THIS is wrong - path line should be of fixed length!


					//Get closest point on line
					int toNode = (i < _nodes.Length - 1) ? i + 1 : 0;
					float lineLerp = 0.0f;
					float dist;

					Vector3 fromNodePos = GetNodePosition(i);
					Vector3 toNodePos = GetNodePosition(toNode);

					Vector3 closestPoint;
					Vector3 pathLineDir = (toNodePos - fromNodePos).normalized;
					if (MathUtils.ClosestPointsOnTwoLines(ray.origin, ray.direction, fromNodePos, pathLineDir, out _, out float closestPointPathLineT))
					{
						closestPoint = fromNodePos + pathLineDir * closestPointPathLineT;
						Vector3 closestPointOnRay = ray.origin + ray.direction * closestPointPathLineT;
						dist = (closestPointOnRay - closestPoint).sqrMagnitude;
					}
					//Lines are parallel - use start points of both lines 
					else
					{
						closestPoint = fromNodePos;
						dist = (closestPoint - ray.origin).sqrMagnitude;
					}

					if (!closestPosition.IsValid() || dist < distanceSqr)
					{
						Vector3 startNodeUp = GetNodeUp(i, pathLineDir);
						Vector3 endNodeUp = GetNodeUp(toNode, pathLineDir);

						closestPosition = new PathPosition(this, ((float)i + lineLerp) * linePathT, closestPoint,
							pathLineDir,
							Vector3.Lerp(startNodeUp, endNodeUp, lineLerp),
							Mathf.Lerp(_nodes[i]._width, _nodes[toNode]._width, lineLerp));

						distanceSqr = dist;
					}
				}

				return closestPosition;
			}

			public Vector3 GetClosestPointToLine(int start, int end, Vector3 point, out float distanceSqr, out float lineT, out Vector3 forward, out Vector3 up, out float width)
			{
				Vector3 fromNodePos = GetNodePosition(start);
				Vector3 toNodePos = GetNodePosition(end);
				Vector3 line = toNodePos - fromNodePos;
				float len = line.sqrMagnitude;

				if (len == 0.0)
				{
					distanceSqr = (point - fromNodePos).sqrMagnitude;
					lineT = 0.0f;
					forward = Vector3.forward;
					up = Vector3.up;
					width = _nodes[start]._width;
					return fromNodePos;
				}
				else
				{
					Vector3 toPoint = point - fromNodePos;
					lineT = Mathf.Clamp(Vector3.Dot(toPoint, line) / len, 0.0f, 1.0f);

					Vector3 closestPointOnLine = fromNodePos + (lineT * line);
					distanceSqr = (point - closestPointOnLine).sqrMagnitude;

					

					forward = line.normalized;

					Vector3 startNodeUp = GetNodeUp(start, forward);
					Vector3 endNodeUp = GetNodeUp(end, forward);
					up = Vector3.Lerp(startNodeUp, endNodeUp, lineT);

					width = Mathf.Lerp(_nodes[start]._width, _nodes[end]._width, lineT);

					return closestPointOnLine;
				}
			}
			#endregion
		}
	}
}