#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

using System.Reflection;
using System.Collections.Generic;
using System;

using Object = UnityEngine.Object;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			[InitializeOnLoad]
			public static class UnityPlayModeSaver
			{
				#region Constants
				private const string kSaveMenuString = "\U0001F5AB  Save Play Mode Changes";
				private const string kSaveSnapshotMenuString = "\U0001F5AB  Save Play Mode Snapshot";
				private const string kRevertMenuString = "\U0001F5D9  Forget Play Mode Changes";

				private const string kSaveComponentMenuString = "CONTEXT/Component/" + kSaveMenuString;
				private const string kSaveComponentSnapshotMenuString = "CONTEXT/Component/" + kSaveSnapshotMenuString; 
				private const string kRevertComponentMenuString = "CONTEXT/Component/" + kRevertMenuString;
				private const string kSaveGameObjectMenuString = "GameObject/" + kSaveMenuString;
				private const string kSaveGameObjectSnapshotMenuString = "GameObject/" + kSaveSnapshotMenuString;
				private const string kRevertGameObjectMenuString = "GameObject/" + kRevertMenuString;
				private const string kWindowMenuString = "Window/Play Mode Saver";

				private const string kUndoText = "Play Mode Changes";
				private const string kEditorPrefsKey = "UnityPlayModeSaver.";
				private const string kEditorPrefsObjectCountKey = kEditorPrefsKey + "SavedObjects";

				private const string kEditorPrefsObjectDeleted = ".Deleted";
				private const string kEditorPrefsObjectScene = ".Scene";
				private const string kEditorPrefsObjectSceneId = ".SceneId";
				private const string kEditorPrefsSceneObjectChildren = ".Children";
				private const string kEditorPrefsScenePrefabInstanceId = ".PrefabInstId";
				private const string kEditorPrefsScenePrefabInstanceChildPath = ".PrefabInstPath";
				private const string kEditorPrefsRuntimeObjectId = ".RuntimeId";
				private const string kEditorPrefsRuntimeObjectType = ".RuntimeType";
				private const string kEditorPrefsRuntimeObjectParentId = ".RuntimeParentId";
				private const string kEditorPrefsRuntimeObjectSceneRootIndex = ".RuntimeSceneRootIndex";
				private const string kEditorPrefsRuntimeObjectPrefab = ".Prefab";
				private const string kEditorPrefsRuntimeObjectPrefabObjIndex = ".PrefabObjIndex";
				private const string kEditorPrefsObjectJson = ".Json";
				private const string kEditorPrefsObjectRefs = ".ObjRefs";
				private const string kEditorPrefsObjectMaterialRefs = ".Materials";
				private const string kEditorPrefsRuntimeObjectRefs = ".RuntimeRefs";

				private const char kItemSplitChar = '?';
				private const char kObjectPathSplitChar = ':';
				private const string kIdentifierProperty = "m_LocalIdentfierInFile";   //note the misspelling!
				private const string kInspectorProperty = "inspectorMode";

				private const string kPrefabUnpackWarningTitle = "Play Mode Saver";
				private const string kPrefabUnpackWarningMsg = "You removed a Component or child GameObject from a prefab during Play Mode which can't be applied without unpacking the prefab.\nDo you want to do this or ignore this change?";
				private const string kPrefabUnpackWarningUnpack = "Unpack";
				private const string kPrefabUnpackWarningIgnore = "Ignore";
				#endregion

				#region Helper Structs
				private struct RestoredObjectData
				{
					public Object _object;
					public Type _createdObjectType;
					public Object _parentObject;
					public GameObject _rootGameObject;
					public bool _deleted;
					public string _json;
					public string _scenePath;
					public string _missingObjectRefs;
					public string _missingMaterials;
					public string _runtimeInternalRefs;
				}

				private struct MaterialRef
				{
					public string _propertyPath;
					public Material _material;
				}

				private struct SavedObject
				{
					public Object _object;
					public int _sceneIdentifier;
					public string _scenePath;
					public string _name;
					public Type _type;
					public string _path;
					public bool _hasSnapshot;
					public string _snapshotEditorPrefKey;
				}

				private enum State
				{ 
					Idle,
					Busy,
				}
				#endregion

				#region Private Data
				private static State _state;
				private static PlayModeStateChange _currentPlayModeState;
				private static List<SavedObject> _savedObjects = new List<SavedObject>();
				#endregion

				#region Constructor
				static UnityPlayModeSaver()
				{
					_currentPlayModeState = Application.isPlaying ? PlayModeStateChange.EnteredPlayMode : PlayModeStateChange.EnteredEditMode;

					EditorApplication.playModeStateChanged += OnModeChanged;
					EditorSceneManager.sceneSaving += OnSceneSaved;
					SceneManager.sceneUnloaded += delegate (Scene scene) { OnSceneUnload(scene); };

					ClearCache();
				}
				#endregion

				#region Menu Functions
				[MenuItem(kSaveComponentMenuString, false, 12)]
				public static void SaveComponent(MenuCommand command)
				{
					Component component = command.context as Component;

					if (Application.isPlaying && component != null)
					{
						if (!IsObjectRegistered(component, out _))
						{
							RegisterSavedObject(component);
						}
						
						UnityPlayModeSaverWindow.Open(false);
					}	
				}

				[MenuItem(kSaveComponentSnapshotMenuString, false, 12)]
				public static void SaveComponentSnapshot(MenuCommand command)
				{
					Component component = command.context as Component;

					if (Application.isPlaying && component != null)
					{
						if (!IsObjectRegistered(component, out int index))
						{
							index = RegisterSavedObject(component);
						}

						SaveSnapshot(index);
						UnityPlayModeSaverWindow.Open(false);
					}
				}

				[MenuItem(kSaveComponentMenuString, true)]
				[MenuItem(kSaveComponentSnapshotMenuString, true)]
				public static bool ValidateSaveComponent(MenuCommand command)
				{
					Component component = command.context as Component;

					if (Application.isPlaying && component != null)
						return !IsObjectRegistered(component, out _);

					return false;
				}

				[MenuItem(kRevertComponentMenuString, false, 12)]
				public static void RevertComponent(MenuCommand command)
				{
					Component component = command.context as Component;

					if (Application.isPlaying && component != null)
					{
						UnregisterObject(component);
						UnityPlayModeSaverWindow.Open(false);
					}	
				}

				[MenuItem(kRevertComponentMenuString, true)]
				public static bool ValidateRevertComponent(MenuCommand command)
				{
					Component component = command.context as Component;

					if (Application.isPlaying && component != null)
						return IsObjectRegistered(component, out _);

					return false;
				}

				[MenuItem(kSaveGameObjectMenuString, false, -100)]
				public static void SaveGameObject()
				{
					if (Application.isPlaying && Selection.gameObjects != null)
					{
						foreach (GameObject gameObject in Selection.gameObjects)
						{
							if (!IsObjectRegistered(gameObject, out _))
							{
								RegisterSavedObject(gameObject);
							}
						}

						UnityPlayModeSaverWindow.Open(false);
					}	
				}

				[MenuItem(kSaveGameObjectSnapshotMenuString, false, -100)]
				public static void SaveGameObjectSnapshot()
				{
					if (Application.isPlaying && Selection.gameObjects != null)
					{
						foreach (GameObject gameObject in Selection.gameObjects)
						{
							if (!IsObjectRegistered(gameObject, out int index))
							{
								index = RegisterSavedObject(gameObject);
							}

							SaveSnapshot(index);
						}

						UnityPlayModeSaverWindow.Open(false);
					}
				}

				[MenuItem(kSaveGameObjectMenuString, true)]
				[MenuItem(kSaveGameObjectSnapshotMenuString, true)]
				public static bool ValidateSaveGameObject()
				{
					if (Application.isPlaying && Selection.gameObjects != null)
					{
						foreach (GameObject go in Selection.gameObjects)
						{
							if (!IsObjectRegistered(go, out _))
								return true;
						}
					}					

					return false;
				}

				[MenuItem(kRevertGameObjectMenuString, false, -100)]
				public static void RevertGameObject()
				{
					if (Application.isPlaying && Selection.gameObjects != null)
					{
						foreach (GameObject go in Selection.gameObjects)
						{
							UnregisterObject(go);
						}

						UnityPlayModeSaverWindow.Open(false);
					}
				}

				[MenuItem(kRevertGameObjectMenuString, true)]
				public static bool ValidateRevertGameObject()
				{
					if (Application.isPlaying && Selection.gameObjects != null)
					{
						foreach (GameObject go in Selection.gameObjects)
						{
							if (IsObjectRegistered(go, out _))
								return true;
						}
					}

					return false;
				}


				[MenuItem(kWindowMenuString)]
				public static void OpenWindow()
				{
					UnityPlayModeSaverWindow.Open(true);
				}
				#endregion

				#region Editor Functions
				private static void OnModeChanged(PlayModeStateChange state)
				{
					_currentPlayModeState = state;

					switch (state)
					{
						case PlayModeStateChange.ExitingEditMode:
							{
								//If restore or save failed, delete the cache
								if (_state != State.Idle)
								{
									ClearCache();
								}

								CacheScenePrefabs();
							}
							break;
						case PlayModeStateChange.ExitingPlayMode:
							{
								SaveObjectValues();
							}
							break;
						case PlayModeStateChange.EnteredEditMode:
							{
								//If save completed ok, restore objects
								if (_state == State.Idle)
								{
									RestoreSavedObjects();
								}
								//otherwise delete the cache
								else
								{
									ClearCache();
								}
								
								RepaintEditorWindows();
							}
							break;
						default: break;
					}
				}

				private static void OnSceneSaved(Scene scene, string path)
				{
					UnityPlayModeSaverSceneUtils.CacheScenePrefabInstances(scene);
				}

				private static void OnSceneUnload(Scene scene)
				{
					//Remove all saved objects from this scene that don't have snapshots
					for (int i=0; i<_savedObjects.Count;)
					{
						if (!_savedObjects[i]._hasSnapshot && _savedObjects[i]._scenePath == scene.path)
						{
							_savedObjects.RemoveAt(i);
						}
						else
						{
							i++;
						}
					}
				}

				private static void RepaintEditorWindows()
				{
					SceneView.RepaintAll();

					Type inspectorWindowType = Type.GetType("UnityEditor.InspectorWindow, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
					if (inspectorWindowType != null)
					{
						EditorWindow.GetWindow(inspectorWindowType).Repaint();
					}

					Type gameViewType = Type.GetType("UnityEditor.GameView, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

					if (gameViewType != null)
					{
						BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
						MethodInfo methodInfo = gameViewType.GetMethod("RepaintAll", bindingFlags, null, new Type[] { }, null);

						if (methodInfo != null)
						{
							methodInfo.Invoke(null, null);
						}
					}
				}
				#endregion

				#region Object Registering
				private static bool IsObjectRegistered(Object obj, out int saveObjIndex)
				{
					saveObjIndex = _savedObjects.FindIndex(x => x._object == obj);
					return saveObjIndex != -1;
				}

				private static int RegisterSavedObject(Object obj)
				{
					SavedObject savedObject = new SavedObject
					{
						_object = obj,
						_sceneIdentifier = GetSceneIdentifier(obj),
						_scenePath = GetScenePath(obj),
						_name = GetObjectName(obj),
						_type = obj.GetType(),
						_path = GetObjectPath(obj),
					};

					_savedObjects.Add(savedObject);
					return _savedObjects.Count - 1;
				}

				private static void UnregisterObject(Object obj)
				{
					if (IsObjectRegistered(obj, out int index))
					{
						ClearSavedObject(index);
					}
				}

				private static int AddSaveObject()
				{
					int objectCount = EditorPrefs.GetInt(kEditorPrefsObjectCountKey);
					int saveObjIndex = objectCount;
					objectCount++;
					EditorPrefs.SetInt(kEditorPrefsObjectCountKey, objectCount);
					return saveObjIndex;
				}

				private static int AddChildSaveObject(string parentEditorPrefKey)
				{
					int childCount = EditorPrefs.GetInt(parentEditorPrefKey + kEditorPrefsSceneObjectChildren, 0);
					int saveObjIndex = childCount;
					childCount++;
					EditorPrefs.SetInt(parentEditorPrefKey + kEditorPrefsSceneObjectChildren, childCount);

					return saveObjIndex;
				}

				#region Scene Objects
				private static string RegisterSceneObject(Object obj, string scenePath, int identifier)
				{
					//Check scene object is a prefab instance...
					if (IsScenePrefab(obj, scenePath, out GameObject prefabInstance, out int prefabSceneId))
					{
						return RegisterScenePrefabObject(scenePath, identifier, prefabInstance, prefabSceneId, obj);
					}
					else
					{
						int saveObjIndex = GetSavedSceneObjectIndex(identifier, scenePath);

						if (saveObjIndex == -1)
						{
							saveObjIndex = AddSaveObject();
						}

						string editorPrefKey = kEditorPrefsKey + Convert.ToString(saveObjIndex);

						EditorPrefs.SetString(editorPrefKey + kEditorPrefsObjectScene, scenePath);
						EditorPrefs.SetInt(editorPrefKey + kEditorPrefsObjectSceneId, identifier);

						return editorPrefKey;
					}
				}

				private static string RegisterChildSceneObject(string parentEditorPrefKey, Object obj, string scenePath, int identifier)
				{
					//Check scene object is a prefab instance...
					if (IsScenePrefab(obj, scenePath, out GameObject prefabInstance, out int prefabSceneId))
					{
						string prefabObjPath = GetScenePrefabChildObjectPath(prefabInstance, obj);

						//First check object is already saved
						int saveObjIndex = GetSavedScenePrefabObjectIndex(identifier, scenePath, prefabObjPath, prefabSceneId);

						if (saveObjIndex != -1)
						{
							//If so delete the origanal saved object (save in heirachy instead)
							string origEditorPrefKey = kEditorPrefsKey + Convert.ToString(saveObjIndex);
							DeleteObjectEditorPrefs(origEditorPrefKey);
						}

						int childObjIndex = AddChildSaveObject(parentEditorPrefKey);
						string editorPrefKey = parentEditorPrefKey + '.' + Convert.ToString(childObjIndex);

						EditorPrefs.SetInt(editorPrefKey + kEditorPrefsObjectSceneId, identifier);
						EditorPrefs.SetString(editorPrefKey + kEditorPrefsScenePrefabInstanceChildPath, prefabObjPath);
						EditorPrefs.SetInt(editorPrefKey + kEditorPrefsScenePrefabInstanceId, prefabSceneId);

						return editorPrefKey;
					}
					else
					{
						//First check object is already saved
						int saveObjIndex = GetSavedSceneObjectIndex(identifier, scenePath);
						
						if (saveObjIndex != -1)
						{
							//If so delete the origanal saved object (save in heirachy instead)
							string origEditorPrefKey = kEditorPrefsKey + Convert.ToString(saveObjIndex);
							DeleteObjectEditorPrefs(origEditorPrefKey);
						}

						int childObjIndex = AddChildSaveObject(parentEditorPrefKey);
						string editorPrefKey = parentEditorPrefKey + '.' + Convert.ToString(childObjIndex);

						EditorPrefs.SetInt(editorPrefKey + kEditorPrefsObjectSceneId, identifier);

						return editorPrefKey;
					}
				}

				private static string RegisterDeletedSceneObject(string scenePath, int identifier)
				{
					int saveObjIndex = GetSavedSceneObjectIndex(identifier, scenePath);

					if (saveObjIndex == -1)
					{
						saveObjIndex = AddSaveObject();
					}

					string editorPrefKey = kEditorPrefsKey + Convert.ToString(saveObjIndex);

					EditorPrefs.SetString(editorPrefKey + kEditorPrefsObjectScene, scenePath);
					EditorPrefs.SetInt(editorPrefKey + kEditorPrefsObjectSceneId, identifier);
					EditorPrefs.SetBool(editorPrefKey + kEditorPrefsObjectDeleted, true);
					
					return editorPrefKey;
				}

				private static int GetSavedSceneObjectIndex(int localIdentifier, string scenePath)
				{
					int numSavedObjects = EditorPrefs.GetInt(kEditorPrefsObjectCountKey, 0);

					for (int i = 0; i < numSavedObjects; i++)
					{
						string editorPrefKey = kEditorPrefsKey + Convert.ToString(i);

						string sceneStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectScene);
						int identifier = EditorPrefs.GetInt(editorPrefKey + kEditorPrefsObjectSceneId, -1);

						if (sceneStr == scenePath && localIdentifier == identifier)
						{
							return i;
						}
					}

					return -1;
				}
				#endregion

				#region Scene Prefab Objects
				private static bool IsScenePrefab(Object obj, string scenePath, out GameObject prefab, out int prefabSceneId)
				{
					if (GetActiveScene(scenePath, out Scene scene))
					{
						return UnityPlayModeSaverSceneUtils.IsScenePrefabInstance(obj, scene, out prefab, out prefabSceneId);
					}

					prefab = null;
					prefabSceneId = -1;
					return false;
				}

				private static string RegisterScenePrefabObject(string scenePath, int identifier, GameObject prefabInstance, int prefabSceneId, Object obj)
				{
					string prefabObjPath = GetScenePrefabChildObjectPath(prefabInstance, obj);

					int saveObjIndex = GetSavedScenePrefabObjectIndex(identifier, scenePath, prefabObjPath, prefabSceneId);

					if (saveObjIndex == -1)
					{
						saveObjIndex = AddSaveObject();
					}

					string editorPrefKey = kEditorPrefsKey + Convert.ToString(saveObjIndex);

					EditorPrefs.SetString(editorPrefKey + kEditorPrefsObjectScene, scenePath);
					EditorPrefs.SetInt(editorPrefKey + kEditorPrefsObjectSceneId, identifier);
					EditorPrefs.SetString(editorPrefKey + kEditorPrefsScenePrefabInstanceChildPath, prefabObjPath);
					EditorPrefs.SetInt(editorPrefKey + kEditorPrefsScenePrefabInstanceId, prefabSceneId);

					return editorPrefKey;
				}

				private static int GetSavedScenePrefabObjectIndex(int localIdentifier, string scenePath, string prefabPath, int prefabId)
				{
					int numSavedObjects = EditorPrefs.GetInt(kEditorPrefsObjectCountKey, 0);

					for (int i = 0; i < numSavedObjects; i++)
					{
						string editorPrefKey = kEditorPrefsKey + Convert.ToString(i);

						string sceneStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectScene);
						int identifier = EditorPrefs.GetInt(editorPrefKey + kEditorPrefsObjectSceneId, -1);
						string prefabObjPathStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsScenePrefabInstanceChildPath);
						int prefabObjId = EditorPrefs.GetInt(editorPrefKey + kEditorPrefsScenePrefabInstanceId, -1);

						if (sceneStr == scenePath && localIdentifier == identifier && prefabObjPathStr == prefabPath && prefabId == prefabObjId)
						{
							return i;
						}
					}

					return -1;
				}
				#endregion

				#region Runtime Objects
				private static string RegisterRuntimeObject(string scenePath, int instanceId)
				{
					int saveObjIndex = GetSavedRuntimeObjectIndex(instanceId, scenePath);

					if (saveObjIndex == -1)
					{
						saveObjIndex = AddSaveObject();
					}

					string editorPrefKey = kEditorPrefsKey + Convert.ToString(saveObjIndex);

					EditorPrefs.SetString(editorPrefKey + kEditorPrefsObjectScene, scenePath);
					EditorPrefs.SetInt(editorPrefKey + kEditorPrefsRuntimeObjectId, instanceId);

					return editorPrefKey;
				}

				private static int GetSavedRuntimeObjectIndex(int instanceId, string scenePath)
				{
					int numSavedObjects = EditorPrefs.GetInt(kEditorPrefsObjectCountKey, 0);

					for (int i = 0; i < numSavedObjects; i++)
					{
						string editorPrefKey = kEditorPrefsKey + Convert.ToString(i);

						string sceneStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectScene);
						int instance = EditorPrefs.GetInt(editorPrefKey + kEditorPrefsRuntimeObjectId, -1);

						if (sceneStr == scenePath && instanceId == instance)
						{
							return i;
						}
					}

					return -1;
				}
				#endregion

				#endregion

				#region Object Saving
				private static void SaveObjectValues()
				{
					_state = State.Busy;

					//Save all saved objects into editor prefs (might also add children)
					foreach (SavedObject obj in _savedObjects)
					{
						if (!obj._hasSnapshot)
						{
							SaveObjectValues(obj);
						}
					}

					_savedObjects.Clear();

					_state = State.Idle;
				}

				private static string SaveObjectValues(SavedObject obj)
				{
					string editorPrefKey = null;

					//Save scene object
					if (obj._sceneIdentifier != -1)
					{
						//Object is still valid
						if (obj._object != null)
						{
							editorPrefKey = RegisterSceneObject(obj._object, obj._scenePath, obj._sceneIdentifier);

							SaveObjectValues(editorPrefKey, obj._object);

							//If its a GameObject then add its components and child GameObjects (some of which might be runtime)
							if (obj._object is GameObject gameObject)
							{
								AddSceneGameObjectChildObjectValues(editorPrefKey, gameObject);
							}
						}
						//Object has been deleted
						else
						{
							RegisterDeletedSceneObject(obj._scenePath, obj._sceneIdentifier);
						}
					}
					//Save runtime object
					else
					{
						//Object is still valid
						if (obj._object != null)
						{
							int instanceId = GetInstanceId(obj._object);
							editorPrefKey = RegisterRuntimeObject(obj._scenePath, instanceId);

							GameObject sceneParent;
							GameObject topOfHieracy;

							if (obj._object is Component component)
							{
								topOfHieracy = component.gameObject;
								FindRuntimeObjectParent(component.gameObject, out sceneParent, ref topOfHieracy);

								//If the new component belongs to a scene object, just save the new component
								if (component.gameObject == sceneParent)
								{
									SaveRuntimeComponent(editorPrefKey, component, sceneParent, sceneParent);
								}
								//Otherwise need to save the whole new gameobject hierarchy
								else
								{
									SaveRuntimeGameObject(editorPrefKey, topOfHieracy, topOfHieracy, sceneParent, null);
								}
							}
							else if (obj._object is GameObject gameObject)
							{
								topOfHieracy = gameObject;
								FindRuntimeObjectParent(gameObject, out sceneParent, ref topOfHieracy);

								if (topOfHieracy != gameObject)
								{
									EditorPrefs.SetInt(editorPrefKey + kEditorPrefsRuntimeObjectId, GetInstanceId(topOfHieracy));
								}

								SaveRuntimeGameObject(editorPrefKey, topOfHieracy, topOfHieracy, sceneParent, null);
							}
						}
					}

					return editorPrefKey;
				}

				private static void SaveObjectValues(string editorPrefKey, Object obj, bool runtimeObject = false, GameObject runtimeObjectTopOfHeirachy = null)
				{
					RestoredObjectData data = GetObjectData(obj, runtimeObject, runtimeObjectTopOfHeirachy);

					EditorPrefs.SetString(editorPrefKey + kEditorPrefsObjectJson, data._json);
					EditorPrefs.SetString(editorPrefKey + kEditorPrefsObjectRefs, data._missingObjectRefs);
					EditorPrefs.SetString(editorPrefKey + kEditorPrefsObjectMaterialRefs, data._missingMaterials);
					EditorPrefs.SetString(editorPrefKey + kEditorPrefsRuntimeObjectRefs, data._runtimeInternalRefs);
				}

				private static bool ShouldUseEditorSerialiser(Object obj)
				{
					if ((obj is Component && !(obj is MonoBehaviour)) || (obj is GameObject))
						return true;

					return false;
				}

				private static RestoredObjectData GetObjectData(Object obj, bool runtimeObject, GameObject runtimeObjectTopOfHierarchy)
				{
					RestoredObjectData data = new RestoredObjectData();

					bool unityType = ShouldUseEditorSerialiser(obj);

					SerializedObject serializedObject = new SerializedObject(obj);
					SerializedProperty propertry = serializedObject.GetIterator();

					while (propertry.NextVisible(true))
					{
						//Store material properties that now point at a runtime instance of a material (they will get reverted to original values)
						if (propertry.type == "PPtr<Material>")
						{
							if (propertry.objectReferenceValue != null && propertry.objectReferenceValue.name.EndsWith("(Instance)"))
							{
								if (!string.IsNullOrEmpty(data._missingMaterials))
									data._missingMaterials += kItemSplitChar;

								data._missingMaterials += propertry.propertyPath;
							}
						}
						//Save any object ptr properties that point at scene objects
						else if (propertry.type.StartsWith("PPtr<") && propertry.objectReferenceValue != null)
						{
							if (unityType)
							{
								//Only store the object if the reference is within the same scene 
								Scene objScne = GetObjectScene(obj);

								if (objScne.IsValid() && objScne == GetObjectScene(propertry.objectReferenceValue))
								{
									int objId = GetSceneIdentifier(propertry.objectReferenceValue);

									if (objId != -1)
									{
										if (!string.IsNullOrEmpty(data._missingObjectRefs))
											data._missingObjectRefs += kItemSplitChar;

										data._missingObjectRefs += Convert.ToString(objId) + kObjectPathSplitChar + propertry.propertyPath;
									}
								}
							}

							if (runtimeObject)
							{
								//If object ref is part of hierachy then need to tsav
								if (GetChildObjectPath(string.Empty, runtimeObjectTopOfHierarchy, propertry.objectReferenceValue, false, out string path))
								{
									if (!string.IsNullOrEmpty(data._runtimeInternalRefs))
										data._runtimeInternalRefs += kItemSplitChar;

									data._runtimeInternalRefs += path + kObjectPathSplitChar + propertry.propertyPath;
								}
							}
						}
					}

					//If Object is a Unity builtin Component we have to restore any scene links as they won't be serialized by EditorJsonUtility
					if (unityType)
					{
						data._json = EditorJsonUtility.ToJson(obj);
					}
					else
					{
						data._json = JsonUtility.ToJson(obj);
					}

					return data;
				}

				#region Scene Objects
				private static Object FindSceneObject(string scenePath, int localIdentifier, bool loadSceneIfNeeded = false)
				{
					if (localIdentifier != -1)
					{
						if (GetActiveScene(scenePath, out Scene scene, loadSceneIfNeeded))
						{
							foreach (GameObject rootObject in scene.GetRootGameObjects())
							{
								Object obj = FindSceneObject(rootObject, localIdentifier);

								if (obj != null)
								{
									return obj;
								}
							}
						}
					}

					return null;
				}

				private static Object FindSceneObject(GameObject gameObject, int localIdentifier)
				{
					if (gameObject != null && localIdentifier != -1)
					{
						//Check game object
						if (GetSceneIdentifier(gameObject) == localIdentifier)
							return gameObject;

						//Check components
						Component[] components = gameObject.GetComponents<Component>();

						foreach (Component component in components)
						{
							if (GetSceneIdentifier(component) == localIdentifier)
								return component;
						}

						//Check children
						foreach (Transform child in gameObject.transform)
						{
							Object obj = FindSceneObject(child.gameObject, localIdentifier);

							if (obj != null)
								return obj;
						}
					}

					return null;
				}

				private static void AddSceneGameObjectChildObjectValues(string parentEditorPrefKey, GameObject gameObject)
				{
					Component[] components = gameObject.GetComponents<Component>();

					//Save each component
					for (int i = 0; i < components.Length; i++)
					{
						int identifier = GetSceneIdentifier(components[i]);

						//Scene component
						if (identifier != -1)
						{
							string editorPrefKey = RegisterChildSceneObject(parentEditorPrefKey, components[i], gameObject.scene.path, identifier);
							SaveObjectValues(editorPrefKey, components[i]);
						}
						//Runtime component
						else
						{
							int instanceId = GetInstanceId(components[i]);
							string scenePath = gameObject.scene.path;
							string editorPrefKey = RegisterRuntimeObject(scenePath, instanceId);
							SaveRuntimeComponent(editorPrefKey, components[i], gameObject, gameObject);
						}
					}

					//Save each child object
					foreach (Transform child in gameObject.transform)
					{
						int identifier = GetSceneIdentifier(child.gameObject);

						//Scene gameobject
						if (identifier != -1)
						{
							string editorPrefKey = RegisterChildSceneObject(parentEditorPrefKey, child.gameObject, child.gameObject.scene.path, identifier);
							SaveObjectValues(editorPrefKey, child.gameObject);
							AddSceneGameObjectChildObjectValues(parentEditorPrefKey, child.gameObject);
						}
						//Runtime gameobject
						else
						{
							int instanceId = GetInstanceId(child.gameObject);
							string scenePath = gameObject.scene.path;
							string editorPrefKey = RegisterRuntimeObject(scenePath, instanceId);
							SaveRuntimeGameObject(editorPrefKey, child.gameObject, child.gameObject, gameObject, null);
						}
					}
				}
				#endregion

				#region Scene Prefab Objects
				private static Object FindScenePrefabObject(string scenePath, int instanceId, string prefabObjPath, bool loadSceneIfNeeded = false)
				{
					if (GetActiveScene(scenePath, out Scene scene, loadSceneIfNeeded))
					{
						GameObject prefabInstance = UnityPlayModeSaverSceneUtils.GetScenePrefabInstance(scene, instanceId);

						if (prefabInstance != null)
						{
							return GetChildObject(prefabInstance, prefabObjPath);
						}
					}

					return null;
				}
				#endregion

				#region Runtime Objects
				private static Object FindRuntimeObject(string scenePath, int instanceId)
				{
					if (GetActiveScene(scenePath, out Scene scene))
					{
						foreach (GameObject rootObject in scene.GetRootGameObjects())
						{
							Object obj = FindRuntimeObject(rootObject, instanceId);

							if (obj != null)
							{
								return obj;
							}
						}
					}

					return null;
				}

				private static Object FindRuntimeObject(GameObject gameObject, int instanceId)
				{
					//Check game object
					if (GetInstanceId(gameObject) == instanceId)
						return gameObject;

					//Check components
					Component[] components = gameObject.GetComponents<Component>();

					foreach (Component component in components)
					{
						if (GetInstanceId(component) == instanceId)
							return component;
					}

					//Check children
					foreach (Transform child in gameObject.transform)
					{
						Object obj = FindRuntimeObject(child.gameObject, instanceId);

						if (obj != null)
							return obj;
					}

					return null;
				}

				private static void SaveRuntimeGameObject(string editorPrefKey, GameObject gameObject, GameObject topOfHieracy, GameObject parentSceneObject, GameObject parentPrefab)
				{
					EditorPrefs.SetString(editorPrefKey + kEditorPrefsRuntimeObjectType, GetTypeString(gameObject.GetType()));
					SaveObjectValues(editorPrefKey, gameObject, true, topOfHieracy);

					SaveRuntimeGameObjectParent(editorPrefKey, gameObject, parentSceneObject);

					//Check if this game object is a prefab
					GameObject prefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(gameObject);

					if (prefabRoot == gameObject)
					{
						Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource(prefabRoot);
						string prefabPath = AssetDatabase.GetAssetPath(parentObject);
						EditorPrefs.SetString(editorPrefKey + kEditorPrefsRuntimeObjectPrefab, prefabPath);
						parentPrefab = prefabRoot;
					}

					//Save all components and child GameObjects
					int childObjectIndex = 0;

					Component[] components = gameObject.GetComponents<Component>();

					for (int i = 0; i < components.Length; i++)
					{
						bool isPartOfCurrentPrefabHieracy = parentPrefab != null && PrefabUtility.GetNearestPrefabInstanceRoot(components[i]) == parentPrefab;

						SaveRuntimeComponent(editorPrefKey + "." + Convert.ToString(childObjectIndex), components[i], topOfHieracy, null);

						if (isPartOfCurrentPrefabHieracy)
							EditorPrefs.SetInt(editorPrefKey + "." + Convert.ToString(childObjectIndex) + kEditorPrefsRuntimeObjectPrefabObjIndex, GetPrefabComponentIndex(parentPrefab, components[i]));
					
						childObjectIndex++;
					}
					
					foreach (Transform child in gameObject.transform)
					{
						bool isPartOfCurrentPrefabHieracy = parentPrefab != null && PrefabUtility.GetNearestPrefabInstanceRoot(child.gameObject) == parentPrefab;

						SaveRuntimeGameObject(editorPrefKey + "." + Convert.ToString(childObjectIndex), child.gameObject, topOfHieracy, null, isPartOfCurrentPrefabHieracy ? parentPrefab : null);

						if (isPartOfCurrentPrefabHieracy)
							EditorPrefs.SetInt(editorPrefKey + "." + Convert.ToString(childObjectIndex) + kEditorPrefsRuntimeObjectPrefabObjIndex, GetPrefabChildIndex(parentPrefab, child.gameObject));

						childObjectIndex++;
					}

					//If a component contains object ref and ref is part of this runtime objects heiracy then save a ref to it somehow?
				}

				private static void SaveRuntimeComponent(string editorPrefKey, Component component, GameObject topOfHieracy, GameObject parentSceneObject)
				{
					EditorPrefs.SetString(editorPrefKey + kEditorPrefsRuntimeObjectType, GetTypeString(component.GetType()));
					SaveObjectValues(editorPrefKey, component, true, topOfHieracy);
					SaveRuntimeGameObjectParent(editorPrefKey, component.gameObject, parentSceneObject);
				}

				private static void SaveRuntimeGameObjectParent(string editorPrefKey, GameObject gameObject, GameObject parentSceneObject)
				{
					if (parentSceneObject != null)
					{
						int identifier = GetSceneIdentifier(parentSceneObject);

						if (identifier != -1)
						{
							if (UnityPlayModeSaverSceneUtils.IsScenePrefabInstance(parentSceneObject, parentSceneObject.scene, out GameObject prefabInstance, out int prefabSceneId))
							{
								string prefabPath = GetScenePrefabChildObjectPath(prefabInstance, parentSceneObject);

								EditorPrefs.SetInt(editorPrefKey + kEditorPrefsScenePrefabInstanceId, prefabSceneId);
								EditorPrefs.SetString(editorPrefKey + kEditorPrefsScenePrefabInstanceChildPath, prefabPath);
							}

							EditorPrefs.SetInt(editorPrefKey + kEditorPrefsRuntimeObjectParentId, identifier);
						}
					}
					else
					{
						EditorPrefs.SetInt(editorPrefKey + kEditorPrefsRuntimeObjectSceneRootIndex, gameObject.transform.GetSiblingIndex());
					}
				}

				private static void FindRuntimeObjectParent(GameObject gameObject, out GameObject sceneParent, ref GameObject topOfHieracy)
				{
					if (GetSceneIdentifier(gameObject) != -1)
					{
						sceneParent = gameObject;
					}
					else if (gameObject.transform.parent == null)
					{
						sceneParent = null;
						topOfHieracy = gameObject;
					}
					else
					{
						topOfHieracy = gameObject;
						FindRuntimeObjectParent(gameObject.transform.parent.gameObject, out sceneParent, ref topOfHieracy);
					}
				}

				private static GameObject GetRuntimeObjectParent(string editorPrefKey, string sceneStr, bool loadSceneIfNeeded = false)
				{
					//Parent is Scene Prefab Instance
					if (EditorPrefs.HasKey(editorPrefKey + kEditorPrefsScenePrefabInstanceId))
					{
						//Get prefab from scene
						int prefabId = EditorPrefs.GetInt(editorPrefKey + kEditorPrefsScenePrefabInstanceId, -1);
						string prefabObjPathStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsScenePrefabInstanceChildPath);

						Object obj = FindScenePrefabObject(sceneStr, prefabId, prefabObjPathStr, loadSceneIfNeeded);
						return obj as GameObject;
					}
					//Parent is Scene object
					else
					{
						int identifier = EditorPrefs.GetInt(editorPrefKey + kEditorPrefsRuntimeObjectParentId, -1);
						Object obj = FindSceneObject(sceneStr, identifier, loadSceneIfNeeded);
						return obj as GameObject;
					}
				}
				#endregion

				#endregion

				#region Object Restoring
				private static void RestoreSavedObjects()
				{
					_state = State.Busy;

					int numSavedObjects = EditorPrefs.GetInt(kEditorPrefsObjectCountKey, 0);

					List<Object> restoredObjects = new List<Object>();
					List<RestoredObjectData> restoredObjectsData = new List<RestoredObjectData>();
					
					for (int i = 0; i < numSavedObjects; i++)
					{
						string editorPrefKey = kEditorPrefsKey + Convert.ToString(i);
						string sceneStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectScene, null);

						//Scene object
						if (EditorPrefs.HasKey(editorPrefKey + kEditorPrefsObjectSceneId))
						{
							//Has object been deleted?
							if (EditorPrefs.GetBool(editorPrefKey + kEditorPrefsObjectDeleted, false))
							{
								int identifier = EditorPrefs.GetInt(editorPrefKey + kEditorPrefsObjectSceneId, -1);

								Object obj = FindSceneObject(sceneStr, identifier, true);

								if (obj != null)
								{
									restoredObjects.Add(obj);
									restoredObjectsData.Add(CreateDeletedSceneObjectRestoredData(obj, sceneStr));
								}
							}
							//Otherwise restore as normal
							else
							{
								//Scene Prefab object
								if (EditorPrefs.HasKey(editorPrefKey + kEditorPrefsScenePrefabInstanceId))
								{
									//Get prefab from scene
									int prefabId = EditorPrefs.GetInt(editorPrefKey + kEditorPrefsScenePrefabInstanceId, -1);
									string prefabObjPathStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsScenePrefabInstanceChildPath);

									Object obj = FindScenePrefabObject(sceneStr, prefabId, prefabObjPathStr, true);

									if (obj != null)
									{
										restoredObjects.Add(obj);
										restoredObjectsData.Add(CreateSceneObjectRestoredData(editorPrefKey, obj, sceneStr));
										
										//If its a game object also restore any saved child objects
										if (obj is GameObject gameObject)
										{
											RestoreSavedObjectChildren(editorPrefKey, gameObject, sceneStr, ref restoredObjects, ref restoredObjectsData);
										}
									}
								}
								//Normal Scene object
								else
								{
									int identifier = EditorPrefs.GetInt(editorPrefKey + kEditorPrefsObjectSceneId, -1);

									Object obj = FindSceneObject(sceneStr, identifier, true);

									if (obj != null)
									{
										restoredObjects.Add(obj);
										restoredObjectsData.Add(CreateSceneObjectRestoredData(editorPrefKey, obj, sceneStr));

										//If its a game object also restore any saved child objects
										if (obj is GameObject gameObject)
										{
											RestoreSavedObjectChildren(editorPrefKey, gameObject, sceneStr, ref restoredObjects, ref restoredObjectsData);
										}
									}
								}
							}
						}
						//Runtime Object
						else
						{
							string typeStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsRuntimeObjectType);

							Type objType = GetType(typeStr);
							GameObject parentObj = GetRuntimeObjectParent(editorPrefKey, sceneStr, true) as GameObject;

							//New runtime gameobject hierachy
							if (objType == typeof(GameObject))
							{
								GameObject gameObject = null;
								Scene scene;
								bool isPrefab = false;

								//If the object is a prefab, instantiate it
								if (EditorPrefs.HasKey(editorPrefKey + kEditorPrefsRuntimeObjectPrefab))
								{
									string prefabPath = EditorPrefs.GetString(editorPrefKey + kEditorPrefsRuntimeObjectPrefab);
									GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

									if (prefab != null)
									{
										gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
										isPrefab = gameObject != null;
									}
								}

								//Otherwise create blank game object
								if (!isPrefab)
								{
									gameObject = new GameObject();
								}

								//If we have a parent scene object, move it to become a child
								if (parentObj != null)
								{
									gameObject.transform.parent = parentObj.transform;
								}
								//otherwise make sure its in the correct scene
								else if (GetActiveScene(sceneStr, out scene, true))
								{
									SceneManager.MoveGameObjectToScene(gameObject, scene);
									int sceneRootIndex = EditorPrefs.GetInt(editorPrefKey + kEditorPrefsRuntimeObjectSceneRootIndex, 0);
									gameObject.transform.SetSiblingIndex(sceneRootIndex);
								}

								List<RestoredObjectData> runtimeRestoredObjects = new List<RestoredObjectData>
									{
										CreateRuntimeObjectRestoredData(editorPrefKey, gameObject, gameObject, sceneStr)
									};

								//Create the new objects heirachy
								gameObject = RestoreRuntimeGameObject(gameObject, editorPrefKey, sceneStr, gameObject, runtimeRestoredObjects);

								//Then restore all the new objects data
								for (int j = 0; j < runtimeRestoredObjects.Count; j++)
								{
									RestoreObjectFromData(runtimeRestoredObjects[j]);
								}

								Undo.RegisterCreatedObjectUndo(gameObject, kUndoText);
							}
							//New runtime component on an existing scene object
							else if (typeof(Component).IsAssignableFrom(objType))
							{
								if (parentObj != null)
								{
									restoredObjectsData.Add(CreateRuntimeCopmonentRestoredData(editorPrefKey, objType, parentObj, sceneStr));
								}
							}
						}

						DeleteObjectEditorPrefs(editorPrefKey);
					}

					if (restoredObjectsData.Count > 0)
					{
						Undo.RecordObjects(restoredObjects.ToArray(), kUndoText);

						for (int i = 0; i < restoredObjectsData.Count; i++)
						{
							RestoreObjectFromData(restoredObjectsData[i]);
						}
					}

					SafeDeleteEditorPref(kEditorPrefsObjectCountKey);
					_state = State.Idle;
				}

				private static void RestoreObjectFromData(RestoredObjectData data)
				{
					if (data._object == null)
					{
						GameObject parent = data._parentObject as GameObject;

						if (typeof(Component).IsAssignableFrom(data._createdObjectType) && parent != null)
						{
							data._object = Undo.AddComponent(parent, data._createdObjectType);
						}
					}

					if (data._object != null)
					{
						if (data._deleted)
						{
							//If object is part of prefab instance then cant destory it without breaking prefab instance
							GameObject prefabInstance = PrefabUtility.GetNearestPrefabInstanceRoot(data._object);

							if (prefabInstance != null)
							{
								if (EditorUtility.DisplayDialog(kPrefabUnpackWarningTitle, kPrefabUnpackWarningMsg, kPrefabUnpackWarningUnpack, kPrefabUnpackWarningIgnore))
								{
									//Unpack prefab.
									PrefabUtility.UnpackPrefabInstance(prefabInstance, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);

									//Destory object
									Undo.DestroyObjectImmediate(data._object);
								}
							}
							else
							{
								Undo.DestroyObjectImmediate(data._object);
							}
						}
						else
						{
							bool unityType = ShouldUseEditorSerialiser(data._object);

							//Find any lost material refs
							List<MaterialRef> materialRefs = FindOriginalMaterials(data._object, data._missingMaterials);

							if (unityType)
							{
								EditorJsonUtility.FromJsonOverwrite(data._json, data._object);
								ApplyObjectRefs(data._object, data._scenePath, data._missingObjectRefs);
							}
							else
							{
								JsonUtility.FromJsonOverwrite(data._json, data._object);
							}

							//Apply runtime internal refs
							ApplyRuntimeObjectRefs(data._object, data._rootGameObject, data._runtimeInternalRefs);

							//Revert any lost material refs
							ApplyMaterialsRefs(data._object, materialRefs);

							//Refresh Canvas renderers
							DirtyCanvasRenderers(data._object);

							EditorUtility.SetDirty(data._object);
						}					
					}
				}

				private static void RestoreSavedObjectChildren(string parentEditorPrefKey, GameObject gameObject, string sceneStr, ref List<Object> restoredObjects, ref List<RestoredObjectData> restoredObjectsData)
				{
					//First build a dictionary of saved object data for child objects (components / gameobjects)
					Dictionary<Object, RestoredObjectData> childrenData = new Dictionary<Object, RestoredObjectData>();

					int children = EditorPrefs.GetInt(parentEditorPrefKey + kEditorPrefsSceneObjectChildren, 0);

					for (int i = 0; i < children; i++)
					{
						string editorPrefKey = parentEditorPrefKey + '.' + Convert.ToString(i);

						//Scene Prefab object
						if (EditorPrefs.HasKey(editorPrefKey + kEditorPrefsScenePrefabInstanceId))
						{
							//Get prefab from scene
							int prefabId = EditorPrefs.GetInt(editorPrefKey + kEditorPrefsScenePrefabInstanceId, -1);
							string prefabObjPathStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsScenePrefabInstanceChildPath);

							Object obj = FindScenePrefabObject(sceneStr, prefabId, prefabObjPathStr, true);

							if (obj != null)
							{
								childrenData.Add(obj, CreateSceneObjectRestoredData(editorPrefKey, obj, sceneStr));
							}
						}
						//Normal Scene object
						else
						{
							int identifier = EditorPrefs.GetInt(editorPrefKey + kEditorPrefsObjectSceneId, -1);

							Object obj = FindSceneObject(sceneStr, identifier, true);

							if (obj != null)
							{
								childrenData.Add(obj, CreateSceneObjectRestoredData(editorPrefKey, obj, sceneStr));
							}
						}
					}

					//Then go through heirachy and restore all child components / gameobejcts from this dictionary
					RestoreChildSavedObject(gameObject, sceneStr, childrenData, ref restoredObjects, ref restoredObjectsData);
				}

				private static void RestoreChildSavedObject(GameObject gameObject, string sceneStr, Dictionary<Object, RestoredObjectData> childrenData, ref List<Object> restoredObjects, ref List<RestoredObjectData> restoredObjectsData)
				{
					Component[] components = gameObject.GetComponents<Component>();

					//Save each component
					for (int i = 0; i < components.Length; i++)
					{
						restoredObjects.Add(components[i]);

						if (childrenData.TryGetValue(components[i], out RestoredObjectData componentRestoredObjectData))
						{
							restoredObjectsData.Add(componentRestoredObjectData);
						}
						//If don't have saved data for the component then delete it (unless its a Transform which can't be deleted)
						else if (components[i].GetType() != typeof(Transform))
						{
							restoredObjectsData.Add(CreateDeletedSceneObjectRestoredData(components[i], sceneStr));
						}
					}

					//Save each child object
					foreach (Transform child in gameObject.transform)
					{
						restoredObjects.Add(child.gameObject);

						if (childrenData.TryGetValue(child.gameObject, out RestoredObjectData restoredObjectData))
						{
							restoredObjectsData.Add(restoredObjectData);
							
							RestoreChildSavedObject(child.gameObject, sceneStr, childrenData, ref restoredObjects, ref restoredObjectsData);
						}
						//If don't have saved data for the child GameObject then delete it
						else
						{
							restoredObjectsData.Add(CreateDeletedSceneObjectRestoredData(child.gameObject, sceneStr));
						}
					}
				}

				private static List<MaterialRef> FindOriginalMaterials(Object obj, string materialStr)
				{
					List<MaterialRef> materialRefs = new List<MaterialRef>();

					SerializedObject serializedObject = new SerializedObject(obj);
					string[] materials = materialStr.Split(kItemSplitChar);

					foreach (string material in materials)
					{
						SerializedProperty materialProp = serializedObject.FindProperty(material);

						if (materialProp != null)
						{
							MaterialRef materialRef = new MaterialRef
							{
								_material = materialProp.objectReferenceValue as Material,
								_propertyPath = material

							};
							materialRefs.Add(materialRef);
						}
					}

					return materialRefs;
				}

				private static void ApplyObjectRefs(Object obj, string sceneStr, string objectRefStr)
				{
					if (objectRefStr != null)
					{
						SerializedObject serializedObject = new SerializedObject(obj);
						string[] objectRefs = objectRefStr.Split(kItemSplitChar);

						foreach (string objectRef in objectRefs)
						{
							int split = objectRef.IndexOf(kObjectPathSplitChar);

							if (split != -1)
							{
								int id = SafeConvertToInt(objectRef.Substring(0, split));

								if (id != -1)
								{
									string objPath = objectRef.Substring(split + 1, objectRef.Length - split - 1);

									SerializedProperty localIdProp = serializedObject.FindProperty(objPath);

									if (localIdProp != null)
									{
										localIdProp.objectReferenceValue = FindSceneObject(sceneStr, id);
									}
								}
							}
						}

						serializedObject.ApplyModifiedPropertiesWithoutUndo();
					}
				}

				private static void ApplyRuntimeObjectRefs(Object obj, GameObject rootGameObject, string objectRefStr)
				{
					if (objectRefStr != null)
					{
						SerializedObject serializedObject = new SerializedObject(obj);
						string[] objectRefs = objectRefStr.Split(kItemSplitChar);

						foreach (string objectRef in objectRefs)
						{
							int split = objectRef.IndexOf(kObjectPathSplitChar);

							if (split != -1)
							{
								string runtimeHierarchyPath = objectRef.Substring(0, split);
								string objPath = objectRef.Substring(split + 1, objectRef.Length - split - 1);

								SerializedProperty localIdProp = serializedObject.FindProperty(objPath);

								if (localIdProp != null)
								{
									localIdProp.objectReferenceValue = GetChildObject(rootGameObject, runtimeHierarchyPath);
								}
							}
						}

						serializedObject.ApplyModifiedPropertiesWithoutUndo();
					}
				}

				private static void ApplyMaterialsRefs(Object obj, List<MaterialRef> materialRefs)
				{
					SerializedObject serializedObject = new SerializedObject(obj);

					foreach (MaterialRef materialRef in materialRefs)
					{
						SerializedProperty materialProp = serializedObject.FindProperty(materialRef._propertyPath);

						if (materialProp != null)
						{
							materialProp.objectReferenceValue = materialRef._material;
						}
					}

					serializedObject.ApplyModifiedPropertiesWithoutUndo();
				}

				private static void DirtyCanvasRenderers(Object obj)
				{
					if (obj is Graphic graphic)
					{
						graphic.SetAllDirty();
					}
					else if (obj is GameObject gameObject)
					{
						DirtyCanvasRenderers(gameObject);
					}
				}

				private static void DirtyCanvasRenderers(GameObject gameObject)
				{
					Graphic[] graphicComponents = gameObject.GetComponents<Graphic>();

					for (int i = 0; i < graphicComponents.Length; i++)
					{
						graphicComponents[i].SetAllDirty();
					}

					foreach (Transform child in gameObject.transform)
					{
						DirtyCanvasRenderers(child.gameObject);
					}
				}

				#region Scene Objects
				private static RestoredObjectData CreateSceneObjectRestoredData(string editorPrefKey, Object obj, string sceneStr)
				{
					return new RestoredObjectData
					{
						_object = obj,
						_createdObjectType = null,
						_parentObject = null,
						_rootGameObject = null,
						_json = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectJson),
						_scenePath = sceneStr,
						_missingObjectRefs = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectRefs),
						_missingMaterials = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectMaterialRefs),
						_runtimeInternalRefs = null
					};
				}

				private static RestoredObjectData CreateDeletedSceneObjectRestoredData(Object obj, string sceneStr)
				{
					return new RestoredObjectData
					{
						_object = obj,
						_deleted = true,
						_createdObjectType = null,
						_parentObject = null,
						_rootGameObject = null,
						_json = null,
						_scenePath = sceneStr,
						_missingObjectRefs = null,
						_missingMaterials = null,
						_runtimeInternalRefs = null
					};
				}
				#endregion

				#region Runtime Objects
				private static RestoredObjectData CreateRuntimeObjectRestoredData(string editorPrefKey, Object obj, GameObject rootObject, string sceneStr)
				{
					return new RestoredObjectData
					{
						_object = obj,
						_createdObjectType = null,
						_parentObject = null,
						_rootGameObject = rootObject,
						_json = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectJson),
						_scenePath = sceneStr,
						_missingObjectRefs = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectRefs),
						_missingMaterials = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectMaterialRefs),
						_runtimeInternalRefs = EditorPrefs.GetString(editorPrefKey + kEditorPrefsRuntimeObjectRefs)
					};
				}

				private static RestoredObjectData CreateRuntimeCopmonentRestoredData(string editorPrefKey, Type componentType, GameObject parentGameObject, string sceneStr)
				{
					return new RestoredObjectData
					{
						_object = null,
						_createdObjectType = componentType,
						_parentObject = parentGameObject,
						_rootGameObject = null,
						_json = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectJson),
						_scenePath = sceneStr,
						_missingObjectRefs = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectRefs),
						_missingMaterials = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectMaterialRefs),
						_runtimeInternalRefs = null
					};
				}

				private static GameObject RestoreRuntimeGameObject(GameObject gameObject, string editorPrefKey, string sceneStr, GameObject runtimeObjectRoot, List<RestoredObjectData> restoredObjectsData)
				{
					int childIndex = 0;
					string childeditorPrefKey;

					while (EditorPrefs.HasKey((childeditorPrefKey = editorPrefKey + "." + Convert.ToString(childIndex)) + kEditorPrefsRuntimeObjectType))
					{
						string typeStr = EditorPrefs.GetString(childeditorPrefKey + kEditorPrefsRuntimeObjectType);
						Type objType = GetType(typeStr);

						Object obj = null;

						if (objType == typeof(Transform))
						{
							obj = gameObject.transform;
						}
						else if (typeof(Component).IsAssignableFrom(objType))
						{
							//If a prefab component..
							if (EditorPrefs.HasKey(childeditorPrefKey + kEditorPrefsRuntimeObjectPrefabObjIndex))
							{
								int componentIndex = EditorPrefs.GetInt(childeditorPrefKey + kEditorPrefsRuntimeObjectPrefabObjIndex, -1);
								obj = GetPrefabComponent(runtimeObjectRoot, gameObject, objType, componentIndex);
							}

							if (obj == null)
							{
								obj = gameObject.AddComponent(objType);
							}
						}
						else if(objType == typeof(GameObject))
						{
							GameObject childGameObject = null;

							//Check is a new prefab instance
							if (EditorPrefs.HasKey(childeditorPrefKey + kEditorPrefsRuntimeObjectPrefab))
							{
								string prefabPath = EditorPrefs.GetString(childeditorPrefKey + kEditorPrefsRuntimeObjectPrefab);
								GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

								if (prefab != null)
								{
									childGameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
									childGameObject.transform.parent = gameObject.transform;
									runtimeObjectRoot = childGameObject;
								}
							}

							//If a prefab child..
							if (EditorPrefs.HasKey(childeditorPrefKey + kEditorPrefsRuntimeObjectPrefabObjIndex))
							{
								int childGameObjectIndex = EditorPrefs.GetInt(childeditorPrefKey + kEditorPrefsRuntimeObjectPrefabObjIndex, -1);
								childGameObject = GetPrefabChild(runtimeObjectRoot, gameObject, childGameObjectIndex);
							}

							if (childGameObject == null)
							{
								childGameObject = new GameObject();
								childGameObject.transform.parent = gameObject.transform;
							}

							obj = childGameObject;
							RestoreRuntimeGameObject(childGameObject, childeditorPrefKey, sceneStr, runtimeObjectRoot, restoredObjectsData);
						}

						restoredObjectsData.Add(CreateRuntimeObjectRestoredData(childeditorPrefKey, obj, runtimeObjectRoot, sceneStr));
						
						DeleteObjectEditorPrefs(childeditorPrefKey);

						childIndex++;
					}

					return gameObject;
				}

				private static void DeleteObjectEditorPrefs(string editorPrefKey)
				{
					SafeDeleteEditorPref(editorPrefKey + kEditorPrefsObjectScene);
					SafeDeleteEditorPref(editorPrefKey + kEditorPrefsObjectSceneId);
					SafeDeleteEditorPref(editorPrefKey + kEditorPrefsObjectDeleted);
					SafeDeleteEditorPref(editorPrefKey + kEditorPrefsScenePrefabInstanceId);
					SafeDeleteEditorPref(editorPrefKey + kEditorPrefsScenePrefabInstanceChildPath);
					SafeDeleteEditorPref(editorPrefKey + kEditorPrefsRuntimeObjectId);
					SafeDeleteEditorPref(editorPrefKey + kEditorPrefsRuntimeObjectParentId);
					SafeDeleteEditorPref(editorPrefKey + kEditorPrefsRuntimeObjectSceneRootIndex);
					SafeDeleteEditorPref(editorPrefKey + kEditorPrefsRuntimeObjectType);
					SafeDeleteEditorPref(editorPrefKey + kEditorPrefsRuntimeObjectPrefab);
					SafeDeleteEditorPref(editorPrefKey + kEditorPrefsRuntimeObjectPrefabObjIndex);
					SafeDeleteEditorPref(editorPrefKey + kEditorPrefsObjectJson);
					SafeDeleteEditorPref(editorPrefKey + kEditorPrefsObjectRefs);
					SafeDeleteEditorPref(editorPrefKey + kEditorPrefsObjectMaterialRefs);
					SafeDeleteEditorPref(editorPrefKey + kEditorPrefsRuntimeObjectRefs);

					int children = EditorPrefs.GetInt(editorPrefKey + kEditorPrefsSceneObjectChildren, 0);

					for (int i=0; i<children; i++)
					{
						DeleteObjectEditorPrefs(editorPrefKey + '.' + i);
					}

					SafeDeleteEditorPref(editorPrefKey + kEditorPrefsSceneObjectChildren);
				}

				private static void ClearSavedObject(int saveObjIndex)
				{
					//Remove values from prefs
					if (_savedObjects[saveObjIndex]._hasSnapshot)
					{
						ClearSnapshot(saveObjIndex);
					}
					
					_savedObjects.RemoveAt(saveObjIndex);
				}
				#endregion

				#endregion

				#region Scene Prefab Instances
				private static void CacheScenePrefabs()
				{
					for (int i=0; i<SceneManager.sceneCount; i++)
					{
						Scene scene = SceneManager.GetSceneAt(i);

						if (scene.IsValid() && scene.isLoaded)
						{
							UnityPlayModeSaverSceneUtils.CacheScenePrefabInstances(scene);
						}
					}
				}

				private static string GetScenePrefabChildObjectPath(GameObject prefab, Object obj)
				{
					if (GetChildObjectPath(string.Empty, prefab, obj, true, out string path))
					{
						return path;
					}

					return string.Empty;
				}
				#endregion

				#region Snapshots
				private static void SaveSnapshot(int saveObjIndex)
				{
					SavedObject savedObject = _savedObjects[saveObjIndex];

					if (savedObject._object != null)
					{
						//Delete current snapshot
						if (savedObject._hasSnapshot)
						{
							DeleteObjectEditorPrefs(savedObject._snapshotEditorPrefKey);
						}

						//Save values to prefs
						string editorPrefKey = SaveObjectValues(savedObject);

						savedObject._hasSnapshot = true;
						savedObject._snapshotEditorPrefKey = editorPrefKey;

						_savedObjects[saveObjIndex] = savedObject;
					}
				}

				private static void ClearSnapshot(int saveObjIndex)
				{
					SavedObject savedObject = _savedObjects[saveObjIndex];

					DeleteObjectEditorPrefs(savedObject._snapshotEditorPrefKey);

					savedObject._hasSnapshot = false;
					savedObject._snapshotEditorPrefKey = null;

					_savedObjects[saveObjIndex] = savedObject;
				}
				#endregion

				#region Helper Functions
				private static int GetSceneIdentifier(Object obj)
				{
					if (obj != null)
					{
						PropertyInfo inspectorModeInfo = typeof(SerializedObject).GetProperty(kInspectorProperty, BindingFlags.NonPublic | BindingFlags.Instance);

						SerializedObject serializedObject = new SerializedObject(obj);
						inspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);

						SerializedProperty localIdProp = serializedObject.FindProperty(kIdentifierProperty);

						if (localIdProp != null && localIdProp.intValue != 0)
							return localIdProp.intValue;
					}

					return -1;
				}

				private static int GetInstanceId(Object obj)
				{
					if (obj != null)
						return obj.GetInstanceID();

					return 0;
				}

				private static bool GetActiveScene(string scenePath, out Scene scene, bool loadScene = false)
				{
					scene = new Scene();

					for (int i = 0; i < SceneManager.sceneCount; i++)
					{
						scene = SceneManager.GetSceneAt(i);

						if (scene.IsValid() && scene.path == scenePath)
						{
							if (scene.isLoaded)
							{
								return true;
							}
						}
					}

					if (loadScene)
					{
						try
						{
							scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
						}
						catch
						{
							return false;
						}
						
						return scene.IsValid();
					}
					
					return false;
				}
				
				private static Scene GetObjectScene(Object obj)
				{
					Component component = obj as Component;

					if (component != null)
					{
						return component.gameObject.scene;
					}
					else
					{
						GameObject gameObject = obj as GameObject;

						if (gameObject != null)
							return gameObject.scene;
					}
					
					return new Scene();
				}

				private static string GetScenePath(Object obj)
				{
					string scenePath = null;

					if (obj is GameObject gameObject)
					{
						scenePath = gameObject.scene.path;
					}
					else if (obj is Component component)
					{
						scenePath = component.gameObject.scene.path;
					}

					return scenePath;
				}

				private static string GetObjectPath(Object obj)
				{
					GameObject gameObject;
					
					if (obj is GameObject gameObj)
					{
						gameObject = gameObj;
					}
					else if (obj is Component component)
					{
						gameObject = component.gameObject;
					}
					else
					{
						return null;
					}

					string path = string.Empty;

					while (gameObject.transform.parent != null)
					{
						gameObject = gameObject.transform.parent.gameObject;
						path = gameObject.name + '/' + path;
					}

					path = gameObject.scene.name + ".unity/" + path;

					return path;
				}

				private static string GetObjectName(Object obj)
				{
					string objectName = null;

					if (obj is GameObject gameObject)
					{
						objectName = gameObject.name;
					}
					else if (obj is Component component)
					{
						objectName = component.gameObject.name + '.' + component.GetType().Name;
					}

					return objectName;
				}

				private static int SafeConvertToInt(string str)
				{
					int value;

					try
					{
						value = Convert.ToInt32(str);
					}
					catch
					{
						value = -1;
					}

					return value;
				}

				private static void SafeDeleteEditorPref(string key)
				{
					if (EditorPrefs.HasKey(key))
						EditorPrefs.DeleteKey(key);
				}

				private static string GetTypeString(Type type)
				{
					if (type == typeof(GameObject))
					{
						return "GameObject";
					}
					else
					{
						return type.AssemblyQualifiedName;
					}
				}

				private static Type GetType(string typeStr)
				{
					if (typeStr == "GameObject")
					{
						return typeof(GameObject);
					}
					else
					{
						return Type.GetType(typeStr);
					}
				}
				
				private static int GetPrefabChildIndex(GameObject prefabRoot, GameObject gameObject)
				{
					int index = 0;

					foreach (Transform child in gameObject.transform.parent)
					{
						if (child.gameObject == gameObject)
						{
							return index;
						}

						if (PrefabUtility.GetNearestPrefabInstanceRoot(child) == prefabRoot)
						{
							index++;
						}
					}

					return index;
				}

				private static int GetPrefabComponentIndex(GameObject prefabRoot, Component component)
				{
					Component[] components = component.gameObject.GetComponents<Component>();

					int index = 0;

					for (int i = 0; i < components.Length; i++)
					{
						if (components[i] == component)
						{
							return index;
						}

						if (components[i].GetType() == component.GetType() && PrefabUtility.GetNearestPrefabInstanceRoot(components[i]) == prefabRoot)
						{
							index++;
						}
					}

					return index;

				}

				private static Component GetPrefabComponent(GameObject prefabRoot, GameObject gameObject, Type type, int index)
				{
					if (index != -1)
					{
						Component[] components = gameObject.GetComponents<Component>();

						int count = 0;

						for (int i = 0; i < components.Length; i++)
						{
							if (PrefabUtility.GetNearestPrefabInstanceRoot(components[i]) == prefabRoot && components[i].GetType() == type)
							{
								if (count == index)
								{
									return components[i];
								}

								count++;
							}
						}
					}

					return null;
				}

				private static GameObject GetPrefabChild(GameObject prefabRoot, GameObject gameObject, int index)
				{
					if (index != -1)
					{
						int count = 0;

						foreach (Transform child in gameObject.transform)
						{
							if (PrefabUtility.GetNearestPrefabInstanceRoot(child) == prefabRoot)
							{
								if (count == index)
								{
									return child.gameObject;
								}

								count++;
							}				
						}
					}
					
					return null;
				}

				private static void ClearCache()
				{
					_savedObjects.Clear();

					int numSavedObjects = EditorPrefs.GetInt(kEditorPrefsObjectCountKey, 0);

					for (int i = 0; i < numSavedObjects; i++)
					{
						string editorPrefKey = kEditorPrefsKey + Convert.ToString(i);
						DeleteObjectEditorPrefs(editorPrefKey);
					}

					SafeDeleteEditorPref(kEditorPrefsObjectCountKey);

					_state = State.Idle;
				}


				private static bool GetChildObjectPath(string path, GameObject rootObject, Object obj, bool sceneObjectsOnly, out string fullPath)
				{
					//Check gameobject itself matches object
					if (rootObject == obj)
					{
						fullPath = path;
						return true;
					}

					//Check any of the gameobjects components matches object
					Component[] components = rootObject.GetComponents<Component>();
					int componentIndex = 0;

					for (int i = 0; i < components.Length; i++)
					{
						if (!sceneObjectsOnly || GetSceneIdentifier(obj) != -1)
						{
							if (components[i] == obj)
							{
								fullPath = path + '.' + Convert.ToString(componentIndex);
								return true;
							}

							componentIndex++;
						}
					}

					//Check any of the child's children matches object
					int childIndex = 0;

					foreach (Transform child in rootObject.transform)
					{
						//Only index scene gameobjects (not ones created at runtime)
						if (!sceneObjectsOnly || GetSceneIdentifier(obj) != -1)
						{
							string childPath = path + '[' + Convert.ToString(childIndex) + ']';

							//Check this child's children
							if (GetChildObjectPath(childPath, child.gameObject, obj, sceneObjectsOnly, out fullPath))
								return true;

							childIndex++;
						}

					}

					fullPath = string.Empty;
					return false;
				}

				private static Object GetChildObject(GameObject rootObject, string path)
				{
					if (string.IsNullOrEmpty(path))
						return rootObject;

					GameObject gameObject = rootObject;
					int j = 0;

					while (j < path.Length)
					{
						int i = path.IndexOf('[', j);

						if (i == -1)
							break;

						j = path.IndexOf(']', i);

						if (j == -1)
							break;

						string childIndexStr = path.Substring(i + 1, j - i - 1);
						int childIndex = SafeConvertToInt(childIndexStr);

						if (childIndex >= 0 && childIndex < gameObject.transform.childCount)
						{
							gameObject = gameObject.transform.GetChild(childIndex).gameObject;
						}
						else
						{
							return null;
						}
					}

					int dotIndex = path.LastIndexOf('.');

					if (dotIndex != -1)
					{
						int componentIndex = SafeConvertToInt(path.Substring(dotIndex + 1));

						Component[] components = gameObject.GetComponents<Component>();

						if (componentIndex >= 0 && componentIndex < components.Length)
						{
							return components[componentIndex];
						}
						else
						{
							return null;
						}
					}
					else
					{
						return gameObject;
					}
				}
				#endregion

				#region Editor Window
				public class UnityPlayModeSaverWindow : EditorWindow
				{
					#region Constants
					private const float kClearButtonWidth = 24f;
					private const float kClearAllButtonWidth = 220f;
					private const float kSaveModeWidth = 102f;
					private const float kSnapshotButtonWidth = 116f;
					private const float kDefaultNameWidth = 280f;
					private const float kMinNameWidth = 50f;
					private const string kWindowTitle = "Play Mode Saver";				
					private static readonly GUIContent kObjectLabel = new GUIContent("Saved Object"); 
					private static readonly GUIContent kObjectPathLabel = new GUIContent("Object Path");
					private static readonly GUIContent kNoObjectsLabel = new GUIContent("Either right click on any Game Object or Component and click 'Save Play Mode Changes'\nor drag any Game Object or Component into this window.");
					private static readonly GUIContent kObjectsDetailsLabel = new GUIContent("These objects will have their values saved upon leaving Play Mode.\nIf an object has a snapshot saved it will restore to that, otherwise it will keep the values it had upon exiting Play Mode.");
					private static readonly GUIContent kNotInEditModeLabel = new GUIContent("Not in Play Mode.");
					private static readonly string[] kSaveModeOptions = new string[] { "Save on Exit", "Snapshot" };
					private const string kDeletedObj = " <i>(Deleted)</i>";
					private const string kSceneNotLoadedObj = " <i>(Scene not loaded)</i>";
					private const string kSaveSnapshot = " Save Snapshot";
					private const string kClearAllButton = "Clear All Saved Objects";
					private static readonly float kResizerWidth = 6.0f;
					private static readonly float kTextFieldSpace = 6.0f;
					#endregion

					#region Private Data
					private Vector2 _scrollPosition = Vector2.zero;
					private float _nameWidth = kDefaultNameWidth;
					private Rect _nameResizerRect;
					private bool _needsRepaint;
					private enum ResizingState
					{
						NotResizing,
						ResizingName,
					}
					private ResizingState _resizing;
					private int _controlID;
					private float _resizingOffset;
					private GUIStyle _bottomBarInfoStyle;
					private GUIStyle _bottomBarInfoTextStyle;
					private GUIStyle _headerStyle;			
					private GUIStyle _itemNameStyle;
					private GUIStyle _itemButtonStyle;
					private GUIStyle _itemPathStyle;
					private GUIStyle _itemSpaceStyle;
					private GUIStyle _noObjectsStyle;
					
					private GUIContent _clearButtonContent;
					private GUIContent _clearAllButtonContent;
					private GUIContent _saveSnapshotContent;
					private Texture _scriptIcon;
					#endregion

					#region Public Interface
					public static UnityPlayModeSaverWindow Open(bool focus)
					{
						UnityPlayModeSaverWindow window = (UnityPlayModeSaverWindow)GetWindow(typeof(UnityPlayModeSaverWindow),
																					   false, kWindowTitle, focus);
						window.Initialize();

						if (!focus)
							window.Repaint();

						return window;
					}
					#endregion

					#region Unity Events
					private void OnGUI()
					{
						_needsRepaint = false;

						if (_currentPlayModeState == PlayModeStateChange.EnteredPlayMode)
						{
							EditorGUILayout.BeginVertical();
							{
								DrawTitleBar();
								DrawTable();
								DrawBottomButton();
							}
							EditorGUILayout.EndVertical();

							HandleInput();
						}
						else
						{
							EditorGUILayout.LabelField(kNotInEditModeLabel, _noObjectsStyle, GUILayout.ExpandHeight(true));
						}
						
						if (_needsRepaint)
							Repaint();
					}
					#endregion

					#region Private Functions
					private void Initialize()
					{
						titleContent = new GUIContent(kWindowTitle, EditorGUIUtility.IconContent("SaveAs").image);
						minSize = new Vector2(kDefaultNameWidth, kDefaultNameWidth);

						if (_headerStyle == null)
						{
							_headerStyle = new GUIStyle(EditorStyles.toolbarButton)
							{
								alignment = TextAnchor.MiddleLeft,
								padding = new RectOffset(8, 8, 0, 0),
								fontStyle = FontStyle.Italic,
							};
						}

						if (_itemNameStyle == null)
						{
							_itemNameStyle = new GUIStyle(EditorStyles.toolbarTextField)
							{
								alignment = TextAnchor.MiddleLeft,
								margin = new RectOffset(0, 0, 2, 3),
								padding = new RectOffset(8, 8, 0, 0),
								fixedHeight = 0,
								stretchHeight = true,
								stretchWidth = true,
								richText = true,
							};
						}

						if (_itemButtonStyle == null)
						{
							_itemButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
							{
								alignment = TextAnchor.MiddleLeft,
							};
						}

						if (_itemPathStyle == null)
						{
							_itemPathStyle = new GUIStyle(EditorStyles.toolbarSearchField)
							{
								fontSize = 12,
								margin = new RectOffset(0, 0, 2, 3),
								padding = new RectOffset(16, 8, 0, 0),
								fixedHeight = 0,
								stretchHeight = true,
								stretchWidth = true,
							};
						}

						if (_itemSpaceStyle == null)
						{
							_itemSpaceStyle = new GUIStyle(EditorStyles.label)
							{
								margin = new RectOffset(0, 0, 0, 0),
								padding = new RectOffset(0, 0, 0, 0),
							};
						}

						if (_noObjectsStyle == null)
						{
							_noObjectsStyle = new GUIStyle(EditorStyles.label)
							{
								alignment = TextAnchor.MiddleCenter,
								stretchWidth = true,
								stretchHeight = true
							};
						}

						if (_bottomBarInfoStyle == null)
						{
							_bottomBarInfoStyle = new GUIStyle(EditorStyles.toolbar)
							{
								fixedHeight = 42,
							};
						}

						if (_bottomBarInfoTextStyle == null)
						{
							_bottomBarInfoTextStyle = new GUIStyle(EditorStyles.toolbarButton)
							{
								alignment = TextAnchor.MiddleCenter,
								fixedHeight = 40,
								fontStyle = FontStyle.Italic
							};
						}

						_saveSnapshotContent = new GUIContent(kSaveSnapshot, EditorGUIUtility.IconContent("SaveAs").image);

						_clearButtonContent = EditorGUIUtility.IconContent("d_winbtn_win_close");

						_clearAllButtonContent = new GUIContent(kClearAllButton);

						_scriptIcon = EditorGUIUtility.IconContent("cs Script Icon").image;
					}

					private void DrawTitleBar()
					{
						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							//Clear Button
							EditorGUILayout.Space(kClearButtonWidth, false);

							//Object name
							GUILayout.Label(kObjectLabel, _headerStyle, GUILayout.Width(_nameWidth - _scrollPosition.x));

							//Name Resizer
							RenderResizer(ref _nameResizerRect);

							//Save Mode
							GUILayout.Label("Save Mode", _headerStyle, GUILayout.Width(kSaveModeWidth));

							//Snapshot buttons
							GUILayout.Label(GUIContent.none, _headerStyle, GUILayout.Width(kSnapshotButtonWidth));

							//Object Path
							GUILayout.Label(kObjectPathLabel, _headerStyle);
						}
						EditorGUILayout.EndHorizontal();
					}

					private void DrawTable()
					{
						bool origGUIenabled = GUI.enabled;

						_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false);
						{
							for (int i=0; i<_savedObjects.Count;)
							{
								bool itemRemoved = false;

								SavedObject savedObject = _savedObjects[i];

								EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
								{
									//Clear object button
									if (GUILayout.Button(_clearButtonContent, EditorStyles.toolbarButton, GUILayout.Width(kClearButtonWidth)))
									{
										itemRemoved = true;
									}

									//Spacer
									GUILayout.Label(GUIContent.none, _itemSpaceStyle, GUILayout.Width(kTextFieldSpace));

									// Object button
									{
										string name = " " + GetObjectName(savedObject);
										
										if (savedObject._object == null)
										{
											GUI.enabled = false;

											Scene scene = SceneManager.GetSceneByPath(savedObject._scenePath);

											if (!scene.IsValid() || !scene.isLoaded)
											{
												name += kSceneNotLoadedObj;
											}
											else
											{
												name += kDeletedObj;
											}
										}

										Texture icon = EditorGUIUtility.ObjectContent(null, savedObject._type).image;

										if (icon == null)
										{
											icon = _scriptIcon;
										}

										GUIContent buttonContent = new GUIContent(name, icon);

										if (GUILayout.Button(buttonContent, _itemNameStyle, GUILayout.Width(_nameWidth - kResizerWidth)))
										{
											FocusOnObject(savedObject._object);
										}

										GUI.enabled = origGUIenabled;
									}

									//Spacer
									GUILayout.Label(GUIContent.none, _itemSpaceStyle, GUILayout.Width(kResizerWidth));

									//Save Mode
									{
										int selected = savedObject._hasSnapshot ? 1 : 0;
										int newSelected = EditorGUILayout.Popup(selected, kSaveModeOptions, EditorStyles.toolbarDropDown, GUILayout.Width(kSaveModeWidth));

										if (selected != newSelected)
										{
											if (newSelected == 1)
											{
												SaveSnapshot(i);
											}
											else
											{
												ClearSnapshot(i);

												//Check objects scene is still loaded, if not remove object
												Scene scene = SceneManager.GetSceneByPath(savedObject._scenePath);

												if (!scene.IsValid() || !scene.isLoaded)
												{
													itemRemoved = true;
												}
											}
										}
									}

									//Snap shot button
									{
										if (savedObject._object == null)
										{
											GUI.enabled = false;
										}

										if (GUILayout.Button(_saveSnapshotContent, _itemButtonStyle, GUILayout.Width(kSnapshotButtonWidth)))
										{
											SaveSnapshot(i);
										}

										GUI.enabled = origGUIenabled;
									}

									//Spacer
									GUILayout.Label(GUIContent.none, _itemSpaceStyle, GUILayout.Width(kTextFieldSpace));

									//Object path
									if (GUILayout.Button(savedObject._path + GetObjectName(savedObject), _itemPathStyle, GUILayout.ExpandWidth(true)))
									{
										FocusOnObject(savedObject._object);
									}

									GUILayout.Label(GUIContent.none, _itemSpaceStyle, GUILayout.Width(kTextFieldSpace));
								}
								EditorGUILayout.EndHorizontal();

								if (itemRemoved)
								{
									ClearSavedObject(i);
									_needsRepaint = true;
								}
								else
								{
									i++;
								}
							}

							if (_savedObjects.Count == 0)
							{
								EditorGUILayout.LabelField(kNoObjectsLabel, _noObjectsStyle, GUILayout.ExpandHeight(true));
							}
						}
						EditorGUILayout.EndScrollView();
					}

					private void DrawBottomButton()
					{
						EditorGUILayout.BeginHorizontal(_bottomBarInfoStyle);
						{
							GUILayout.Label(kObjectsDetailsLabel, _bottomBarInfoTextStyle);
						}
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							GUILayout.FlexibleSpace();

							if (GUILayout.Button(_clearAllButtonContent, GUILayout.Width(kClearAllButtonWidth)))
							{
								ClearCache();
								_needsRepaint = true;
							}

							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndHorizontal();
					}

					private void HandleInput()
					{
						Event inputEvent = Event.current;

						if (inputEvent == null)
							return;

						EventType controlEventType = inputEvent.GetTypeForControl(_controlID);

						if (_resizing != ResizingState.NotResizing && inputEvent.rawType == EventType.MouseUp)
						{
							_resizing = ResizingState.NotResizing;
							_needsRepaint = true;
						}

						switch (controlEventType)
						{
							case EventType.MouseDown:
								{
									if (inputEvent.button == 0)
									{
										if (_nameResizerRect.Contains(inputEvent.mousePosition))
										{
											_resizing = ResizingState.ResizingName;
										}
										else
										{
											_resizing = ResizingState.NotResizing;
										}

										if (_resizing != ResizingState.NotResizing)
										{
											inputEvent.Use();
											_resizingOffset = inputEvent.mousePosition.x;
										}
									}
								}
								break;

							case EventType.MouseUp:
								{
									if (_resizing != ResizingState.NotResizing)
									{
										inputEvent.Use();
										_resizing = ResizingState.NotResizing;
									}
								}
								break;

							case EventType.MouseDrag:
								{
									if (_resizing != ResizingState.NotResizing)
									{
										if (_resizing == ResizingState.ResizingName)
										{
											_nameWidth += (inputEvent.mousePosition.x - _resizingOffset);
											_nameWidth = Math.Max(_nameWidth, kMinNameWidth);
										}

										_resizingOffset = inputEvent.mousePosition.x;
										_needsRepaint = true;
									}
								}
								break;
							case EventType.DragUpdated:
								{
									bool objectsAreAllowed = true;

									foreach (Object obj in DragAndDrop.objectReferences)
									{
										if (!(obj is GameObject) && !(obj is Component))
										{
											objectsAreAllowed = false;
											break;
										}
									}

									DragAndDrop.visualMode = objectsAreAllowed ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
								}
								break;

							case EventType.DragPerform:
								{
									foreach (Object obj in DragAndDrop.objectReferences)
									{
										if (!IsObjectRegistered(obj, out _))
										{
											if (obj is GameObject gameObject)
											{
												RegisterSavedObject(gameObject);
											}
											else if (obj is Component component)
											{
												RegisterSavedObject(component);
											}
										}
									}

									DragAndDrop.AcceptDrag();
								}
								break;
						}
					}

					private void RenderResizer(ref Rect rect)
					{
						GUILayout.Box(GUIContent.none, EditorStyles.toolbar, GUILayout.Width(kResizerWidth), GUILayout.ExpandHeight(true));
						rect = GUILayoutUtility.GetLastRect();
						EditorGUIUtility.AddCursorRect(rect, MouseCursor.SplitResizeLeftRight);
					}

					private static void FocusOnObject(Object obj)
					{
						Selection.activeObject = obj;
						SceneView.FrameLastActiveSceneView();
						EditorGUIUtility.PingObject(obj);
					}

					private string GetObjectName(SavedObject savedObject)
					{
						if (savedObject._object == null)
						{
							return savedObject._name;
						}
						else
						{
							return UnityPlayModeSaver.GetObjectName(savedObject._object);
						}
					}
					#endregion
				}
				#endregion
			}
		}
	}
}

#endif