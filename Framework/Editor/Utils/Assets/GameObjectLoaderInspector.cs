using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace Utils
	{
		using Editor;

		[CustomEditor(typeof(GameObjectLoader), true)]
		public class GameObjectLoaderInspector : UnityEditor.Editor
		{
			private GameObject _prefabAsset;
			private SerializedProperty _filePath;
			private SerializedProperty _fileGUID;

			void OnEnable()
			{
				GameObjectLoader prefabLoader = (GameObjectLoader)target;

				_filePath = serializedObject.FindProperty("_filePath");
				_fileGUID = serializedObject.FindProperty("_fileGUID");

				if (!string.IsNullOrEmpty(_fileGUID.stringValue))
				{
					string filepath = AssetDatabase.GUIDToAssetPath(_fileGUID.stringValue);
					_prefabAsset = AssetDatabase.LoadAssetAtPath(filepath, typeof(GameObject)) as GameObject;
				}

				if (_prefabAsset == null && !string.IsNullOrEmpty(_filePath.stringValue))
				{
					_prefabAsset = AssetDatabase.LoadAssetAtPath(_filePath.stringValue, typeof(GameObject)) as GameObject;
				}
			}

			public override void OnInspectorGUI()
			{
				GameObjectLoader prefabLoader = (GameObjectLoader)target;

				EditorGUI.BeginChangeCheck();

				_prefabAsset = EditorGUILayout.ObjectField("Source Prefab", _prefabAsset, typeof(GameObject), false) as GameObject;

				if (EditorGUI.EndChangeCheck())
				{
					if (_prefabAsset != null)
					{
						_filePath.stringValue = AssetDatabase.GetAssetPath(_prefabAsset);
						_fileGUID.stringValue = AssetDatabase.AssetPathToGUID(_filePath.stringValue);
					}
					else
					{
						_filePath.stringValue = null;
						_fileGUID.stringValue = null;
					}

					serializedObject.ApplyModifiedProperties();
				}

				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(EditorUtils.GetLabelWidth()));

					if (GUILayout.Button("Load"))
					{
						prefabLoader.Load();
					}

					if (GUILayout.Button("Unload"))
					{
						prefabLoader.Unload();
					}

					GUILayout.FlexibleSpace();
				}
				EditorGUILayout.EndHorizontal();
			}
		}
	}
}