using System;
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
			public string _scenePath;
			#endregion

			public static implicit operator string(SceneRef property)
			{
				return property.IsSceneRefValid() ? SceneUtils.GetSceneNameFromPath(property._scenePath) : "No Scene";
			}

			public Scene GetScene()
			{
				Scene scene = new Scene();

				if (IsSceneRefValid())
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
				return SceneUtils.GetSceneNameFromPath(_scenePath);
			}

			public bool IsSceneRefValid()
			{
				return !string.IsNullOrEmpty(_scenePath);
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
				if (IsSceneRefValid())
				{
					EditorSceneManager.OpenScene(_scenePath, OpenSceneMode.Additive);
				}
			}
#endif
		}
	}
}