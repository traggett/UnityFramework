using System;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	namespace Utils
	{
		public class GameObjectLoader : MonoBehaviour
		{
			[Serializable]
			public class Ref
			{
				
				public string _loaderName;
			}

			public string _filePath;
			public string _fileGUID;

			private GameObject _prefab;
			private GameObject _gameObject;
			
			public GameObject GetLoadedObject()
			{
				return _gameObject;
			}

			public bool IsLoaded()
			{
				return _gameObject != null;
			}

			public void Load()
			{
				string resourcePath = StringUtils.GetResourcePath(_filePath);
				_prefab = Resources.Load<GameObject>(resourcePath) as GameObject;

				if (_prefab != null)
				{
#if UNITY_EDITOR
					_gameObject = PrefabUtility.InstantiatePrefab(_prefab) as GameObject;
#else

					_gameObject = GameObjectUtils.SafeInstantiate(_prefab);
#endif

					if (_gameObject != null)
					{
						_gameObject.transform.parent = this.transform;
						_gameObject.name = _prefab.name;
					}
				}
			}

			public void Unload()
			{
				GameObjectUtils.DeleteChildren(this.transform, true);
			}

			public static void Load(Scene scene, Ref loaderRef)
			{
				GameObjectLoader loader = SceneUtils.FindInScene<GameObjectLoader>(scene, loaderRef._loaderName);

				if (loader != null)
				{
					loader.Load();
				}
			}
		}
	}
}
