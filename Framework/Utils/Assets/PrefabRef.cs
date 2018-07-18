using System;
using UnityEngine;

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

			public GameObject LoadAndInstantiatePrefab(Transform parent = null)
			{
				GameObject prefab = null;

				string resourcePath = StringUtils.GetResourcePath(_filePath);
				GameObject prefabSourceObject = Resources.Load<GameObject>(resourcePath) as GameObject;

				if (prefabSourceObject != null)
				{
					prefab = GameObjectUtils.SafeInstantiate(prefabSourceObject, parent);
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
					prefabInstance = GameObjectUtils.SafeInstantiate(prefabObject);
					prefabInstance.name = prefabObject.name;
				}

				return prefabInstance;
			}

			public string GetGUID()
			{
				return _fileGUID;
			}
		}
	}
}
