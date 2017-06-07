using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			[CustomEditor(typeof(SceneIndexer))]
			public class SceneIndexerInspector : UnityEditor.Editor
			{
				public override void OnInspectorGUI()
				{
					EditorGUILayout.Separator();

					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(EditorGUIUtility.labelWidth - (EditorGUI.indentLevel * 15.0f) + 11));

						if (GUILayout.Button("Rebuild Scene", GUILayout.ExpandWidth(false)))
						{
							SceneIndexer indexer = target as SceneIndexer;
							indexer.CacheSceneObjects();
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			}
		}
	}
}