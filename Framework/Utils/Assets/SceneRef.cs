 using System;

using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
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
			private string _scenePath;
			#endregion

			public SceneRef(string scenePath)
			{
				_scenePath = scenePath;
			}

			public SceneRef(Scene scene)
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

			public static implicit operator string(SceneRef property)
			{
				return property.IsSceneRefValid() ? SceneUtils.GetSceneNameFromPath(property._scenePath) : "No Scene";
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
					scene = SceneManager.GetSceneByPath(_scenePath);
				}

				return scene;
			}

			public string GetSceneName()
			{
				return SceneUtils.GetSceneNameFromPath(_scenePath);
			}

			public bool IsSceneRefValid()
			{
				return !string.IsNullOrEmpty(_scenePath);
			}

#if UNITY_EDITOR
			public string GetScenePath()
			{
				return _scenePath;
			}

			public void OpenSceneInEditor()
			{
				if (IsSceneRefValid())
				{
					EditorSceneManager.OpenScene(_scenePath, OpenSceneMode.Additive);
				}
			}
#endif
		}
	}
}