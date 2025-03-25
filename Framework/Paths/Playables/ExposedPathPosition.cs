using UnityEngine;

namespace Framework
{
	namespace Paths
	{
		[System.Serializable]
		public class ExposedPathPosition
		{
			public ExposedReference<Path> _path;
			public float _pathT;
			public ExposedReference<PathNode> _pathNode;
			public Vector3 _pathPosition;
			public Vector3 _pathForward;
			public Vector3 _pathUp;
			public float _pathWidth;

			public PathPosition GetPathPosition(IExposedPropertyTable resolver)
			{
				Path path = _path.Resolve(resolver);
				// PathNode pathNode = _pathNode.Resolve(resolver);

				return new PathPosition(path, _pathT, _pathPosition, _pathForward, _pathUp, _pathWidth);
			}

#if UNITY_EDITOR
			public static void DrawPathPosGizmo(PathPosition position)
			{
				DrawPathPosGizmo(position, Color.green);
			}

			public static void DrawPathPosGizmo(PathPosition position, Color color)
			{
				if (position.IsValid())
				{
					Color origColor = UnityEditor.Handles.color;
					UnityEditor.Handles.color = color;
					UnityEditor.Handles.SphereHandleCap(2, position._pathPosition, Quaternion.identity, 0.1f, EventType.Repaint);
					UnityEditor.Handles.color = origColor;
				}
			}
#endif
		}
	}
}