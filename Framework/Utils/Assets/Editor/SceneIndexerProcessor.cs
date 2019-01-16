using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			public class SceneIndexerProcessor : UnityEditor.AssetModificationProcessor
			{
				public static string[] OnWillSaveAssets(string[] paths)
				{
					List<string> sceneNames = new List<string>();

					foreach (string path in paths)
					{
						if (path.EndsWith(".unity"))
						{
							sceneNames.Add(System.IO.Path.GetFileNameWithoutExtension(path));
						}
					}

					foreach (string sceneName in sceneNames)
					{
						Scene scene = SceneManager.GetSceneByName(sceneName);

						if (scene.IsValid())
						{
							UpdateSceneIndex(scene);
						}
					}

					return paths;
				}


				private static void UpdateSceneIndex(Scene scene)
				{
					//Find SceneIndexer and update its caches list
					SceneIndexer indexer = SceneUtils.FindInScene<SceneIndexer>(scene);

					//Create a new one if one doesn't exist
					if (indexer == null)
					{
						GameObject gameObject = new GameObject("Scene Indexer", typeof(SceneIndexer))
						{
							hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable

						};

						SceneManager.MoveGameObjectToScene(gameObject, scene);
						indexer = gameObject.GetComponent<SceneIndexer>();
					}

					indexer.CacheSceneObjects();
				}
			}
		}
	}
}
