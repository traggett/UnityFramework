using System;
using UnityEngine;
using System.Collections;

//Disable private SerializedField warnings
#pragma warning disable 0649

namespace Framework
{
	namespace Utils
	{
		[Serializable]
		public struct PrefabResourceRef
		{
			#region Private Data
			[SerializeField]
			private string _filePath;
			[SerializeField]
			private string _fileGUID;
			#endregion

			public GameObject LoadAndInstantiatePrefab(Transform parent = null)
			{
				GameObject prefab = null;

				string resourcePath = AssetUtils.GetResourcePath(_filePath);
				GameObject prefabSourceObject = Resources.Load<GameObject>(resourcePath) as GameObject;
                
                if (prefabSourceObject != null)
				{
					prefab = GameObjectUtils.SafeInstantiate(prefabSourceObject, parent);
					prefab.name = prefabSourceObject.name;
				}

				return prefab;
			}

            public IEnumerator AsyncLoadAndInstantiatePrefab(Action<GameObject> onInstantiatedPrefab = null, Transform parent = null)
            {
                string resourcePath = AssetUtils.GetResourcePath(_filePath);
                ResourceRequest request = Resources.LoadAsync<GameObject>(resourcePath);

                while (!request.isDone)
                {
					yield return null;
				}

                GameObject prefabSourceObject = (GameObject)request.asset;

                if (prefabSourceObject != null)
                {
                    GameObject prefab = GameObjectUtils.SafeInstantiate(prefabSourceObject, parent);
                    prefab.name = prefabSourceObject.name;

                    onInstantiatedPrefab?.Invoke(prefab);
                }
            }

            public GameObject LoadPrefabResource()
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
