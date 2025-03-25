using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using Maths;
	using Utils;

	namespace Paths
	{
		public class BezierPath : Path
		{
			[Serializable]
			public struct NodeControlPoints
			{
				public Vector3 _startTangent;
				public Vector3 _endTangent;
			}

			[SerializeField]
			public NodeControlPoints[] _controlPoints;

			private readonly int kNumSamples = 16;

			#region Path
			public override float GetDistanceBetween(float fromT, float toT)
			{
				ClampPathT(ref fromT, ref toT);

				float distance = 0.0f;
				int numSamples = (int)Mathf.Ceil(Mathf.Abs(toT - fromT) * kNumSamples);
				float sampleT = (toT > fromT ? 1.0f : -1.0f) / (float)kNumSamples;
				float pathT = fromT;
				PathPosition pathPosition = GetPoint(fromT);

				for (int i = 0; i < numSamples; i++)
				{
					if (i == numSamples - 1)
						pathT = toT;
					else
						pathT += sampleT;

					PathPosition samplePos = GetPoint(pathT);
					Vector3 toSamplePos = samplePos._pathPosition - pathPosition._pathPosition;
					distance += toSamplePos.magnitude;
					pathPosition = samplePos;
				}

				return distance;
			}

			protected override PathPosition GetPointAt(float pathT)
			{
				GetNodeSection(pathT, out int startNode, out int endNode, out float lineLerp);

				float lookAhead = 0.01f;
				
				Vector3 pathPosition = CalcLinePoint(startNode, endNode, lineLerp);
				Vector3 pathForward;
				Vector3 pathUp;

				if (lineLerp + lookAhead >= 1.0f)
				{
					pathForward = -_controlPoints[endNode]._startTangent.normalized;
					pathUp = GetNodeUp(endNode, pathForward);
				}
				else
				{
					Vector3 nextPOs = CalcLinePoint(startNode, endNode, lineLerp + lookAhead);
					pathForward = (nextPOs - pathPosition).normalized;
					pathUp = Vector3.Lerp(GetNodeUp(startNode, pathForward), GetNodeUp(endNode, pathForward), lineLerp);
				}

				float pathWidth = Mathf.Lerp(_nodes[startNode]._width, _nodes[endNode]._width, lineLerp);

				return new PathPosition(this, pathT, pathPosition, pathForward, pathUp, pathWidth);
			}

			protected override void ValidateNodes()
			{
				base.ValidateNodes();

				if (_controlPoints.Length != _nodes.Length)
				{
					ArrayUtils.Resize(ref _controlPoints, _nodes.Length);
				}
			}

			protected override void OnNodeRemoved(int index)
			{
				ArrayUtils.RemoveAt(ref _controlPoints, index);
			}

			public override PathPosition TravelPath(float fromT, float toT, float distance)
			{
				ClampPathT(ref fromT);
				ClampPathT(ref toT);

				Direction1D direction = toT >= fromT ? Direction1D.Forwards : Direction1D.Backwards;

				float sampleT = (direction == Direction1D.Forwards ? 1.0f : -1.0f) / (float)kNumSamples;

				PathPosition pathPosition = GetPoint(fromT);
				PathPosition samplePos = GetPoint(pathPosition._pathT + sampleT);

				while (true)
				{
					Vector3 toSamplePos = samplePos._pathPosition - pathPosition._pathPosition;
					float sampleDist = toSamplePos.magnitude;

					if (distance <= sampleDist || sampleDist == 0.0f)
					{
						float distLerp = sampleDist <= 0f ? 0.0f : (distance / sampleDist);
						pathPosition._pathPosition = pathPosition._pathPosition + (toSamplePos * distLerp);
						pathPosition._pathForward = Vector3.Lerp(pathPosition._pathForward, samplePos._pathForward, distLerp);
						pathPosition._pathUp = Vector3.Lerp(pathPosition._pathUp, samplePos._pathUp, distLerp);
						pathPosition._pathT = pathPosition._pathT + (sampleT * distLerp);
						pathPosition._pathWidth = Mathf.Lerp(pathPosition._pathWidth, samplePos._pathWidth, distLerp);
						break;
					}
					else
					{
						distance -= sampleDist;
						pathPosition._pathT += sampleT;
						pathPosition._pathPosition = samplePos._pathPosition;
						pathPosition._pathForward = samplePos._pathForward;
						pathPosition._pathUp = samplePos._pathUp;
						pathPosition._pathWidth = samplePos._pathWidth;
						samplePos = GetPoint(pathPosition._pathT + sampleT);
					}
				}

				//Face opposite direction if traveling backwards
				if (direction == Direction1D.Backwards)
				{
					pathPosition._pathForward = Quaternion.AngleAxis(180f, pathPosition._pathUp) * pathPosition._pathForward;
				}

				return pathPosition;
			}

			public override PathPosition GetClosestPoint(Vector3 position, out float sqrMagnitude)
			{
				PathPosition closestPosition = default;

				float sampleT = 1.0f / (float)kNumSamples;
				float pathT = 0.0f;
				sqrMagnitude = 0.0f;

				for (int i = 0; i < kNumSamples; i++)
				{
					PathPosition samplePos = GetPoint(pathT);
					Vector3 toSamplePos = samplePos._pathPosition - position;
					float posDistance = toSamplePos.sqrMagnitude;

					if (!closestPosition.IsValid() || posDistance < sqrMagnitude)
					{
						closestPosition = samplePos;
						sqrMagnitude = posDistance;
					}

					pathT += sampleT;
				}

				return closestPosition;
			}

			public override PathPosition GetClosestPoint(Ray ray, out float distanceSqr)
			{
				throw new Exception();
			}

#if UNITY_EDITOR
			public override void OnNodeSceneGUI(PathNode node)
			{
				int i = GetNodeIndex(node);

				if (i != 0)
					HandleControlPoint(i, true);

				if (i != _nodes.Length - 1)
					HandleControlPoint(i, false);
			}

			private void HandleControlPoint(int i, bool startTangent)
			{
				EditorGUI.BeginChangeCheck();

				Vector3 nodePos = GetNodePosition(i);
				Vector3 controlPos = Handles.PositionHandle(nodePos + this.transform.TransformVector(startTangent ? _controlPoints[i]._startTangent : _controlPoints[i]._endTangent), Quaternion.identity);

				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(this, "Changed Bezier Control Point");

					if (startTangent)
						_controlPoints[i]._startTangent = this.transform.InverseTransformVector(controlPos - nodePos);
					else
						_controlPoints[i]._endTangent = this.transform.InverseTransformVector(controlPos - nodePos);

					//If first or last control, rotate node with control??
				}

				Handles.DrawLine(nodePos, controlPos);
				Handles.SphereHandleCap(0, controlPos, Quaternion.identity, _nodes[i]._width * 0.33f, EventType.Repaint);
			}
#endif
			#endregion

			private Vector3 CalcLinePoint(int startNode, int endNode, float lineLerp)
			{
				Vector3 p0 = GetNodePosition(startNode);
				Vector3 p1 = p0 + this.transform.TransformVector(_controlPoints[startNode]._endTangent);

				Vector3 p3 = GetNodePosition(endNode);
				Vector3 p2 = p3 + this.transform.TransformVector(_controlPoints[endNode]._startTangent);
				
				return CalculateCubicBezierPoint(lineLerp, p0, p1, p2, p3);
			}

			private static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
			{
				float u = 1.0f - t;
				float tt = t * t;
				float uu = u * u;
				float uuu = uu * u;
				float ttt = tt * t;
				Vector3 p = uuu * p0;
				p += 3.0f * uu * t * p1;
				p += 3.0f * u * tt * p2;
				p += ttt * p3;
				return p;
			}
		}
	}
}