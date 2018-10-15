#if DEBUG

using UnityEngine;

namespace Framework
{
	namespace Utils
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
		}
	}
}

#endif