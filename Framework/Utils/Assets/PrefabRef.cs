using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	namespace Utils
	{
		[Serializable]
		public struct PrefabRef
		{
			[SerializeField]
			private string _filePath;
			[SerializeField]
			private string _fileGUID;

			public GameObject LoadAndInstantiatePrefab()
			{
				GameObject prefab = null;

				string resourcePath = StringUtils.GetResourcePath(_filePath);
				GameObject prefabSourceObject = Resources.Load<GameObject>(resourcePath) as GameObject;

				if (prefabSourceObject != null)
				{
#if UNITY_EDITOR
					prefab = PrefabUtility.InstantiatePrefab(prefabSourceObject) as GameObject;
#else

					prefab = GameObjectUtils.SafeInstantiate(prefabSourceObject);
#endif
					prefab.name = prefabSourceObject.name;
				}

				return prefab;
			}

			public GameObject LoadPrefab()
			{
				string resourcePath = StringUtils.GetResourcePath(_filePath);
				return Resources.Load<GameObject>(resourcePath) as GameObject; 
			}

			public GameObject InstantiatePrefab(GameObject prefabObject)
			{
				GameObject prefabInstance = null;

				if (prefabObject != null)
				{
#if UNITY_EDITOR
					prefabInstance = PrefabUtility.InstantiatePrefab(prefabObject) as GameObject;
#else

					prefabInstance = GameObjectUtils.SafeInstantiate(prefabObject);
#endif
					prefabInstance.name = prefabObject.name;
				}

				return prefabInstance;
			}
		}
	}
}
