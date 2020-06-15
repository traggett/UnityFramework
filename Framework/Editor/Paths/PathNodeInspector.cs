using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Paths
	{
		namespace Editor
		{
			[CustomEditor(typeof(PathNode))]
			[CanEditMultipleObjects]
			public class PathNodeInspector : UnityEditor.Editor
			{
				public override void OnInspectorGUI()
				{
					serializedObject.ApplyModifiedProperties();
				}
			}
		}
	}
}