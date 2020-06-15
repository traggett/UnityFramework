using Framework.Utils;

using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Paths
	{
		namespace Editor
		{
			[CustomEditor(typeof(BezierPath))]
			public class BezierPathInspector : PathInspector
			{
				public override void OnInspectorGUI()
				{
					base.OnInspectorGUI();

					SerializedProperty property = serializedObject.FindProperty("_debugDrawPointCount");
					EditorGUILayout.PropertyField(property);

					SerializedProperty colorProperty = serializedObject.FindProperty("_pathColor");
					EditorGUILayout.PropertyField(colorProperty);
					
					serializedObject.ApplyModifiedProperties();
				}

				protected override void OnAddedNode(PathNode node)
				{
					BezierPath path = (BezierPath)target;

					BezierPath.NodeControlPoints controlPoint = new BezierPath.NodeControlPoints();
					controlPoint._startTangent = Vector3.forward;
					controlPoint._endTangent = -Vector3.forward;

					ArrayUtils.Add(ref path._controlPoints, controlPoint);

					if (path._controlPoints.Length != path._nodes.Length)
						throw new System.Exception();
				}

				protected override void OnRemovedNode(int index)
				{
					BezierPath path = (BezierPath)target;

					ArrayUtils.RemoveAt(ref path._controlPoints, index);

					if (path._controlPoints.Length != path._nodes.Length)
						throw new System.Exception();
				}

				protected override void OnNodeChangedPosition(int oldIndex, int newIndex)
				{
					BezierPath path = (BezierPath)target;

					BezierPath.NodeControlPoints origControlPoint = path._controlPoints[oldIndex];
					path._controlPoints[oldIndex] = path._controlPoints[newIndex];
					path._controlPoints[newIndex] = origControlPoint;

					if (path._controlPoints.Length != path._nodes.Length)
						throw new System.Exception();
				}
			}
		}
	}
}