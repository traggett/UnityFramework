using System;

using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using Serialization;

	namespace Utils
	{
		[Serializable]
		public struct GameObjectRef
		{
			#region Public Data
			public enum eSourceType
			{
				Scene,
				Prefab,
				Loaded
			}
			[SerializeField]
			private eSourceType _sourceType;
			[SerializeField]
			private string _objectName;
			[SerializeField]
			private SceneRef _scene;
			[SerializeField]
			private int _sceneObjectID;
			[SerializeField]
			private AssetRef<GameObject> _prefab;
			#endregion

			#region Private Data
			private GameObject _sourceObject;
			#endregion

			#region Editor Data
#if UNITY_EDITOR
			[NonSerialized]
			public bool _editorCollapsed;
#endif
			#endregion

			#region Public Interface
			public static implicit operator string(GameObjectRef property)
			{
				if (!string.IsNullOrEmpty(property._objectName))
				{
					return System.IO.Path.GetFileNameWithoutExtension(property._objectName);
				}
				else
				{
					return "GameObject";
				}
			}

			public static implicit operator GameObject(GameObjectRef property)
			{
				if (property != null)
					return property.GetGameObject();

				return null;
			}

			public override string ToString()
			{
				if (!string.IsNullOrEmpty(_objectName))
				{
					return _objectName;
				}

				return typeof(GameObject).Name;
			}

			public string GetGameObjectName()
			{
				return _objectName;
			}

			public eSourceType GetSourceType()
			{
				return _sourceType;
			}

			public GameObject GetGameObject()
			{
				if (_scene != null)
				{
					switch (_sourceType)
					{
						case eSourceType.Scene:
							return GetSceneObject(_scene.GetScene());
						case eSourceType.Prefab:
							return GetPrefabObject();
						case eSourceType.Loaded:
							return GetLoadedObject(_scene.GetScene());
					}
				}

				return null;
			}

			public bool IsValid()
			{
				switch (_sourceType)
				{
					case eSourceType.Prefab:
						return _prefab.IsValid() && !string.IsNullOrEmpty(_objectName);
					case eSourceType.Loaded:
						return !string.IsNullOrEmpty(_objectName);
					case eSourceType.Scene:
					default:
						return _sceneObjectID != -1 && _scene.IsSceneRefValid();
				}
			}

			public static object FixUpGameObjectRefsInObject(object obj, GameObject gameObject)
			{
				return Serializer.UpdateChildObjects(obj, FixUpGameObjectRef, gameObject);
			}

#if UNITY_EDITOR
			public GameObjectRef(eSourceType sourceType)
			{
				_sourceType = sourceType;
				_objectName = string.Empty;
				_scene = new SceneRef();
				_sceneObjectID = -1;
				_prefab = new AssetRef<GameObject>();
				_sourceObject = null;
				_editorCollapsed = false;
			}

			public GameObjectRef(eSourceType sourceType, GameObject gameObject)
			{
				_sourceType = sourceType;
				_sourceObject = null;
				_editorCollapsed = false;
				_prefab = new AssetRef<GameObject>();
				_scene = new SceneRef();

				switch (sourceType)
				{
					case eSourceType.Prefab:
						{
							GameObject prefabAsset = (GameObject)PrefabUtility.GetPrefabParent(gameObject);

							if (prefabAsset != null)
								gameObject = prefabAsset;

							//Then find its root
							GameObject prefabRoot = PrefabUtility.FindPrefabRoot(gameObject);

							if (prefabRoot != null)
							{
								_objectName = GameObjectUtils.GetChildFullName(gameObject, prefabRoot);
								_prefab = new AssetRef<GameObject>(prefabRoot);
							}
							else
							{
								_objectName = string.Empty;
							}

							_sceneObjectID = -1;
						}
						break;
					case eSourceType.Loaded:
						{
							GameObjectLoader loader = gameObject.GetComponentInParent<GameObjectLoader>();

							if (loader != null)
							{
								_scene = new SceneRef(loader.gameObject.scene);
								_sceneObjectID = SceneIndexer.GetIdentifier(loader.gameObject);

								if (gameObject != null && GameObjectUtils.IsChildOf(gameObject.transform, loader.transform))
								{
									_objectName = GameObjectUtils.GetChildFullName(gameObject, loader.gameObject);
								}
								else
								{
									_objectName = null;
								}
							}
							else
							{
								_objectName = null;
								_sceneObjectID = -1;
							}
						}
						break;
					default:
					case eSourceType.Scene:
						{
							if (gameObject != null && gameObject.scene.IsValid())
							{
								_scene = new SceneRef(gameObject.scene);
								_objectName = gameObject.name;
								_sceneObjectID = SceneIndexer.GetIdentifier(gameObject);
							}
							else
							{
								_objectName = string.Empty;
								_sceneObjectID = -1;
							}
						}
						break;

				}
			}

			public GameObjectLoader GetEditorGameObjectLoader(Scene scene)
			{
				return GetGameObjectLoader(scene);
			}

			public SceneRef GetSceneRef()
			{
				return _scene;
			}
#endif
			#endregion

			#region Private Functions
			private static object FixUpGameObjectRef(object obj, object gameObject)
			{
				if (obj.GetType() == typeof(GameObjectRef))
				{
					GameObjectRef gameObjectRef = (GameObjectRef)obj;
					gameObjectRef._sourceObject = (GameObject)gameObject;
					return gameObjectRef;
				}

				return obj;
			}

			private GameObject GetSceneObject(Scene scene)
			{
				GameObject gameObject = null;

				if (scene.IsValid() && scene.isLoaded)
				{
					GameObject obj = SceneIndexer.GetObject(scene, _sceneObjectID);

					if (obj != null)
					{
						gameObject = obj;
						_objectName = obj.name;
					}
				}

				return gameObject;
			}

			private GameObject GetPrefabObject()
			{
				GameObject gameObject = null;
				GameObject sourceObject = _sourceObject;

#if UNITY_EDITOR
				if (_sourceObject == null)
				{
					//In the editor find asset from the editor prefab field 
					sourceObject = _prefab._editorAsset;
				}
#endif
				GameObject prefabObject = PrefabRoot.GetPrefabRoot(sourceObject);

				if (prefabObject != null && !string.IsNullOrEmpty(_objectName))
				{
					if (prefabObject.name == _objectName || prefabObject.name == _objectName + "(Clone)")
					{
						gameObject = prefabObject;
					}
					else
					{
						Transform child = prefabObject.transform.Find(_objectName);
						if (child != null)
						{
							gameObject = child.gameObject;
						}
					}
				}

				return gameObject;
			}

			private GameObject GetLoadedObject(Scene scene)
			{
				GameObject gameObject = null;

				if (scene.IsValid() && scene.isLoaded)
				{
					GameObjectLoader loader = GetGameObjectLoader(scene);
					
					if (loader != null && loader.IsLoaded())
					{
						Transform child = loader.transform.Find(_objectName);
						if (child != null)
						{
							gameObject = child.gameObject;
						}
					}
				}

				return gameObject;
			}

			private GameObjectLoader GetGameObjectLoader(Scene scene)
			{
				GameObjectLoader loader = null;

				if (_sourceType == eSourceType.Loaded)
				{
					if (scene.IsValid() && scene.isLoaded)
					{
						GameObject obj = SceneIndexer.GetObject(scene, _sceneObjectID);
						if (obj != null)
						{
							loader = obj.GetComponent<GameObjectLoader>();
						}
					}
				}

				return loader;
			}
			#endregion
		}
	}
}