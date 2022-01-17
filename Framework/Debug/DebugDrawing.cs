using UnityEngine;

namespace Framework
{
	namespace Debug
	{
		public static class DebugDrawing
		{
			public static Mesh CapsuleMesh
			{
				get
				{
					if (_capsule == null)
					{
						_capsule = Resources.GetBuiltinResource(typeof(Mesh), "Capsule.fbx") as Mesh;
					}

					return _capsule;
				}
			}
			private static Mesh _capsule;

			public static void DrawCollider(Collider collider, Color color)
			{
				Color origColor = Gizmos.color;
				Gizmos.color = color;
				Gizmos.matrix = collider.transform.localToWorldMatrix;

				if (collider is BoxCollider)
				{
					BoxCollider boxCollider = (BoxCollider)collider;
					Gizmos.DrawCube(boxCollider.center, boxCollider.size);
				}
				else if(collider is SphereCollider)
				{
					SphereCollider sphereCollider = (SphereCollider)collider;
					Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius);
				}
				else if (collider is CapsuleCollider)
				{
					CapsuleCollider capsuleCollider = (CapsuleCollider)collider;

					Quaternion rotation;

					switch (capsuleCollider.direction)
					{
						//X axis
						case 0: rotation = Quaternion.AngleAxis(90, Vector3.forward); break;
						//Z axis
						case 2: rotation = Quaternion.AngleAxis(90, Vector3.right); break;
						//Y axis
						default:
						case 1: rotation = Quaternion.identity; break;
					}

					float height = capsuleCollider.height;

					Gizmos.DrawMesh(CapsuleMesh, capsuleCollider.center, rotation, new Vector3(capsuleCollider.radius, height * 0.25f, capsuleCollider.radius));
				}
				else if (collider is MeshCollider)
				{
					MeshCollider meshCollider = (MeshCollider)collider;
					Gizmos.DrawMesh(meshCollider.sharedMesh);
				}

				Gizmos.color = origColor;
				Gizmos.matrix = Matrix4x4.identity;
			}

			public static void DrawWireArc(Vector3 position, float fromAngle, float toAngle, float radius, float numSegments = 20)
			{
				fromAngle *= Mathf.Deg2Rad;
				toAngle *= Mathf.Deg2Rad;

				if (toAngle < fromAngle)
				{
					float a = toAngle;
					toAngle = fromAngle;
					fromAngle = a;
				}
				
				float visibleRange = toAngle - fromAngle;

				if (numSegments < 1)
					numSegments = 1;

				float segmentAngle = visibleRange / numSegments;

				Vector3 positionA = position + new Vector3(Mathf.Sin(fromAngle) * radius, 0f, Mathf.Cos(fromAngle) * radius);

				Gizmos.DrawLine(position, positionA);

				for (int i = 0; i < numSegments; i++)
				{
					float nextAngle;

					if (i == numSegments - 1)
						nextAngle = toAngle;
					else
						nextAngle = fromAngle + segmentAngle;

					Vector3 positionB = position + new Vector3(Mathf.Sin(nextAngle) * radius, 0f, Mathf.Cos(nextAngle) * radius);

					Gizmos.DrawLine(positionA, positionB);

					positionA = positionB;
					fromAngle = nextAngle;
				}

				Gizmos.DrawLine(position, positionA);
			}
		}
	}
}
