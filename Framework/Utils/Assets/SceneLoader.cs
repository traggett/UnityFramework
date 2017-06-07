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
				Scene scene = sceneInfo._scene.GetScene();

				if (!scene.IsValid() || !scene.isLoaded)
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
					
					if (!scene.IsValid())
						scene = sceneInfo._scene.GetScene();

					//Still have to wait until scene is loaded grr..
					while (!scene.isLoaded)
					{
						yield return null;
					}
				}

				LoadAdditonalAssets(sceneInfo);

				yield break;
			}

			public static IEnumerator LoadSceneNonAsync(SceneLoadingInfo sceneInfo)
			{
				Scene scene = sceneInfo._scene.GetScene();

				if (!scene.IsValid() || !scene.isLoaded)
				{
					try
					{
						SceneManager.LoadScene(sceneInfo._scene.GetSceneName(), LoadSceneMode.Additive);
					}
					catch
					{
						yield break;
					}
				}

				if (!scene.IsValid())
					scene = sceneInfo._scene.GetScene();

				//Still have to wait until scene is loaded grr..
				while (!scene.isLoaded)
				{
					yield return null;
				}

				LoadAdditonalAssets(sceneInfo);

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
