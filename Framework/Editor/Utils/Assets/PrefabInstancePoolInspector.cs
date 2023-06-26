using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			[CustomEditor(typeof(PrefabInstancePool))]
			public class PrefabInstancePoolInspector : UnityEditor.Editor
			{
				public override void OnInspectorGUI()
				{
					PrefabInstancePool prefabInstancePool = target as PrefabInstancePool;

					SerializedProperty prefabProperty = serializedObject.FindProperty("_prefab");
					SerializedProperty initialiseOnAwakeProperty = serializedObject.FindProperty("_initialiseOnAwake");
					SerializedProperty initialPoolSizeProperty = serializedObject.FindProperty("_initialPoolSize");
					SerializedProperty growAmountProperty = serializedObject.FindProperty("_growAmount");

					prefabProperty.objectReferenceValue = EditorGUILayout.ObjectField("Prefab", prefabProperty.objectReferenceValue, typeof(GameObject), false);

					EditorGUILayout.PropertyField(initialiseOnAwakeProperty);
					EditorGUILayout.PropertyField(initialPoolSizeProperty);
					EditorGUILayout.PropertyField(growAmountProperty);

					EditorGUILayout.Separator();

					EditorGUILayout.HelpBox(new GUIContent(string.Format("Current Pool Size: {0} Number active Prefabs: {1}", + prefabInstancePool.PoolSize, prefabInstancePool.ActivePrefabCount)));

					EditorGUILayout.Separator();

					EditorGUILayout.BeginHorizontal();
					{
						if (GUILayout.Button("Initialize Pool"))
						{
							//If pool size is different resize current pool!

							prefabInstancePool.InitialisePool();
						}

						if (GUILayout.Button("Clear Pool"))
						{
							prefabInstancePool.DeInitialisePool();
						}
					}
					EditorGUILayout.EndHorizontal();

					serializedObject.ApplyModifiedProperties();
				}
			}
		}
	}
}