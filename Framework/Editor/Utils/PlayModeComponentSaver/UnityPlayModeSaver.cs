#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
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
				private const string kSaveMenuString = "\u2714  Save Play Mode Changes";
				private const string kRevertMenuString = "\u2716  Forget Play Mode Changes";

				private const string kSaveComponentMenuString = "CONTEXT/Component/" + kSaveMenuString;
				private const string kRevertComponentMenuString = "CONTEXT/Component/" + kRevertMenuString;
				private const string kSaveGameObjectMenuString = "GameObject/" + kSaveMenuString;
				private const string kRevertGameObjectMenuString = "GameObject/" + kRevertMenuString;
				private const string kClearCacheMenuString = "Tools/Clear PlayModeSaver Cache";

				private const string kUndoText = "Play Mode Changes";
				private const string kEditorPrefsKey = "UnityPlayModeSaver.";
				private const string kEditorPrefsObjectCountKey = kEditorPrefsKey + "SavedObjects";

				private const string kEditorPrefsObjectScene = ".Scene";
				private const string kEditorPrefsObjectSceneId = ".SceneId";
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

				private const char kItemSplitChar = '?';
				private const char kObjectPathSplitChar = ':';
				private const string kIdentifierProperty = "m_LocalIdentfierInFile";   //note the misspelling!
				private const string kInspectorProperty = "inspectorMode";
				#endregion

				#region Helper Structs
				private struct RestoredObjectData
				{
					public Object _object;
					public Type _createdObjectType;
					public Object _parentObject;
					public string _json;
					public string _scenePath;
					public string _missingObjectRefs;
					public string _missingMaterials;
				}

				private struct ObjectRefProperty
				{
					public string _propertyPath;
					public int _objectId;	
				}

				private struct MaterialRef
				{
					public string _propertyPath;
					public Material _material;
				}

				private enum State
				{ 
					Idle,
					Busy,
				}
				private static State _state;
				#endregion
				
				#region Constructor
				static UnityPlayModeSaver()
				{
					EditorApplication.playModeStateChanged += OnModeChanged;
					EditorSceneManager.sceneSaving += OnSceneSaved;
					ClearCache();
				}
				#endregion
				
				#region Menu Functions
				[MenuItem(kSaveComponentMenuString, false, 12)]
				public static void SaveComponent(MenuCommand command)
				{
					Component component = command.context as Component;

					if (Application.isPlaying && component != null)
						RegisterSavedObject(component, component.gameObject.scene.path);
				}

				[MenuItem(kSaveComponentMenuString, true)]
				public static bool ValidateSaveComponent(MenuCommand command)
				{
					Component component = command.context as Component;

					if (Application.isPlaying && component != null)
						return !IsObjectRegistered(component, component.gameObject.scene.path);

					return false;
				}

				[MenuItem(kRevertComponentMenuString, false, 12)]
				public static void RevertComponent(MenuCommand command)
				{
					Component component = command.context as Component;

					if (Application.isPlaying && component != null)
						UnregisterObject(component, component.gameObject.scene.path);
				}

				[MenuItem(kRevertComponentMenuString, true)]
				public static bool ValidateRevertComponent(MenuCommand command)
				{
					Component component = command.context as Component;

					if (Application.isPlaying && component != null)
						return IsObjectRegistered(component, component.gameObject.scene.path);

					return false;
				}

				[MenuItem(kSaveGameObjectMenuString, false, -100)]
				public static void SaveGameObject()
				{
					if (Application.isPlaying && Selection.gameObjects != null)
					{
						foreach (GameObject go in Selection.gameObjects)
						{
							RegisterSavedObject(go, go.scene.path);
						}
					}	
				}

				[MenuItem(kSaveGameObjectMenuString, true)]
				public static bool ValidateSaveGameObject()
				{
					if (Application.isPlaying && Selection.gameObjects != null)
					{
						foreach (GameObject go in Selection.gameObjects)
						{
							if (!IsObjectRegistered(go, go.scene.path))
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
							UnregisterObject(go, go.scene.path);
						}
					}
				}

				[MenuItem(kRevertGameObjectMenuString, true)]
				public static bool ValidateRevertGameObject()
				{
					if (Application.isPlaying && Selection.gameObjects != null)
					{
						foreach (GameObject go in Selection.gameObjects)
						{
							if (IsObjectRegistered(go, go.scene.path))
								return true;
						}
					}

					return false;
				}


				[MenuItem(kClearCacheMenuString)]
				public static void ClearCache()
				{
					int numSavedObjects = EditorPrefs.GetInt(kEditorPrefsObjectCountKey, 0);

					for (int i = 0; i < numSavedObjects; i++)
					{
						string editorPrefKey = kEditorPrefsKey + Convert.ToString(i);
						DeleteObjectEditorPrefs(editorPrefKey);
					}

					SafeDeleteEditorPref(kEditorPrefsObjectCountKey);

					_state = State.Idle;
				}
				#endregion

				#region Editor Functions
				private static void OnModeChanged(PlayModeStateChange state)
				{
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
				private static bool IsObjectRegistered(Object obj, string scenePath)
				{
					int identifier = GetSceneIdentifier(obj);

					if (identifier != -1)
					{
						if (GetSavedSceneObjectIndex(identifier, scenePath) != -1)
							return true;
					}
					else
					{
						int instanceId = GetInstanceId(obj);

						if (GetSavedRuntimeObjectIndex(instanceId, scenePath) != -1)
							return true;
					}

					return false;
				}
				
				private static void RegisterSavedObject(Object obj, string scenePath)
				{
					int identifier = GetSceneIdentifier(obj);
					
					if (identifier != -1)
					{
						RegisterSceneObject(obj, scenePath, identifier);
					}
					else
					{
						int instanceId = GetInstanceId(obj);
						RegisterRuntimeObject(scenePath, instanceId);
					}
				}

				private static void UnregisterObject(Object obj, string scenePath)
				{
					int identifier = GetSceneIdentifier(obj);

					if (identifier != -1)
					{
						UnregisterSceneObject(scenePath, identifier);
					}
					else
					{
						int instanceId = GetInstanceId(obj);
						UnregisterRuntimeObject(scenePath, instanceId);
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

				#region Scene Objects
				private static string RegisterSceneObject(Object obj, string scenePath, int identifier)
				{
					GameObject prefab;
					int prefabSceneId;

					//Check scene object is a prefab instance...
					if (IsScenePrefab(obj, scenePath, out prefab, out prefabSceneId))
					{
						return RegisterScenePrefabObject(scenePath, identifier, prefab, prefabSceneId, obj);
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

				private static void UnregisterSceneObject(string scenePath, int identifier)
				{
					int saveObjIndex = GetSavedSceneObjectIndex(identifier, scenePath);

					if (saveObjIndex != -1)
					{
						string editorPrefKey = kEditorPrefsKey + Convert.ToString(saveObjIndex);
						DeleteObjectEditorPrefs(editorPrefKey);
					}
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

				private static void UnregisterRuntimeObject(string scenePath, int instanceId)
				{
					int saveObjIndex = GetSavedRuntimeObjectIndex(instanceId, scenePath);

					if (saveObjIndex != -1)
					{
						string editorPrefKey = kEditorPrefsKey + Convert.ToString(saveObjIndex);
						DeleteObjectEditorPrefs(editorPrefKey);
					}
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

					int numSavedObjects = EditorPrefs.GetInt(kEditorPrefsObjectCountKey, 0);

					for (int i = 0; i < numSavedObjects; i++)
					{
						SaveObjectValues(i);

						//Saving object values can result in more objects being added to list so re-grab the count.
						numSavedObjects = EditorPrefs.GetInt(kEditorPrefsObjectCountKey, 0);
					}

					_state = State.Idle;
				}

				private static void SaveObjectValues(int saveObjIndex)
				{
					string editorPrefKey = kEditorPrefsKey + Convert.ToString(saveObjIndex);
					string sceneStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectScene);

					//Scene object
					if (EditorPrefs.HasKey(editorPrefKey + kEditorPrefsObjectSceneId))
					{
						int identifier = EditorPrefs.GetInt(editorPrefKey + kEditorPrefsObjectSceneId, -1);

						Object obj = FindSceneObject(sceneStr, identifier);

						if (obj != null)
						{
							SaveObjectValues(editorPrefKey, obj);

							//If its a GameObject then add its components and child GameObjects (some of which might be runtime)
							if (obj is GameObject gameObject)
							{
								AddSceneGameObjectChildObjectValues(gameObject);
							}
						}
					}
					//Runtime object
					else if (EditorPrefs.HasKey(editorPrefKey + kEditorPrefsRuntimeObjectId))
					{
						int instanceId = EditorPrefs.GetInt(editorPrefKey + kEditorPrefsRuntimeObjectId, -1);

						Object obj = FindRuntimeObject(sceneStr, instanceId);

						if (obj != null)
						{
							GameObject sceneParent;
							GameObject topOfHieracy;

							if (obj is Component component)
							{
								topOfHieracy = component.gameObject;
								FindRuntimeObjectParent(component.gameObject, out sceneParent, ref topOfHieracy);

								//If the new component belongs to a scene object, just save the new component
								if (component.gameObject == sceneParent)
								{
									SaveRuntimeComponent(editorPrefKey, component, sceneParent);
								}
								//Otherwise need to save the whole new gameobject hierarchy
								else
								{
									SaveRuntimeGameObject(editorPrefKey, topOfHieracy, sceneParent, null);
								}
							}
							else if (obj is GameObject gameObject)
							{
								topOfHieracy = gameObject;
								FindRuntimeObjectParent(gameObject, out sceneParent, ref topOfHieracy);

								if (topOfHieracy != gameObject)
								{
									EditorPrefs.SetInt(editorPrefKey + kEditorPrefsRuntimeObjectId, GetInstanceId(topOfHieracy));
								}

								SaveRuntimeGameObject(editorPrefKey, topOfHieracy, sceneParent, null);
							}
						}
					}
				}

				private static void SaveObjectValues(string editorPrefKey, Object obj)
				{
					RestoredObjectData data = GetObjectData(obj);

					EditorPrefs.SetString(editorPrefKey + kEditorPrefsObjectJson, data._json);
					EditorPrefs.SetString(editorPrefKey + kEditorPrefsObjectRefs, data._missingObjectRefs);
					EditorPrefs.SetString(editorPrefKey + kEditorPrefsObjectMaterialRefs, data._missingMaterials);
				}

				private static bool ShouldUseEditorSerialiser(Object obj)
				{
					if ((obj is Component && !(obj is MonoBehaviour)) || (obj is GameObject))
						return true;

					return false;
				}

				private static RestoredObjectData GetObjectData(Object obj)
				{
					RestoredObjectData data = new RestoredObjectData();

					bool unityType = ShouldUseEditorSerialiser(obj);

					List<string> materials = new List<string>();
					List<ObjectRefProperty> objectProperties = unityType ? new List<ObjectRefProperty>() : null;

					GetSpecialCaseProperties(obj, materials, objectProperties);

					//If Component is a Unity in built type we have to restore any scene links as they won't be serialized by EditorJsonUtility
					if (unityType)
					{
						data._json = EditorJsonUtility.ToJson(obj);

						//Build missing object refs string
						data._missingObjectRefs = "";

						foreach (ObjectRefProperty prop in objectProperties)
						{
							if (!string.IsNullOrEmpty(data._missingObjectRefs))
								data._missingObjectRefs += kItemSplitChar;

							data._missingObjectRefs += Convert.ToString(prop._objectId) + kObjectPathSplitChar + prop._propertyPath;
						}
					}
					else
					{
						data._json = JsonUtility.ToJson(obj);
						data._missingObjectRefs = "";
					}

					//Build missing materials string
					data._missingMaterials = "";

					foreach (string material in materials)
					{
						if (!string.IsNullOrEmpty(data._missingMaterials))
							data._missingMaterials += kItemSplitChar;

						data._missingMaterials += material;
					}

					return data;
				}

				private static void GetSpecialCaseProperties(Object obj, List<string> materials, List<ObjectRefProperty> objectProperties)
				{
					SerializedObject serializedObject = new SerializedObject(obj);
					SerializedProperty propertry = serializedObject.GetIterator();

					while (propertry.NextVisible(true))
					{
						//Store material properties that now point at a runtime instance of a material (they will get reverted to original values)
						if (propertry.type == "PPtr<Material>")
						{
							if (propertry.objectReferenceValue != null && propertry.objectReferenceValue.name.EndsWith("(Instance)"))
								materials.Add(propertry.propertyPath);
						}
						//Save any object ptr properties that point at scene objects
						else if (objectProperties != null && propertry.type.StartsWith("PPtr<"))
						{
							//Only store the object if the reference is within the same scene 
							Scene objScne = GetObjectScene(obj);

							if (objScne.IsValid() && objScne == GetObjectScene(propertry.objectReferenceValue))
							{
								int objId = GetSceneIdentifier(propertry.objectReferenceValue);

								if (objId != -1)
								{
									ObjectRefProperty objRef = new ObjectRefProperty
									{
										_objectId = objId,
										_propertyPath = propertry.propertyPath
									};
									objectProperties.Add(objRef);
								}
							}
						}
					}
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

				private static void AddSceneGameObjectChildObjectValues(GameObject gameObject)
				{
					Component[] components = gameObject.GetComponents<Component>();

					//Save each component
					for (int i = 0; i < components.Length; i++)
					{
						int identifier = GetSceneIdentifier(components[i]);

						if (identifier != -1)
						{
							//Need to check is scene prefab instance
							string editorPrefKey = RegisterSceneObject(components[i], gameObject.scene.path, identifier);
							SaveObjectValues(editorPrefKey, components[i]);
						}
						else
						{
							int instanceId = GetInstanceId(components[i]);
							string scenePath = gameObject.scene.path;
							string editorPrefKey = RegisterRuntimeObject(scenePath, instanceId);
							SaveRuntimeComponent(editorPrefKey, components[i], gameObject);
						}
					}

					//Save each child object
					foreach (Transform child in gameObject.transform)
					{
						int identifier = GetSceneIdentifier(child.gameObject);

						if (identifier != -1)
						{
							//Need to check is scene prefab instance
							string editorPrefKey = RegisterSceneObject(child.gameObject, child.gameObject.scene.path, identifier);
							SaveObjectValues(editorPrefKey, child.gameObject);
							AddSceneGameObjectChildObjectValues(child.gameObject);
						}
						else
						{
							int instanceId = GetInstanceId(child.gameObject);
							string scenePath = gameObject.scene.path;
							string editorPrefKey = RegisterRuntimeObject(scenePath, instanceId);
							SaveRuntimeGameObject(editorPrefKey, child.gameObject, gameObject, null);
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
							return GetScenePrefabChildObject(prefabInstance, prefabObjPath);
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

				private static void SaveRuntimeGameObject(string editorPrefKey, GameObject gameObject, GameObject parentSceneObject, GameObject parentPrefab)
				{
					EditorPrefs.SetString(editorPrefKey + kEditorPrefsRuntimeObjectType, GetTypeString(gameObject.GetType()));
					SaveObjectValues(editorPrefKey, gameObject);

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

						SaveRuntimeComponent(editorPrefKey + "." + Convert.ToString(childObjectIndex), components[i], null);

						if (isPartOfCurrentPrefabHieracy)
							EditorPrefs.SetInt(editorPrefKey + "." + Convert.ToString(childObjectIndex) + kEditorPrefsRuntimeObjectPrefabObjIndex, GetPrefabComponentIndex(parentPrefab, components[i]));
					
						childObjectIndex++;
					}
					
					foreach (Transform child in gameObject.transform)
					{
						bool isPartOfCurrentPrefabHieracy = parentPrefab != null && PrefabUtility.GetNearestPrefabInstanceRoot(child.gameObject) == parentPrefab;

						SaveRuntimeGameObject(editorPrefKey + "." + Convert.ToString(childObjectIndex), child.gameObject, null, isPartOfCurrentPrefabHieracy ? parentPrefab : null);

						if (isPartOfCurrentPrefabHieracy)
							EditorPrefs.SetInt(editorPrefKey + "." + Convert.ToString(childObjectIndex) + kEditorPrefsRuntimeObjectPrefabObjIndex, GetPrefabChildIndex(parentPrefab, child.gameObject));

						childObjectIndex++;
					}
				}

				private static void SaveRuntimeComponent(string editorPrefKey, Component component, GameObject parentSceneObject)
				{
					EditorPrefs.SetString(editorPrefKey + kEditorPrefsRuntimeObjectType, GetTypeString(component.GetType()));
					SaveObjectValues(editorPrefKey, component);
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

						if (EditorPrefs.HasKey(editorPrefKey + kEditorPrefsObjectScene))
						{
							string sceneStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectScene);

							//Scene object
							if (EditorPrefs.HasKey(editorPrefKey + kEditorPrefsObjectSceneId))
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

										RestoredObjectData data = new RestoredObjectData
										{
											_object = obj,
											_json = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectJson),
											_scenePath = sceneStr,
											_missingObjectRefs = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectRefs),
											_missingMaterials = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectMaterialRefs)
										};
										restoredObjectsData.Add(data);
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

										RestoredObjectData data = new RestoredObjectData
										{
											_object = obj,
											_json = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectJson),
											_scenePath = sceneStr,
											_missingObjectRefs = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectRefs),
											_missingMaterials = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectMaterialRefs)
										};
										restoredObjectsData.Add(data);
									}
								}
							}
							//Runtime Object
							else
							{
								string typeStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsRuntimeObjectType);

								Type objType = GetType(typeStr);
								GameObject parentObj = GetRuntimeObjectParent(editorPrefKey, sceneStr, true) as GameObject;

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

									//Restore the game objects data
									RestoredObjectData data = new RestoredObjectData
									{
										_object = gameObject,
										_json = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectJson),
										_scenePath = sceneStr,
										_missingObjectRefs = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectRefs),
										_missingMaterials = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectMaterialRefs)
									};
									RestoreObjectFromData(data);

									//Then restore all its children
									gameObject = RestoreRuntimeGameObject(gameObject, editorPrefKey, sceneStr, isPrefab ? gameObject : null);

									Undo.RegisterCreatedObjectUndo(gameObject, kUndoText);
								}
								else if (typeof(Component).IsAssignableFrom(objType))
								{
									if (parentObj != null)
									{
										RestoredObjectData data = new RestoredObjectData
										{
											_object = null,
											_createdObjectType = objType,
											_parentObject = parentObj,
											_json = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectJson),
											_scenePath = sceneStr,
											_missingObjectRefs = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectRefs),
											_missingMaterials = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectMaterialRefs)
										};

										restoredObjectsData.Add(data);
									}
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

						//Revert any lost material refs
						ApplyMaterialsRefs(data._object, materialRefs);
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
				
				#region Runtime Objects
				private static GameObject RestoreRuntimeGameObject(GameObject gameObject, string editorPrefKey, string sceneStr, GameObject prefabRoot)
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
								obj = GetPrefabComponent(prefabRoot, gameObject, objType, componentIndex);
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
									prefabRoot = childGameObject;
								}
							}

							//If a prefab child..
							if (EditorPrefs.HasKey(childeditorPrefKey + kEditorPrefsRuntimeObjectPrefabObjIndex))
							{
								int childGameObjectIndex = EditorPrefs.GetInt(childeditorPrefKey + kEditorPrefsRuntimeObjectPrefabObjIndex, -1);
								childGameObject = GetPrefabChild(prefabRoot, gameObject, childGameObjectIndex);
							}

							if (childGameObject == null)
							{
								childGameObject = new GameObject();
								childGameObject.transform.parent = gameObject.transform;
							}

							obj = childGameObject;
							RestoreRuntimeGameObject(childGameObject, childeditorPrefKey, sceneStr, prefabRoot);
						}

						RestoredObjectData data = new RestoredObjectData
						{
							_object = obj,
							_json = EditorPrefs.GetString(childeditorPrefKey + kEditorPrefsObjectJson),
							_scenePath = sceneStr,
							_missingObjectRefs = EditorPrefs.GetString(childeditorPrefKey + kEditorPrefsObjectRefs),
							_missingMaterials = EditorPrefs.GetString(childeditorPrefKey + kEditorPrefsObjectMaterialRefs)
						};

						RestoreObjectFromData(data);

						DeleteObjectEditorPrefs(childeditorPrefKey);

						childIndex++;
					}

					return gameObject;
				}

				private static void DeleteObjectEditorPrefs(string editorPrefKey)
				{
					SafeDeleteEditorPref(editorPrefKey + kEditorPrefsObjectScene);
					SafeDeleteEditorPref(editorPrefKey + kEditorPrefsObjectSceneId);
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
					string path;

					GetScenePrefabChildObjectPath(string.Empty, prefab, obj, out path);

					return path;
				}

				private static bool GetScenePrefabChildObjectPath(string path, GameObject prefabGameObject, Object obj, out string fullPath)
				{
					//Check gameobject itself matches object
					if (prefabGameObject == obj)
					{
						fullPath = path;
						return true;
					}

					//Check any of the gameobjects components matches object
					Component[] components = prefabGameObject.GetComponents<Component>();
					int componentIndex = 0;

					for (int i = 0; i < components.Length; i++)
					{
						//Only index scene components (not ones created at runtime)
						int identifier = GetSceneIdentifier(obj);

						if (identifier != -1)
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

					foreach (Transform child in prefabGameObject.transform)
					{
						//Only index scene gameobjects (not ones created at runtime)
						int identifier = GetSceneIdentifier(child.gameObject);

						if (identifier != -1)
						{
							string childPath = path + '[' + Convert.ToString(childIndex) + ']';

							//Check this child's children
							if (GetScenePrefabChildObjectPath(childPath, child.gameObject, obj, out fullPath))
								return true;

							childIndex++;
						}

					}

					fullPath = string.Empty;
					return false;
				}

				private static Object GetScenePrefabChildObject(GameObject prefabGameObject, string path)
				{
					if (string.IsNullOrEmpty(path))
						return prefabGameObject;
					
					GameObject gameObject = prefabGameObject;
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
				#endregion
			}
		}
	}
}

#endif