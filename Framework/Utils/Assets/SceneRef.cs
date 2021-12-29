using System;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Framework
{
	namespace Utils
	{
		[Serializable]
		public struct SceneRef
		{
			#region Public Data
			[SerializeField]
			private UnityEngine.Object _sceneAsset;
			[SerializeField]
			private string _scenePath;
			#endregion

			public SceneRef(string scenePath)
			{
#if UNITY_EDITOR
				_sceneAsset = AssetDatabase.LoadAssetAtPath(scenePath, typeof(SceneAsset));
#else
				_sceneAsset = null;
#endif
				_scenePath = scenePath;
			}

			public SceneRef(Scene scene)
			{
				if (scene.IsValid())
				{
#if UNITY_EDITOR
					_sceneAsset = AssetDatabase.LoadAssetAtPath(scene.path, typeof(SceneAsset));
#else
					_sceneAsset = null;
#endif
					_scenePath = scene.path;
				}
				else
				{
					_sceneAsset = null;
					_scenePath = null;
				}
			}

#if UNITY_EDITOR
			public SceneRef(SceneAsset sceneAsset)
			{
				if (sceneAsset != null)
				{
					_sceneAsset = sceneAsset;
					_scenePath = AssetDatabase.GetAssetPath(sceneAsset);
				}
				else
				{
					_sceneAsset = null;
					_scenePath = null;
				}
			}
#endif

			public static implicit operator string(SceneRef property)
			{
				return property.IsSceneRefValid() ? property.GetSceneName() : "None";
			}

			public bool IsSceneLoaded()
			{
				Scene scene = GetScene();
				return scene.IsValid() && scene.isLoaded;
			}

			public Scene GetScene()
			{
				Scene scene = new Scene();

				if (IsSceneRefValid())
				{
					scene = SceneManager.GetSceneByPath(GetScenePath());
				}

				return scene;
			}

			public string GetSceneName()
			{
				return SceneUtils.GetSceneNameFromPath(GetScenePath());
			}

			public string GetScenePath()
			{
				return _scenePath;
			}

			public bool IsSceneRefValid()
			{
				return !string.IsNullOrEmpty(_scenePath);
			}

#if UNITY_EDITOR
			public void OpenSceneInEditor()
			{
				if (_sceneAsset != null)
				{
					EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(_sceneAsset), OpenSceneMode.Additive);
				}
			}
#endif
		}
	}
}