using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{
	namespace Utils
	{
		public class PrefabInstancePool : MonoBehaviour
		{
			#region Serialised Data
			[SerializeField] private GameObject _prefab;
			[SerializeField] private bool _initialiseOnAwake;
			[SerializeField] private int _initialPoolSize = 10;
			[SerializeField] private int _growAmount = 1;
			[SerializeField] private PooledPrefab[] _instances;
			#endregion

			#region Public Properties
			public GameObject Prefab
			{
				get
				{
					return _prefab;
				}
			}

			public int PoolSize
			{
				get 
				{ 
					return _instances != null ? _instances.Length : 0; 
				}
			}

			public int ActivePrefabCount
			{
				get
				{
					int count = 0;

					if (_instances != null)
					{
						for (int i = 0; i < _instances.Length; i++)
						{
							if (!_instances[i]._isFree)
							{
								count++;
							}
						}
					}
					
					return count;
				}
			}
			#endregion

			#region Helper Struct
			[Serializable]
			private struct PooledPrefab
			{
				#region Public Data
				public GameObject _prefabRoot;
				public bool _isFree;
				public bool _markedForDestroy;
				#endregion
			}
			#endregion

			#region Unity Messages
			private void Awake()
			{
				if (_initialiseOnAwake)
				{
					InitialisePool();
				}
			}

			private void Update()
			{
				if (_instances != null)
				{
					for (int i = 0; i < _instances.Length; i++)
					{
						if (_instances[i]._markedForDestroy)
						{
							DestroyAtIndex(i);
						}
					}
				}
			}
			#endregion

			#region Public Interface
			public void InitialisePool()
			{
				if (_prefab != null && _instances == null)
				{
					_instances = new PooledPrefab[Math.Max(_initialPoolSize, 1)];

					for (int i = 0; i < _instances.Length; i++)
					{
						InstantiatePrefab(i);
					}
				}
			}

			public void DeInitialisePool()
			{
				if (_instances != null)
				{
					for (int i = 0; i < _instances.Length; i++)
					{
						if (_instances[i]._prefabRoot != null)
						{
#if UNITY_EDITOR
							if (!Application.isPlaying)
							{
								DestroyImmediate(_instances[i]._prefabRoot);
							}
							else
#endif
							{
								UnityEngine.Object.Destroy(_instances[i]._prefabRoot);
							}
						}
					}

					_instances = null;
				}
			}

			public static void InitialiseAllPrefabInstancePools()
			{
				for (int i = 0; i < SceneManager.sceneCount; i++)
				{
					Scene scene = SceneManager.GetSceneAt(i);
					InitialiseScenePrefabInstancePools(scene);
				}
			}

			public static void InitialiseScenePrefabInstancePools(Scene scene)
			{
				PrefabInstancePool[] prefabPools = SceneUtils.FindAllComponentInferfacesInScene<PrefabInstancePool>(scene, true);

				for (int j = 0; j < prefabPools.Length; j++)
				{
					prefabPools[j].InitialisePool();
				}
			}

			public T Instantiate<T>(Transform parent = null, bool resetTransform = true) where T : Component
			{
				GameObject gameObject = Instantiate(parent, resetTransform);

				if (gameObject != null)
				{
					T component = gameObject.GetComponent<T>();

					if (component != null)
					{
						return component;
					}
					else
					{
#if DEBUG
						UnityEngine.Debug.LogError("PrefabInstancePool " + GameObjectUtils.GetGameObjectPath(this.gameObject) + " prefab has no component of type " + typeof(T).Name);
#endif
						Destroy(gameObject);
					}
				}

				return null;
			}

			public GameObject Instantiate(Transform parent = null, bool resetTransform = true)
			{
				InitialisePool();

				GameObject newInstance = null;

				for (int i = 0; i < _instances.Length; i++)
				{
					if (_instances[i]._isFree)
					{
						if (_instances[i]._prefabRoot == null)
							InstantiatePrefab(i);

						newInstance = _instances[i]._prefabRoot;
						_instances[i]._isFree = false;
						break;
					}
				}

				if (newInstance == null)
				{
					int origSize = _instances.Length;
					int growAmount = Math.Max(1, _growAmount);

					ArrayUtils.Resize(ref _instances, _instances.Length + growAmount);

					for (int i = origSize; i < _instances.Length; i++)
					{
						InstantiatePrefab(i);
					}
					
					//Return first new instance
					newInstance = _instances[origSize]._prefabRoot;
					_instances[origSize]._isFree = false;
				}

				if (parent == null)
				{
					parent = this.transform;
				}

				if (newInstance.transform.parent != parent)
				{
					newInstance.transform.SetParent(parent, false);
				}

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

				newInstance.hideFlags = HideFlags.None;
				newInstance.SetActive(true);

				return newInstance;
			}

			private int GetPrefabInstanceIndex(GameObject prefab)
			{
				for (int i = 0; i < _instances.Length; i++)
				{
					if (_instances[i]._prefabRoot == prefab)
						return i;
				}

				return -1;
			}

			public bool Destroy(GameObject gameObject, bool instant = true)
			{
				int index = GetPrefabInstanceIndex(gameObject);

				if (index != -1)
				{
					return Destroy(index, instant);
				}

				return false;
			}

			public bool Destroy(Component component, bool instant = true)
			{
				int index = GetPrefabInstanceIndex(component.gameObject);

				if (index != -1)
				{
					return Destroy(index, instant);
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
							DestroyAtIndex(i);
						}
						else
						{
							_instances[i]._markedForDestroy = true;

							if (_instances[i]._prefabRoot != null)
								_instances[i]._prefabRoot.SetActive(false);
						}
					}
				}
			}
			#endregion

			#region Private Functions
			private void InstantiatePrefab(int index)
			{
#if DEBUG
				if (_prefab == null)
				{
					UnityEngine.Debug.LogError("PrefabInstancePool " + GameObjectUtils.GetGameObjectPath(this.gameObject) + " is missing it's prefab!");
					return;
				}
#endif

				GameObject gameObject;

#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					gameObject = PrefabUtility.InstantiatePrefab(_prefab, this.transform) as GameObject;
				}
				else
#endif
				{
					gameObject = UnityEngine.Object.Instantiate(_prefab, this.transform);
				}

				gameObject.hideFlags = HideFlags.HideInHierarchy;
				gameObject.gameObject.SetActive(false);

				_instances[index]._prefabRoot = gameObject;
				_instances[index]._isFree = true;
				_instances[index]._markedForDestroy = false;
			}

			private bool Destroy(int index, bool instant)
			{
#if UNITY_EDITOR
				// Always destroy instantly in edit mode
				if (!Application.isPlaying)
				{
					instant = true;
				}
#endif

				if (index != -1 && !_instances[index]._isFree)
				{
					if (instant)
					{
						DestroyAtIndex(index);
					}
					else
					{
						_instances[index]._markedForDestroy = true;

						if (_instances[index]._prefabRoot != null)
						{
							_instances[index]._prefabRoot.SetActive(false);
							_instances[index]._prefabRoot.hideFlags = HideFlags.HideInHierarchy;
						}
					}

					return true;
				}

				return false;
			}

			private void DestroyAtIndex(int index)
			{
				_instances[index]._isFree = true;
				_instances[index]._markedForDestroy = false;

				GameObject gameObject = _instances[index]._prefabRoot;
				
				if (gameObject != null && gameObject.transform.parent != this.transform)
				{
					gameObject.transform.SetParent(this.transform, false);
				}

				gameObject.SetActive(false);
				gameObject.hideFlags = HideFlags.HideInHierarchy;
			}
			#endregion
		}
	}
}