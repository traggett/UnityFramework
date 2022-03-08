using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{
	namespace Utils
	{
		public class PrefabInstancePool : MonoBehaviour
		{
			#region Helper Component
			private class PooledPrefab : MonoBehaviour
			{
				public PrefabInstancePool _parentPool;
				public bool _isFree;
			}
			#endregion

			#region Public Data
			public GameObject _prefab;
			public int _initialPoolSize;
			public int _growAmount;
			#endregion

			#region Private Data
			private PooledPrefab[] _instances;
			private List<int> _toDestroy = new List<int>();
			#endregion

			#region Unity Messages
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
			#endregion

			#region Public Interface
			public T Instantiate<T>(Transform parent = null, bool resetTransform = true) where T : Component
			{
				GameObject gameObject = Instantiate(parent, resetTransform);

				if (gameObject != null)
				{
					return gameObject.GetComponent<T>();
				}

				return null;
			}

			public GameObject Instantiate(Transform parent = null, bool resetTransform = true)
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
					if (resetTransform)
					{
						newInstance.transform.localPosition = _prefab.transform.localPosition;
						newInstance.transform.localRotation = _prefab.transform.localRotation;
						newInstance.transform.localScale = _prefab.transform.localScale;

						if (newInstance.transform is RectTransform rectTransform && _prefab.transform is RectTransform prefabRectTransform)
						{
							rectTransform.anchoredPosition = prefabRectTransform.anchoredPosition;
							rectTransform.anchorMin = prefabRectTransform.anchorMin;
							rectTransform.anchorMax = prefabRectTransform.anchorMax;
							rectTransform.sizeDelta = prefabRectTransform.sizeDelta;
							rectTransform.pivot = prefabRectTransform.pivot;
						}
					}
					
					newInstance.transform.SetParent(parent, false);
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

			public bool Destroy(Component component, bool instant = true)
			{
				int index = GetPrefabIndex(component.gameObject);
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

			public void DestroyAll(bool instant = true)
			{
				if (_instances != null)
				{
					for (int i = 0; i < _instances.Length; i++)
					{
						if (instant)
						{
							Destroy(i);
						}
						else
						{
							_toDestroy.Add(i);
							_instances[i].gameObject.SetActive(false);
						}
					}
				}
			}

			public static void InitAllPrefabInstancePools()
			{
				for (int i = 0; i < SceneManager.sceneCount; i++)
				{
					Scene scene = SceneManager.GetSceneAt(i);

					PrefabInstancePool[] prefabPools = SceneUtils.FindAllComponentInferfacesInScene<PrefabInstancePool>(scene, true);

					for (int j = 0; j < prefabPools.Length; j++)
					{
						prefabPools[j].Init();
					}
				}
			}

			public static bool DestroyPrefab(Component component, bool instant = true)
			{
				PooledPrefab pooledPrefab = component.GetComponent<PooledPrefab>();

				if (pooledPrefab != null)
				{
					return pooledPrefab._parentPool.Destroy(pooledPrefab.gameObject, instant);
				}

				return false;
			}

			public static bool DestroyPrefab(GameObject gameObject, bool instant = true)
			{
				PooledPrefab pooledPrefab = gameObject.GetComponent<PooledPrefab>();

				if (pooledPrefab != null)
				{
					return pooledPrefab._parentPool.Destroy(gameObject, instant);
				}

				return false;
			}
			#endregion

			#region Private Functions
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
#if DEBUG
				if (_prefab == null)
				{
					UnityEngine.Debug.LogError("PrefabInstancePool " + GameObjectUtils.GetGameObjectPath(this.gameObject) + " is missing it's prefab!");
					return null;
				}					
#endif
				GameObject gameObject = Instantiate(_prefab, this.transform);
				PooledPrefab prefab = gameObject.AddComponent<PooledPrefab>();
				prefab._parentPool = this;
				prefab._isFree = true;
				prefab.hideFlags = hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;

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
			#endregion
		}
	}
}