using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

using System.Reflection;
using System.Collections.Generic;

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

				private const string kUndoText = "Set Play Mode Values";
				private const string kEditorPrefsKey = "UnityPlayModeSaver.";
				private const string kEditorPrefsObjectCountKey = kEditorPrefsKey + "SavedObjects";
				private const string kEditorPrefsObjectScene = ".Scene";
				private const string kEditorPrefsObjectId = ".Id";
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
				#endregion
				
				#region Constructor
				static UnityPlayModeSaver()
				{
					EditorApplication.playModeStateChanged += OnModeChanged;
				}
				#endregion
				
				#region Menu Functions
				[MenuItem(kSaveComponentMenuString, false, 12)]
				private static void SaveComponent(MenuCommand command)
				{
					Component component = command.context as Component;

					if (Application.isPlaying && component != null)
					{
						SaveComponent(component);
					}
				}

				[MenuItem(kSaveComponentMenuString, true)]
				private static bool ValidateSaveComponent(MenuCommand command)
				{
					Component component = command.context as Component;

					if (Application.isPlaying && component != null)
					{
						int id = GetLocalIdentifier(component);

						if (id != -1 && GetSavedObjectIndex(id, component.gameObject.scene.path) == -1)
						{
							return true;
						}
					}

					return false;
				}

				[MenuItem(kRevertComponentMenuString, false, 12)]
				private static void RevertComponent(MenuCommand command)
				{
					Component component = command.context as Component;

					if (Application.isPlaying && component != null)
					{
						RevertComponent(component);
					}
				}

				[MenuItem(kRevertComponentMenuString, true)]
				private static bool ValidateRevertComponent(MenuCommand command)
				{
					Component component = command.context as Component;

					if (Application.isPlaying && component != null)
					{
						int identifier = GetLocalIdentifier(component);

						if (identifier != -1)
						{
							string scenePath = component.gameObject.scene.path;

							if (GetSavedObjectIndex(identifier, scenePath) != -1)
								return true;
						}
					}

					return false;
				}

				[MenuItem(kSaveGameObjectMenuString, false, -1)]
				private static void SaveGameObject(MenuCommand command)
				{
					GameObject gameObject = command.context as GameObject;

					if (Application.isPlaying && gameObject != null)
					{
						SaveGameObject(gameObject);
					}
				}

				[MenuItem(kSaveGameObjectMenuString, true)]
				private static bool ValidateSaveGameObject(MenuCommand command)
				{
					GameObject gameObject = command.context as GameObject;
					
					if (Application.isPlaying && gameObject != null)
					{
						int id = GetLocalIdentifier(gameObject);

						if (id != -1 && GetSavedObjectIndex(id, gameObject.scene.path) == -1)
						{
							return true;
						}
					}

					return false;
				}

				[MenuItem(kRevertGameObjectMenuString, false, -1)]
				private static void RevertGameObject(MenuCommand command)
				{
					GameObject gameObject = command.context as GameObject;

					if (Application.isPlaying && gameObject != null)
					{
						RevertGameObject(gameObject);
					}
				}

				[MenuItem(kRevertGameObjectMenuString, true)]
				private static bool ValidateRevertGameObject(MenuCommand command)
				{
					GameObject gameObject = command.context as GameObject;

					if (Application.isPlaying && gameObject != null)
					{
						int identifier = GetLocalIdentifier(gameObject);

						if (identifier != -1)
						{
							string scenePath = gameObject.scene.path;

							if (GetSavedObjectIndex(identifier, scenePath) != -1)
								return true;
						}
					}

					return false;
				}
				#endregion

				#region Component Functions
				private static void SaveComponent(Component component)
				{
					int identifier = GetLocalIdentifier(component);

					if (identifier != -1)
					{
						SaveObject(component.gameObject.scene.path, identifier);
					}
				}

				private static void RevertComponent(Component component)
				{
					int identifier = GetLocalIdentifier(component);

					if (identifier != -1)
					{
						RevertObject(component.gameObject.scene.path, identifier);
					}
				}
				#endregion

				#region GameObjects Functions
				private static void SaveGameObject(GameObject gameObject)
				{
					int identifier = GetLocalIdentifier(gameObject);

					if (identifier != -1)
					{
						SaveObject(gameObject.scene.path, identifier);
					}
				}

				private static void RevertGameObject(GameObject gameObject)
				{
					int identifier = GetLocalIdentifier(gameObject);

					if (identifier != -1)
					{
						RevertObject(gameObject.scene.path, identifier);
					}
				}

				private static void AddGameObjectChildObjectValues(GameObject gameObject)
				{
					Component[] components = gameObject.GetComponents<Component>();

					for (int i = 0; i < components.Length; i++)
					{
						int identifier = GetLocalIdentifier(components[i]);

						if (identifier != -1)
						{
							SaveObject(gameObject.scene.path, identifier);
							SaveObjectValues(components[i], identifier);
						}
					}

					foreach (Transform child in gameObject.transform)
					{
						int identifier = GetLocalIdentifier(child.transform);

						if (identifier != -1)
						{
							SaveObject(gameObject.scene.path, identifier);
							SaveObjectValues(child.transform, identifier);
						}
					}
				}
				#endregion

				#region Private Functions
				private static void OnModeChanged(PlayModeStateChange state)
				{
					switch (state)
					{
						case PlayModeStateChange.ExitingPlayMode:
							{
								SaveObjectValues();
							}
							break;
						case PlayModeStateChange.EnteredEditMode:
							{
								RestoreSavedObjects();
								RepaintEditorWindows();
							}
							break;
						default: break;
					}
				}

				private static void SaveObject(string scenePath, int identifier)
				{
					int saveObjIndex = GetSavedObjectIndex(identifier, scenePath);

					//If the component isn't already in saved object list add it
					if (saveObjIndex == -1)
					{
						int objectCount = EditorPrefs.GetInt(kEditorPrefsObjectCountKey);
						saveObjIndex = objectCount;
						objectCount++;
						EditorPrefs.SetInt(kEditorPrefsObjectCountKey, objectCount);
					}

					string editorPrefKey = kEditorPrefsKey + System.Convert.ToString(saveObjIndex);

					EditorPrefs.SetString(editorPrefKey + kEditorPrefsObjectScene, scenePath);
					EditorPrefs.SetString(editorPrefKey + kEditorPrefsObjectId, System.Convert.ToString(identifier));
				}

				private static void SaveObjectValues(int saveObjIndex)
				{
					string editorPrefKey = kEditorPrefsKey + System.Convert.ToString(saveObjIndex);

					if (EditorPrefs.HasKey(editorPrefKey + kEditorPrefsObjectScene))
					{
						string sceneStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectScene);
						string identifierStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectId);

						Object obj = FindObject(sceneStr, SafeConvertToInt(identifierStr));
						SaveObjectValues(obj, saveObjIndex);
					}
				}

				private static void SaveObjectValues(Object obj, int saveObjIndex)
				{
					RestoredObjectData data = GetObjectData(obj);

					string editorPrefKey = kEditorPrefsKey + System.Convert.ToString(saveObjIndex);

					EditorPrefs.SetString(editorPrefKey + kEditorPrefsObjectJson, data._json);
					EditorPrefs.SetString(editorPrefKey + kEditorPrefsObjectRefs, data._missingObjectRefs);
					EditorPrefs.SetString(editorPrefKey + kEditorPrefsObjectMaterialRefs, data._missingMaterials);

					if (obj is GameObject)
					{
						AddGameObjectChildObjectValues((GameObject)obj);
					}
				}

				private static void SaveObjectValues()
				{
					int numSavedObjects = EditorPrefs.GetInt(kEditorPrefsObjectCountKey, 0);

					for (int i = 0; i < numSavedObjects; i++)
					{
						SaveObjectValues(i);

						//Saving object values can result in more objects being added to list so re-grab the count.
						numSavedObjects = EditorPrefs.GetInt(kEditorPrefsObjectCountKey, 0);
					}
				}

				private static void RevertObject(string scenePath, int identifier)
				{
					int saveObjIndex = GetSavedObjectIndex(identifier, scenePath);

					if (saveObjIndex != -1)
					{
						string editorPrefKey = kEditorPrefsKey + System.Convert.ToString(saveObjIndex);

						SafeDeleteEditorPref(editorPrefKey + kEditorPrefsObjectScene);
						SafeDeleteEditorPref(editorPrefKey + kEditorPrefsObjectId);
						SafeDeleteEditorPref(editorPrefKey + kEditorPrefsObjectJson);
						SafeDeleteEditorPref(editorPrefKey + kEditorPrefsObjectRefs);
						SafeDeleteEditorPref(editorPrefKey + kEditorPrefsObjectMaterialRefs);
					}
				}

				private static void RestoreSavedObjects()
				{
					int numSavedObjects = EditorPrefs.GetInt(kEditorPrefsObjectCountKey, 0);

					List<Object> restoredObjects = new List<Object>();
					List<RestoredObjectData> restoredObjectsData = new List<RestoredObjectData>();

					for (int i = 0; i < numSavedObjects; i++)
					{
						string editorPrefKey = kEditorPrefsKey + System.Convert.ToString(i);

						if (EditorPrefs.HasKey(editorPrefKey + kEditorPrefsObjectScene))
						{
							string sceneStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectScene);
							string identifierStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectId);

							Object obj = FindObject(sceneStr, SafeConvertToInt(identifierStr));

							if (obj != null)
							{
								string jsonStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectJson);
								string objectRefStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectRefs);
								string materialStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectMaterialRefs);

								restoredObjects.Add(obj);

								RestoredObjectData data = new RestoredObjectData
								{
									_json = jsonStr,
									_scenePath = sceneStr,
									_missingObjectRefs = objectRefStr,
									_missingMaterials = materialStr
								};

								restoredObjectsData.Add(data);
							}
						}	

						SafeDeleteEditorPref(editorPrefKey + kEditorPrefsObjectScene);
						SafeDeleteEditorPref(editorPrefKey + kEditorPrefsObjectId);
						SafeDeleteEditorPref(editorPrefKey + kEditorPrefsObjectJson);
						SafeDeleteEditorPref(editorPrefKey + kEditorPrefsObjectRefs);
						SafeDeleteEditorPref(editorPrefKey + kEditorPrefsObjectMaterialRefs);
					}

					if (restoredObjects.Count > 0)
					{
						Undo.RecordObjects(restoredObjects.ToArray(), kUndoText);

						for (int i = 0; i < restoredObjects.Count; i++)
						{
							RestoreObjectFromData(restoredObjects[i], restoredObjectsData[i]);
						}
					}

					EditorPrefs.DeleteKey(kEditorPrefsObjectCountKey);
				}

				private static int GetSavedObjectIndex(int localIdentifier, string scenePath)
				{
					int numSavedObjects = EditorPrefs.GetInt(kEditorPrefsObjectCountKey, 0);

					for (int i = 0; i < numSavedObjects; i++)
					{
						string editorPrefKey = kEditorPrefsKey + System.Convert.ToString(i);

						string sceneStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectScene);
						string identifierStr = EditorPrefs.GetString(editorPrefKey + kEditorPrefsObjectId);

						if (sceneStr == scenePath && localIdentifier == SafeConvertToInt(identifierStr))
						{
							return i;
						}
					}

					return -1;
				}
				
				private static void RestoreObjectFromData(Object obj, RestoredObjectData data)
				{
					bool unityType = ShouldUseEditorSerialiser(obj);

					//Find any lost material refs
					List<MaterialRef> materialRefs = FindOriginalMaterials(obj, data._missingMaterials);

					if (unityType)
					{
						EditorJsonUtility.FromJsonOverwrite(data._json, obj);
						ApplyObjectRefs(obj, data._scenePath, data._missingObjectRefs);
					}
					else
					{
						JsonUtility.FromJsonOverwrite(data._json, obj);
					}

					//Revert any lost material refs
					ApplyMaterialsRefs(obj, materialRefs);
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

							data._missingObjectRefs += System.Convert.ToString(prop._objectId) + kObjectPathSplitChar + prop._propertyPath;
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
								int objId = GetLocalIdentifier(propertry.objectReferenceValue);

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
									localIdProp.objectReferenceValue = FindObject(sceneStr, id);
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

				private static int GetLocalIdentifier(Object obj)
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

				private static Object FindObject(string scenePath, int localIdentifier)
				{
					Scene scene;
					if (GetActiveScene(scenePath, out scene))
					{
						foreach (GameObject rootObject in scene.GetRootGameObjects())
						{
							Object obj = FindObject(rootObject, localIdentifier);

							if (obj != null)
							{
								return obj;
							}
						}
					}
					else
					{
						Debug.LogError("UnityPlayModeSaver: Can't save Components Play Modes changes as its Scene '" + scenePath + "' is not open in the Editor.");
					}

					return null;
				}

				private static Object FindObject(GameObject gameObject, int localIdentifier)
				{
					//Check game object
					if (GetLocalIdentifier(gameObject) == localIdentifier)
						return gameObject;

					//Check components
					Component[] components = gameObject.GetComponents<Component>();

					foreach (Component component in components)
					{
						if (GetLocalIdentifier(component) == localIdentifier)
							return component;
					}

					//Check children
					foreach (Transform child in gameObject.transform)
					{
						Object obj = FindObject(child.gameObject, localIdentifier);

						if (obj != null)
							return obj;
					}

					return null;
				}

				private static bool GetActiveScene(string scenePath, out Scene scene)
				{
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

					scene = new Scene();
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

				private static bool ShouldUseEditorSerialiser(Object obj)
				{
					if ((obj is Component && !(obj is MonoBehaviour)) || (obj is GameObject))
						return true;

					return false;
				}

				private static void RepaintEditorWindows()
				{
					SceneView.RepaintAll();
					
					System.Type inspectorWindowType = System.Type.GetType("UnityEditor.InspectorWindow, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
					if (inspectorWindowType != null)
					{
						EditorWindow.GetWindow(inspectorWindowType).Repaint();
					}

					System.Type gameViewType = System.Type.GetType("UnityEditor.GameView, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

					if (gameViewType != null)
					{
						BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
						MethodInfo methodInfo = gameViewType.GetMethod("RepaintAll", bindingFlags, null, new System.Type[] { }, null);

						if (methodInfo != null)
						{
							methodInfo.Invoke(null, null);
						}
					}
				}

				private static int SafeConvertToInt(string str)
				{
					int value;

					try
					{
						value = System.Convert.ToInt32(str);
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
				#endregion
			}
		}
	}
}