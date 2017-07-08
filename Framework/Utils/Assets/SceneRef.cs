using System;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Framework
{
	using Serialization;

	namespace Utils
	{
		[Serializable]
		public sealed class SceneRef : ICustomEditorInspector
		{
			#region Public Data
			public string _scenePath;
			#endregion

			public static implicit operator string(SceneRef property)
			{
				return property.IsSceneValid() ? GetSceneNameFromPath(property._scenePath) : "No Scene";
			}

			public Scene GetScene()
			{
				Scene scene = new Scene();

				if (IsSceneValid())
				{
#if UNITY_EDITOR
					scene = EditorSceneManager.GetSceneByPath(_scenePath);
#else
					scene = SceneManager.GetSceneByPath(_scenePath);
#endif
				}

				return scene;
			}

			public string GetSceneName()
			{
				return GetSceneNameFromPath(_scenePath);
			}

			public bool IsSceneValid()
			{
				return !string.IsNullOrEmpty(_scenePath);
			}

			private static string GetSceneNameFromPath(string scenePath)
			{
				int folder = scenePath.LastIndexOf("/") + 1;
				int file = scenePath.LastIndexOf(".");
				return scenePath.Substring(folder, file - folder);
			}

#if UNITY_EDITOR
			public void ClearScene()
			{
				_scenePath = null;
			}

			public void SetScene(Scene scene)
			{
				if (scene.IsValid())
				{
					_scenePath = scene.path;
				}
				else
				{
					_scenePath = null;
				}
			}

			public void OpenSceneInEditor()
			{
				if (IsSceneValid())
				{
					EditorSceneManager.OpenScene(_scenePath, OpenSceneMode.Additive);
				}
			}
#endif

			#region ICustomEditable
#if UNITY_EDITOR
			public bool RenderObjectProperties(GUIContent label)
			{
				GUIContent[] sceneNames = new GUIContent[EditorBuildSettings.scenes.Length+1];
				string[] scenePaths = new string[EditorBuildSettings.scenes.Length];

				sceneNames[0] = new GUIContent("(No Scene)");

				int currIndex = 0;
				for (int i=0; i< EditorBuildSettings.scenes.Length; i++)
				{
					EditorBuildSettingsScene editorScene = EditorBuildSettings.scenes[i];
					sceneNames[i+1] = new GUIContent(GetSceneNameFromPath(editorScene.path));
					scenePaths[i] = editorScene.path;

					if (editorScene.path == _scenePath)
					{
						currIndex = i+1;
					}
				}

				EditorGUI.BeginChangeCheck();
				currIndex = EditorGUILayout.Popup(label, currIndex, sceneNames);
				if (EditorGUI.EndChangeCheck())
				{
					_scenePath = currIndex == 0 ? null : scenePaths[currIndex - 1];
					return true;
				}

				return false;
			}
#endif
			#endregion
		}
	}
}