using UnityEngine;

namespace Framework
{
	namespace Paths
	{
		[System.Serializable]
		public class PathPosition
		{
			public Path _path;
			public float _pathT;
			public PathNode _pathNode;
			public Vector3 _pathPosition;
			public Vector3 _pathForward;
			public Vector3 _pathUp;
			public float _pathWidth;

			public PathPosition(Path path, float pathT, Vector3 pathPosition, Vector3 pathForward, Vector3 pathUp, float pathWidth)
			{
				_path = path;
				_pathT = pathT;
				_pathNode = null;
				_pathPosition = pathPosition;
				_pathForward = pathForward;
				_pathUp = pathUp;
				_pathWidth = pathWidth;
			}

			public PathPosition(PathPosition other)
			{
				_path = other._path;
				_pathT = other._pathT;
				_pathNode = other._pathNode;
				_pathPosition = other._pathPosition;
				_pathForward = other._pathForward;
				_pathUp = other._pathUp;
				_pathWidth = other._pathWidth;
			}

#if UNITY_EDITOR
			public static void DrawPathPosGizmo(PathPosition position)
			{
				DrawPathPosGizmo(position, Color.green);
			}

			public static void DrawPathPosGizmo(PathPosition position, Color color)
			{
				if (position != null)
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