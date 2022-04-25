using Framework.Serialization;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{

	namespace Utils
	{
		[Serializable]
		public struct GameObjectRef
		{
			public enum SourceType
			{
				Scene,
				Relative,
			}

			#region Serialized Data
			[SerializeField]
			private SourceType _sourceType;
			[SerializeField]
			private string _objectName;
			[SerializeField]
			private SceneRef _scene;
			[SerializeField]
			private int _sceneObjectID;
			[SerializeField]
			private string _relativeObjectPath;
			#endregion

			#region Private Data
			private GameObject _gameObject;
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

			public SourceType GetSourceType()
			{
				return _sourceType;
			}

			public GameObject GetGameObject()
			{
				if (_gameObject == null)
				{
					switch (_sourceType)
					{
						case SourceType.Scene:
							_gameObject = GetSceneObject(_scene.GetScene());
							break;
						case SourceType.Relative:
							_gameObject = null;
							break;
					}
				}

				return _gameObject;
			}

			public bool IsValid()
			{
				switch (_sourceType)
				{
					case SourceType.Relative:
						return !string.IsNullOrEmpty(_relativeObjectPath);
					case SourceType.Scene:
					default:
						return _sceneObjectID != -1 && _scene.IsSceneRefValid();
				}
			}

			public static object FixUpGameObjectRefs(object obj, GameObject rootObject)
			{
				return Serializer.UpdateChildObjects(obj, FixUpGameObjectRef, rootObject);
			}

#if UNITY_EDITOR
			public GameObjectRef(SourceType sourceType)
			{
				_sourceType = sourceType;
				_objectName = string.Empty;
				_scene = new SceneRef();
				_sceneObjectID = -1;
				_relativeObjectPath = string.Empty;
				_gameObject = null;
				_editorCollapsed = false;
			}

			public GameObjectRef(SourceType sourceType, GameObject gameObject)
			{
				_sourceType = sourceType;
				_editorCollapsed = false;
				_relativeObjectPath = string.Empty;
				_scene = new SceneRef();
				_gameObject = gameObject;

				switch (sourceType)
				{
					case SourceType.Relative:
						{
							_objectName = null;
							_sceneObjectID = -1;

							//TO DO - find realtive path to serilaisation root
						}
						break;
					default:
					case SourceType.Scene:
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

			public SceneRef GetSceneRef()
			{
				return _scene;
			}
#endif
			#endregion

			#region Private Functions
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

			private static object FixUpGameObjectRef(object obj, object rootObject)
			{
				if (obj.GetType() == typeof(GameObjectRef))
				{
					GameObjectRef gameObjectRef = (GameObjectRef)obj;

					//Tell relativge pathed objects what root is

					if (gameObjectRef._sourceType == SourceType.Relative)
					{
						gameObjectRef._relativeObjectPath = FindRelativePath(gameObjectRef._gameObject, (GameObject)rootObject);
					}

					return gameObjectRef;
				}

				return obj;
			}

			private static string FindRelativePath(GameObject gameObject, GameObject rootObject)
			{
				return null;
			}
			#endregion
		}
	}
}