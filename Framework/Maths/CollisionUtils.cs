using UnityEngine;

namespace Framework
{
	namespace Maths
	{
		public static class CollisionUtils
		{
			public static bool IsPointInsideCollider(Collider c, Vector3 p)
			{
				if (c is SphereCollider sphereCollider)
				{
					return IsPointInsideSphereCollider(sphereCollider, p);
				}
				else if (c is BoxCollider boxCollider)
				{
					return IsPointInsideBoxCollider(boxCollider, p);
				}
				else if (c is CapsuleCollider capsuleCollider)
				{
					return IsPointInsideCapsuleCollider(capsuleCollider, p);
				}
				else
				{
					Vector3 offset = c.bounds.center - p;
					Ray inputRay = new Ray(p, offset.normalized);
					if (!c.Raycast(inputRay, out _, offset.magnitude * 1.1f))
					{
						return true;
					}
				}

				return false;
			}

			public static bool IsPointInsideSphereCollider(SphereCollider c, Vector3 p)
			{
				Vector3 worldCentre = c.transform.TransformPoint(c.center);
				return (worldCentre - p).sqrMagnitude < c.radius * c.radius;
			}

			public static bool IsPointInsideBoxCollider(BoxCollider c, Vector3 p)
			{
				Vector3 localP = c.transform.InverseTransformPoint(p);
				return Mathf.Abs(c.center.x - localP.x) < c.size.x * 0.5f && Mathf.Abs(c.center.y - localP.y) < c.size.y * 0.5f && Mathf.Abs(c.center.z - localP.z) < c.size.z * 0.5f;
			}

			public static bool IsPointInsideCapsuleCollider(CapsuleCollider c, Vector3 p)
			{
				//Lossy scale should be used?!?
				Vector3 localP = c.transform.InverseTransformPoint(p);

				float halfHeight = c.height * 0.5f;
				float halfHeightWithoutRadius = halfHeight - c.radius;
				float radiusSqrd = c.radius * c.radius;

				switch (c.direction)
				{
					//X axis
					case 0:
						return Mathf.Abs(localP.x - c.center.x) < halfHeight && (localP - new Vector3(Mathf.Clamp(localP.x, c.center.x - halfHeightWithoutRadius, c.center.x + halfHeightWithoutRadius), c.center.y, c.center.z)).sqrMagnitude < radiusSqrd;
					//Y axis
					case 1:
						return Mathf.Abs(localP.y - c.center.y) < halfHeight && (localP - new Vector3(c.center.x, Mathf.Clamp(localP.y, c.center.y - halfHeightWithoutRadius, c.center.y + halfHeightWithoutRadius), c.center.z)).sqrMagnitude < radiusSqrd;
					//Z axis
					case 2:
						return Mathf.Abs(localP.z - c.center.z) < halfHeight && (localP - new Vector3(c.center.x, c.center.y, Mathf.Clamp(localP.z, c.center.z - halfHeightWithoutRadius, c.center.z + halfHeightWithoutRadius))).sqrMagnitude < radiusSqrd;
				}

				return false;
			}
		}
	}
}