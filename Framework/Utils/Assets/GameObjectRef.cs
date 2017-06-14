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
		public sealed class GameObjectRef : ISerializationCallbackReceiver, ICustomEditorInspector
		{
			public enum eSourceType
			{
				Scene,
				Prefab,
				Loaded
			}

			#region Public Data
			public eSourceType _sourceType = eSourceType.Scene;
			public string _objectName = string.Empty;
			public SceneRef _scene = new SceneRef();
			public int _sceneObjectID = -1;
			public AssetRef<GameObject> _prefab = new AssetRef<GameObject>();
			#endregion

			#region Private Data
			private GameObject _sourceObject;
#if UNITY_EDITOR
			private GameObject _editorGameObject;
			private GameObjectLoader _editorLoaderGameObject;
			private bool _editorFoldout = true;
			private bool _editorSceneLoaded = false;
			private bool _editorLoaderIsLoaded = false;
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
				GameObject gameObject = GetGameObject();

				if (gameObject != null)
				{
					return gameObject.gameObject.name;
				}

				return typeof(GameObject).Name;
			}

			public GameObject GetGameObject()
			{
				switch (_sourceType)
				{
					case eSourceType.Scene:
						return GetSceneObject(_scene.GetScene());
					case eSourceType.Prefab:
						return GetPrefabObject();
					case eSourceType.Loaded:
						return GetLoadedObject(_scene.GetScene());
					default:
						return null;
				}
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
						return _sceneObjectID != -1 && _scene.IsSceneValid();
				}
			}

			public GameObjectLoader GetGameObjectLoader(Scene scene)
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

			public static void FixupGameObjectRefs(GameObject sourceObject, object node)
			{
				if (node != null)
				{
					object[] nodeFieldObjects = SerializedFieldInfo.GetSerializedFieldInstances(node);

					foreach (object nodeFieldObject in nodeFieldObjects)
					{
						GameObjectRef gameObjectRefProperty = nodeFieldObject as GameObjectRef;

						if (gameObjectRefProperty != null)
						{
							gameObjectRefProperty._sourceObject = sourceObject;
						}

						FixupGameObjectRefs(sourceObject, nodeFieldObject);
					}
				}
			}

#if UNITY_EDITOR
			public GameObject GetEditorGameObject()
			{
				return _editorGameObject;
			}

			public void ClearGameObject()
			{
				_sceneObjectID = -1;
				_objectName = null;
				_scene.ClearScene();
				_prefab.ClearAsset();
				_editorGameObject = null;
				_editorLoaderGameObject = null;
				_editorSceneLoaded = false;
				_editorLoaderIsLoaded = false;
			}

			public void SetSceneGameObject(GameObject gameObject)
			{
				if (gameObject != null && gameObject.scene.IsValid())
				{
					_editorGameObject = gameObject;
					_scene.SetScene(gameObject.scene);
					_objectName = gameObject.name;
					_sceneObjectID = SceneIndexer.GetIdentifier(gameObject);
				}
				else
				{
					ClearGameObject();
				}
			}

			public void SetPrefabGameObject(GameObject gameObject)
			{
				GameObject prefabRoot = PrefabUtility.FindPrefabRoot(gameObject);

				UnityEngine.Object prefabObj = PrefabUtility.GetPrefabParent(gameObject);
				if (prefabObj == null)
				{
					prefabObj = PrefabUtility.GetPrefabObject(prefabRoot);
				}

				if (prefabRoot != null && prefabObj != null)
				{
					_objectName = GameObjectUtils.GetChildFullName(gameObject, prefabRoot);
					_editorGameObject = gameObject;

					string prefabPath = AssetDatabase.GetAssetPath(prefabObj);
					_prefab.SetAsset(prefabPath);
				}
				else
				{
					ClearGameObject();
				}
			}

			public void SetLoadedGameObject(GameObject gameObject)
			{
				GameObjectLoader loader = gameObject.GetComponentInParent<GameObjectLoader>();
				SetLoadedGameObject(loader, gameObject);
			}

			public void SetLoadedGameObject(GameObjectLoader loader, GameObject gameObject)
			{
				if (loader != null)
				{
					_scene.SetScene(loader.gameObject.scene);
					_sceneObjectID = SceneIndexer.GetIdentifier(loader.gameObject);
					_editorLoaderGameObject = loader;

					if (gameObject != null && GameObjectUtils.IsChildOf(gameObject.transform, loader.transform))
					{
						_objectName = GameObjectUtils.GetChildFullName(gameObject, loader.gameObject);
						_editorGameObject = gameObject;
					}
					else
					{
						_objectName = null;
						_editorGameObject = null;
					}
				}
				else
				{
					ClearGameObject();
				}
			}

			public bool RenderSceneNotLoadedField()
			{
				bool clear = false;

				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField("Scene '" + _scene + "' not loaded");

					if (GUILayout.Button("Load", GUILayout.ExpandWidth(false)))
					{
						_scene.OpenSceneInEditor();
					}

					clear = GUILayout.Button("Clear", GUILayout.ExpandWidth(false));
				}
				EditorGUILayout.EndHorizontal();

				return clear;
			}

			public bool RenderLoadedNotLoadedField(GameObjectLoader loader)
			{
				bool clear = false;

				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField("('" + loader.name + "' not loaded)");

					if (GUILayout.Button("Load", GUILayout.ExpandWidth(false)))
					{
						loader.Load();
					}

					clear = GUILayout.Button("Clear", GUILayout.ExpandWidth(false));
				}
				EditorGUILayout.EndHorizontal();

				return clear;
			}
#endif
			#endregion

			#region ISerializationCallbackReceiver
			public void OnBeforeSerialize()
			{

			}

			public void OnAfterDeserialize()
			{
#if UNITY_EDITOR
				_editorLoaderGameObject = GetGameObjectLoader(_scene.GetScene());
				_editorGameObject = GetGameObject();
#endif
			}
			#endregion

			#region ICustomEditable
#if UNITY_EDITOR
			public bool RenderObjectProperties(GUIContent label)
			{
				bool dataChanged = false;

				if (label == null)
					label = new GUIContent();

				label.text += " (" + this + ")";

				_editorFoldout = EditorGUILayout.Foldout(_editorFoldout, label);
				if (_editorFoldout)
				{
					int origIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel++;

					//Show drop down
					eSourceType prevType = _sourceType;
					_sourceType = SerializedObjectEditorGUILayout.ObjectField(_sourceType, "Source Type", out dataChanged);

					if (prevType != _sourceType)
					{
						ClearGameObject();
						dataChanged = true;
					}

					switch (_sourceType)
					{
						case eSourceType.Scene:
							{
								dataChanged |= RenderSceneObjectProperties();
							}
							break;
						case eSourceType.Prefab:
							{
								dataChanged |= RenderPrefabProperties();
							}
							break;
						case eSourceType.Loaded:
							{
								dataChanged |= RenderLoadedProperties();
							}
							break;
					}

					EditorGUI.indentLevel = origIndent;
				}

				return dataChanged;
			}
#endif
			#endregion

			#region Private Functions
#if UNITY_EDITOR
			private bool RenderPrefabProperties()
			{
				GameObject obj = (GameObject)EditorGUILayout.ObjectField("Object", _editorGameObject, typeof(GameObject), true);

				if (obj != _editorGameObject)
				{
					SetPrefabGameObject(obj);
					return true;
				}

				return false;
			}

			private bool RenderSceneObjectProperties()
			{
				bool dataChanged = false;

				if (_scene.IsSceneValid())
				{
					Scene scene = _scene.GetScene();

					if (scene.IsValid() && scene.isLoaded)
					{
						if (!_editorSceneLoaded)
						{
							_editorGameObject = GetSceneObject(scene);
							_editorSceneLoaded = true;

							if (_editorGameObject == null)
							{
								ClearGameObject();
								dataChanged = true;
							}
						}

						dataChanged |= RenderSceneObjectField();
					}
					else
					{
						_editorSceneLoaded = false;

						if (RenderSceneNotLoadedField())
						{
							ClearGameObject();
							dataChanged = true;
						}
					}
				}
				else
				{
					dataChanged |= RenderSceneObjectField();
				}

				return dataChanged;
			}

			private bool RenderSceneObjectField()
			{
				GameObject obj = (GameObject)EditorGUILayout.ObjectField("Object", _editorGameObject, typeof(GameObject), true);

				if (obj != _editorGameObject)
				{
					SetSceneGameObject(obj);
					return true;
				}

				return false;
			}

			private bool RenderLoadedProperties()
			{
				if (_scene.IsSceneValid())
				{
					Scene scene = _scene.GetScene();

					if (scene.IsValid() && scene.isLoaded)
					{
						if (!_editorSceneLoaded)
						{
							_editorLoaderGameObject = GetGameObjectLoader(scene);
							_editorGameObject = GetLoadedObject(scene);
							_editorSceneLoaded = true;
						}

						if (_editorLoaderGameObject != null)
						{
							if (_editorLoaderGameObject.IsLoaded())
							{
								if (!_editorLoaderIsLoaded)
								{
									_editorGameObject = GetLoadedObject(_scene.GetScene());
									_editorLoaderIsLoaded = true;
								}

								return RenderLoadedObjectField();
							}
							else
							{
								_editorLoaderIsLoaded = false;

								if (RenderLoadedNotLoadedField(_editorLoaderGameObject))
								{
									ClearGameObject();
									return true;
								}
							}
						}
					}
					else
					{
						_editorSceneLoaded = false;

						if (RenderSceneNotLoadedField())
						{
							ClearGameObject();
							return true;
						}
					}
				}
				else
				{
					return RenderLoadedObjectField();
				}

				return false;
			}

			private bool RenderLoadedObjectField()
			{
				EditorGUILayout.BeginHorizontal();
				GameObject obj = (GameObject)EditorGUILayout.ObjectField("Object", _editorGameObject, typeof(GameObject), true);
				EditorGUILayout.EndHorizontal();

				if (obj != _editorGameObject)
				{
					SetLoadedGameObject(obj);
					return true;
				}

				return false;
			}
#endif

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
			#endregion
		}
	}
}