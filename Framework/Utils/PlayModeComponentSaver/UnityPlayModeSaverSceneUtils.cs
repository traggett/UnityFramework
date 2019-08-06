using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	namespace Utils
	{
		public class UnityPlayModeSaverSceneUtils : MonoBehaviour
		{
			#region Public Data
			[System.Serializable]
			public class PrefabInstance
			{
				public GameObject _gameObject;
				public string _prefab;
			}

			[HideInInspector]
			public PrefabInstance[] _scenePrefabInstances;
			#endregion

#if UNITY_EDITOR
			#region Public Interface
			public static void CacheScenePrefabInstances(Scene scene)
			{
				UnityPlayModeSaverSceneUtils prefabIndexer = GetPrefabIndexer(scene);

				if (prefabIndexer == null)
				{
					GameObject gameObject = new GameObject("Prefab Indexer", typeof(UnityPlayModeSaverSceneUtils))
					{hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable
						
					};
					SceneManager.MoveGameObjectToScene(gameObject, scene);
					prefabIndexer = gameObject.GetComponent<UnityPlayModeSaverSceneUtils>();
				}

				prefabIndexer.BuildScenePrefabMap();
			}

			public static bool IsScenePrefabInstance(Object obj, Scene scene, out GameObject prefab, out int id)
			{
				UnityPlayModeSaverSceneUtils prefabIndexer = GetPrefabIndexer(scene);

				if (prefabIndexer != null)
				{
					return prefabIndexer.IsScenePrefabInstance(obj, out prefab, out id);
				}

				prefab = null;
				id = -1;
				return false;
			}

			public static GameObject GetScenePrefabInstance(Scene scene, int id)
			{
				UnityPlayModeSaverSceneUtils prefabIndexer = GetPrefabIndexer(scene);

				if (prefabIndexer != null)
				{
					return prefabIndexer.GetScenePrefabInstance(id);
				}

				return null;
			}
			#endregion

			#region Private Functions
			private void BuildScenePrefabMap()
			{
				Scene scene = this.gameObject.scene;

				List<PrefabInstance> prefabInstances = new List<PrefabInstance>();

				foreach (GameObject rootObject in scene.GetRootGameObjects())
				{
					CheckGameObjectForPrefabs(rootObject, null, ref prefabInstances);
				}

				_scenePrefabInstances = prefabInstances.ToArray();
			}

			private bool IsScenePrefabInstance(Object obj, out GameObject prefab, out int id)
			{
				Component component = obj as Component;
				GameObject gameObject = obj as GameObject;

				for (int i = 0; i < _scenePrefabInstances.Length; i++)
				{
					//Check scene prefab still exists
					if (_scenePrefabInstances[i]._gameObject != null)
					{
						if (component != null)
						{
							if (CheckForComponent(_scenePrefabInstances[i]._gameObject, component))
							{
								prefab = _scenePrefabInstances[i]._gameObject;
								id = i;
								return true;
							}
						}
						else if (gameObject != null)
						{
							if (CheckForGameObject(_scenePrefabInstances[i]._gameObject, gameObject))
							{
								prefab = _scenePrefabInstances[i]._gameObject;
								id = i;
								return true;
							}
						}
					}
				}

				prefab = null;
				id = -1;
				return false;
			}

			private void CheckGameObjectForPrefabs(GameObject gameObject, GameObject parentPrefabRoot, ref List<PrefabInstance> prefabInstances)
			{
				GameObject prefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(gameObject);

				if (prefabRoot != null && prefabRoot != parentPrefabRoot)
				{
					PrefabInstance prefabInstance = new PrefabInstance
					{
						_gameObject = prefabRoot,
						_prefab = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefabRoot)
					};
					prefabInstances.Add(prefabInstance);
					parentPrefabRoot = prefabRoot;
				}

				foreach (Transform child in gameObject.transform)
				{
					CheckGameObjectForPrefabs(child.gameObject, parentPrefabRoot, ref prefabInstances);
				}
			}

			private bool CheckForComponent(GameObject prefabGameObject, Component component)
			{
				Component[] components = prefabGameObject.GetComponents<Component>();

				for (int i = 0; i < components.Length; i++)
				{
					if (components[i] == component)
					{
						return true;
					}
				}

				foreach (Transform child in prefabGameObject.transform)
				{
					if (CheckForComponent(child.gameObject, component))
						return true;
				}

				return false;
			}

			private bool CheckForGameObject(GameObject prefabGameObject, GameObject gameObject)
			{
				if (prefabGameObject == gameObject)
					return true;

				foreach (Transform child in prefabGameObject.transform)
				{
					if (CheckForGameObject(child.gameObject, gameObject))
						return true;
				}

				return false;
			}

			private GameObject GetScenePrefabInstance(int id)
			{
				if (id < 0 || id >= _scenePrefabInstances.Length)
					return null;

				return _scenePrefabInstances[id]._gameObject;
			}

			private static UnityPlayModeSaverSceneUtils GetPrefabIndexer(Scene scene)
			{
				foreach (GameObject rootObject in scene.GetRootGameObjects())
				{
					UnityPlayModeSaverSceneUtils prefabIndexer = rootObject.GetComponentInChildren<UnityPlayModeSaverSceneUtils>();

					if (prefabIndexer != null)
						return prefabIndexer;
				}
				
				return null;
			}
			#endregion
#endif
		}
	}
}
