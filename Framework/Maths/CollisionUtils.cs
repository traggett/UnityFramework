using UnityEngine;

namespace Framework
{
	namespace Maths
	{
		public static class CollisionUtils
		{
			public static bool IsPointInsideCollider(Collider collider, Vector3 point)
			{
				if (collider is SphereCollider sphereCollider)
				{
					return IsPointInsideSphereCollider(sphereCollider, point);
				}
				else if (collider is BoxCollider boxCollider)
				{
					return IsPointInsideBoxCollider(boxCollider, point);
				}
				else if (collider is CapsuleCollider capsuleCollider)
				{
					return IsPointInsideCapsuleCollider(capsuleCollider, point);
				}
				else
				{
					Vector3 offset = collider.bounds.center - point;
					Ray inputRay = new Ray(point, offset.normalized);
					if (!collider.Raycast(inputRay, out _, offset.magnitude * 1.1f))
					{
						return true;
					}
				}

				return false;
			}

			public static bool IsPointInsideSphereCollider(SphereCollider collider, Vector3 point)
			{
				Vector3 localspacePoint = collider.transform.InverseTransformPoint(point);
				Vector3 worldCentre = collider.transform.TransformPoint(collider.center);
				return (worldCentre - localspacePoint).sqrMagnitude < collider.radius * collider.radius;
			}

			public static bool IsPointInsideBoxCollider(BoxCollider collider, Vector3 point)
			{
				Vector3 localspacePoint = collider.transform.InverseTransformPoint(point);
				return Mathf.Abs(collider.center.x - localspacePoint.x) < collider.size.x * 0.5f && Mathf.Abs(collider.center.y - localspacePoint.y) < collider.size.y * 0.5f && Mathf.Abs(collider.center.z - localspacePoint.z) < collider.size.z * 0.5f;
			}

			public static bool IsPointInsideCapsuleCollider(CapsuleCollider collider, Vector3 point)
			{
				Vector3 localspacePoint = collider.transform.InverseTransformPoint(point);

				float halfHeight = collider.height * 0.5f;
				float halfHeightWithoutRadius = halfHeight - collider.radius;
				float radiusSqrd = collider.radius * collider.radius;

				switch (collider.direction)
				{
					//X axis
					case 0:
						return Mathf.Abs(localspacePoint.x - collider.center.x) < halfHeight && (localspacePoint - new Vector3(Mathf.Clamp(localspacePoint.x, collider.center.x - halfHeightWithoutRadius, collider.center.x + halfHeightWithoutRadius), collider.center.y, collider.center.z)).sqrMagnitude < radiusSqrd;
					//Y axis
					case 1:
						return Mathf.Abs(localspacePoint.y - collider.center.y) < halfHeight && (localspacePoint - new Vector3(collider.center.x, Mathf.Clamp(localspacePoint.y, collider.center.y - halfHeightWithoutRadius, collider.center.y + halfHeightWithoutRadius), collider.center.z)).sqrMagnitude < radiusSqrd;
					//Z axis
					case 2:
						return Mathf.Abs(localspacePoint.z - collider.center.z) < halfHeight && (localspacePoint - new Vector3(collider.center.x, collider.center.y, Mathf.Clamp(localspacePoint.z, collider.center.z - halfHeightWithoutRadius, collider.center.z + halfHeightWithoutRadius))).sqrMagnitude < radiusSqrd;
				}

				return false;
			}


			// Find the points of intersection.
			public static int Get2dLineCircleIntersection(Vector2 circleCentre, float circleRadius, Vector2 lineStart, Vector2 lineDir, out Vector2 S1, out Vector2 S2)
			{
				Vector2 h = circleCentre - lineStart;

				float lf = Vector2.Dot(lineDir, h);

				float s = (circleRadius * circleRadius) - Vector2.Dot(h, h) + (lf * lf);   // s=r^2-h^2+lf^2

				// no intersection points ?
				if (s < 0.0)
				{
					S1 = Vector2.zero;
					S2 = Vector2.zero;
					return 0;
				}

				s = Mathf.Sqrt(s);    // s=sqrt(r^2-h^2+lf^2)

				int result = 0;

				if (lf < s)                               // S1 behind A ?
				{
					if (lf + s >= 0)                          // S2 before A ?}
					{
						s = -s;                               // swap S1 <-> S2}
						result = 1;                           // one intersection point
					}
				}
				else
				{
					result = 2;                          // 2 intersection points
				}


				S1 = lineStart + lineDir * (lf - s);
				S2 = lineStart + lineDir * (lf + s);

				return result;
			}

		}
	}
}