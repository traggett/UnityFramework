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
		public struct PrefabResourceRef
		{
			[SerializeField]
			private string _filePath;
			[SerializeField]
			private string _fileGUID;

			public GameObject LoadAndInstantiatePrefab(Transform parent = null)
			{
				GameObject prefab = null;

				string resourcePath = AssetUtils.GetResourcePath(_filePath);
				GameObject prefabSourceObject = Resources.Load<GameObject>(resourcePath) as GameObject;

				if (prefabSourceObject != null)
				{
#if UNITY_EDITOR
					prefab = (GameObject)PrefabUtility.InstantiatePrefab(prefabSourceObject);
#else
					prefab = GameObjectUtils.SafeInstantiate(prefabSourceObject, parent);
#endif
					prefab.name = prefabSourceObject.name;
				}

				return prefab;
			}

			public GameObject LoadPrefab()
			{
				string resourcePath = AssetUtils.GetResourcePath(_filePath);
				return Resources.Load<GameObject>(resourcePath) as GameObject; 
			}

			public string GetGUID()
			{
				return _fileGUID;
			}
		}
	}
}
