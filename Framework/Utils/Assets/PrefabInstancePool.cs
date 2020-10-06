using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{
	namespace Utils
	{
		public class PrefabInstancePool : MonoBehaviour
		{
			public GameObject _prefab;
			public int _initialPoolSize;
			public int _growAmount;

			private PooledPrefab[] _instances;
			private List<int> _toDestroy = new List<int>();

			private void Update()
			{
				if (_instances != null)
				{
					foreach (int index in _toDestroy)
					{
						Destroy(index);
					}

					_toDestroy.Clear();
				}
			}

			public GameObject Instantiate(Transform parent = null)
			{
				Init();

				GameObject newInstance = null;

				for (int i = 0; i < _instances.Length; i++)
				{
					if (_instances[i]._isFree)
					{
						newInstance = _instances[i].gameObject;
						_instances[i]._isFree = false;
						break;
					}
				}

				if (newInstance == null)
				{
					PooledPrefab[] newItems = new PooledPrefab[Mathf.Max(1, _growAmount)];
					for (int i = 0; i < newItems.Length; i++)
					{
						newItems[i] = CreatePrefab();
						newItems[i]._isFree = true;
					}
					ArrayUtils.Concat(ref _instances, newItems);

					newInstance = newItems[0].gameObject;
					newItems[0]._isFree = false;
				}

				if (parent != null && parent != newInstance.transform.parent)
				{
					newInstance.transform.SetParent(parent, false);
					newInstance.transform.localPosition = _prefab.transform.localPosition;
					newInstance.transform.localRotation = _prefab.transform.localRotation;
					newInstance.transform.localScale = _prefab.transform.localScale;
				}

				newInstance.SetActive(true);

				return newInstance;
			}

			public bool Destroy(GameObject gameObject, bool instant = true)
			{
				int index = GetPrefabIndex(gameObject);
				if (index != -1)
				{
					if (instant)
					{
						Destroy(index);
					}
					else
					{
						_toDestroy.Add(index);
						gameObject.SetActive(false);
					}
					
					return true;
				}

				return false;
			}

			private void Destroy(int index)
			{
				_instances[index]._isFree = true;

				GameObject gameObject = _instances[index].gameObject;
				gameObject.SetActive(false);

				if (gameObject.transform.parent != this.transform)
				{
					gameObject.transform.SetParent(this.transform, false);
				}
			}

			public static void InitAllPrefabInstancePools()
			{
				for (int i = 0; i < SceneManager.sceneCount; i++)
				{
					Scene scene = SceneManager.GetSceneAt(i);

					PrefabInstancePool[] prefabPools = SceneUtils.FindAllComponentInferfacesInScene<PrefabInstancePool>(scene);

					for (int j = 0; j < prefabPools.Length; j++)
					{
						prefabPools[j].Init();
					}
				}
			}

			public static void DestroyChildPrefabs(Transform parent, bool instant = true)
			{
				for (int i = 0; i < parent.childCount; i++)
				{
					PooledPrefab prefab = parent.GetChild(i).GetComponent<PooledPrefab>();

					if (prefab != null)
					{
						prefab._parentPool.Destroy(prefab.gameObject, instant);
					}
				}
			}

			private void Init()
			{
				if (_instances == null)
				{
					_instances = new PooledPrefab[_initialPoolSize];

					for (int i = 0; i < _instances.Length; i++)
					{
						_instances[i] = CreatePrefab();
					}
				}
			}

			private PooledPrefab CreatePrefab()
			{
				GameObject gameObject = Instantiate(_prefab, this.transform);
				PooledPrefab prefab = gameObject.AddComponent<PooledPrefab>();
				prefab._parentPool = this;
				prefab._isFree = true;
				gameObject.SetActive(false);
				return prefab;
			}

			private int GetPrefabIndex(GameObject gameObject)
			{
				for (int i = 0; i < _instances.Length; i++)
				{
					if (_instances[i].gameObject == gameObject)
					{
						return i;
					}
				}

				return -1;
			}
		}
	}
}