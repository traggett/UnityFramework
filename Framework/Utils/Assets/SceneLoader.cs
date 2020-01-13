using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;

namespace Framework
{
	namespace Utils
	{
		public static class SceneLoader
		{
			#region Public Interface
			public static IEnumerator LoadScene(SceneLoadingInfo sceneInfo)
			{
				if (!sceneInfo._scene.IsSceneLoaded())
				{
					AsyncOperation asyncOp;

					try
					{
						asyncOp = SceneManager.LoadSceneAsync(sceneInfo._scene.GetSceneName(), LoadSceneMode.Additive);
					}
					catch
					{
						yield break;
					}

					while (!asyncOp.isDone)
					{
						yield return null;
					}

					//Still have to wait until scene is loaded grr..
					while (!sceneInfo._scene.IsSceneLoaded())
					{
						yield return null;
					}
				}

				LoadAdditonalAssets(sceneInfo);

				yield break;
			}

			public static IEnumerator LoadSceneNonAsync(SceneLoadingInfo sceneInfo)
			{
				if (!sceneInfo._scene.IsSceneLoaded())
				{
					try
					{
						SceneManager.LoadScene(sceneInfo._scene.GetSceneName(), LoadSceneMode.Additive);
					}
					catch
					{
						yield break;
					}

					//Still have to wait until scene is loaded grr..
					while (!sceneInfo._scene.IsSceneLoaded())
					{
						yield return null;
					}

					LoadAdditonalAssets(sceneInfo);
				}

				yield break;
			}

			public static void UnloadScene(string sceneName)
			{
				SceneManager.UnloadSceneAsync(sceneName);
			}

			public static void UnloadScene(Scene scene)
			{
				UnloadScene(scene.name);
			}
			#endregion

			#region Private Functions
			private static void LoadAdditonalAssets(SceneLoadingInfo sceneInfo)
			{
				Scene scene = sceneInfo._scene.GetScene();

				if (sceneInfo._additionalLoadingObjects != null)
				{
					foreach (GameObjectLoader.Ref loader in sceneInfo._additionalLoadingObjects)
					{
						GameObjectLoader.Load(scene, loader);
					}
				}
			}
			#endregion
		}
	}
}
