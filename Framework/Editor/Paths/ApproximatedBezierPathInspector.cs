using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Paths
	{
		namespace Editor
		{
			[CustomEditor(typeof(ApproximatedBezierPath))]
			public class ApproximatedBezierPathInspector : BezierPathInspector
			{
				public override void OnInspectorGUI()
				{
					base.OnInspectorGUI();

					ApproximatedBezierPath path = (ApproximatedBezierPath)target;

					SerializedProperty property = serializedObject.FindProperty("_numPathSamples");
					EditorGUILayout.PropertyField(property);

					if (GUILayout.Button("Update Approximated Path"))
					{
						path.UpdateApproximatedPath();
					}

					serializedObject.ApplyModifiedProperties();
				}
			}
		}
	}
}