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
				private const string kSaveComponentMenuString = "CONTEXT/Component/Save Values To Scene";
				private const string kRevertComponentMenuString = "CONTEXT/Component/Revert Play Mode Changes";
				private const string kUndoText = "Set Play Mode Values";
				private const string kEditorPrefsKey = "UnityPlayModeSaver.";
				private const string kEditorPrefsObjectCountKey = kEditorPrefsKey + "SavedObjects";
				private const string kEditorPrefsObjectScene = ".Scene";
				private const string kEditorPrefsObjectId = ".Id";
				private const string kEditorPrefsObjectJson = ".Json";
				private const string kEditorPrefsObjectRefs = ".ObjRefs";
				private const string kEditorPrefsObjectMaterialRefs = ".Materials";
				private const char kSplitChar = '?';
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

					return Application.isPlaying && component != null && GetIdentifier(component) != -1;
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
						int identifier = GetIdentifier(component);

						if (identifier != -1)
						{
							string scenePath = component.gameObject.scene.path;

							if (GetComponentSaveIndex(identifier, scenePath) != -1)
								return true;
						}
					}

					return false;
				}
				#endregion

				#region Private Functions
				private static void OnModeChanged(PlayModeStateChange state)
				{
					if (state == PlayModeStateChange.EnteredEditMode)
					{
						RestoreSavedObjects();
					}
				}

				private static void RestoreSavedObjects()
				{
					int numSavedObjects = EditorPrefs.GetInt(kEditorPrefsObjectCountKey, 0);

					List<Object> restoredObjects = new List<Object>();
					List<RestoredObjectData> restoredObjectsData = new List<RestoredObjectData>();

					for (int i = 0; i < numSavedObjects; i++)
					{
						if (EditorPrefs.HasKey(kEditorPrefsKey + System.Convert.ToString(i) + kEditorPrefsObjectScene))
						{
							string sceneStr = EditorPrefs.GetString(kEditorPrefsKey + System.Convert.ToString(i) + kEditorPrefsObjectScene);
							string identifierStr = EditorPrefs.GetString(kEditorPrefsKey + System.Convert.ToString(i) + kEditorPrefsObjectId);

							Object obj = GetObject(sceneStr, SafeConvertToInt(identifierStr));

							if (obj != null)
							{
								string jsonStr = EditorPrefs.GetString(kEditorPrefsKey + System.Convert.ToString(i) + kEditorPrefsObjectJson);
								string objectRefStr = EditorPrefs.GetString(kEditorPrefsKey + System.Convert.ToString(i) + kEditorPrefsObjectRefs);
								string materialStr = EditorPrefs.GetString(kEditorPrefsKey + System.Convert.ToString(i) + kEditorPrefsObjectMaterialRefs);

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

						SafeDeleteEditorPref(EditorPrefs.GetString(kEditorPrefsKey + System.Convert.ToString(i) + kEditorPrefsObjectScene));
						SafeDeleteEditorPref(EditorPrefs.GetString(kEditorPrefsKey + System.Convert.ToString(i) + kEditorPrefsObjectId));
						SafeDeleteEditorPref(EditorPrefs.GetString(kEditorPrefsKey + System.Convert.ToString(i) + kEditorPrefsObjectJson));
						SafeDeleteEditorPref(EditorPrefs.GetString(kEditorPrefsKey + System.Convert.ToString(i) + kEditorPrefsObjectRefs));
						SafeDeleteEditorPref(EditorPrefs.GetString(kEditorPrefsKey + System.Convert.ToString(i) + kEditorPrefsObjectMaterialRefs));
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

				private static void SaveComponent(Component component)
				{
					int identifier = GetIdentifier(component);

					if (identifier != -1)
					{
						string scenePath = component.gameObject.scene.path;

						int saveObjIndex = GetComponentSaveIndex(identifier, scenePath);

						//If the component isn't already in saved object list add it
						if (saveObjIndex == -1)
						{
							int objectCount = EditorPrefs.GetInt(kEditorPrefsObjectCountKey);
							saveObjIndex = objectCount;
							objectCount++;
							EditorPrefs.SetInt(kEditorPrefsObjectCountKey, objectCount);
						}

						RestoredObjectData data = GetComponentData(component);

						EditorPrefs.SetString(kEditorPrefsKey + System.Convert.ToString(saveObjIndex) + kEditorPrefsObjectScene, scenePath);
						EditorPrefs.SetString(kEditorPrefsKey + System.Convert.ToString(saveObjIndex) + kEditorPrefsObjectId, System.Convert.ToString(identifier));
						EditorPrefs.SetString(kEditorPrefsKey + System.Convert.ToString(saveObjIndex) + kEditorPrefsObjectJson, data._json);
						EditorPrefs.SetString(kEditorPrefsKey + System.Convert.ToString(saveObjIndex) + kEditorPrefsObjectRefs, data._missingObjectRefs);
						EditorPrefs.SetString(kEditorPrefsKey + System.Convert.ToString(saveObjIndex) + kEditorPrefsObjectMaterialRefs, data._missingMaterials);
					}
				}

				private static void RevertComponent(Component component)
				{
					int identifier = GetIdentifier(component);

					if (identifier != -1)
					{
						string scenePath = component.gameObject.scene.path;
						int saveObjIndex = GetComponentSaveIndex(identifier, scenePath);

						if (saveObjIndex != -1)
						{
							SafeDeleteEditorPref(kEditorPrefsKey + System.Convert.ToString(saveObjIndex) + kEditorPrefsObjectScene);
							SafeDeleteEditorPref(kEditorPrefsKey + System.Convert.ToString(saveObjIndex) + kEditorPrefsObjectId);
							SafeDeleteEditorPref(kEditorPrefsKey + System.Convert.ToString(saveObjIndex) + kEditorPrefsObjectJson);
							SafeDeleteEditorPref(kEditorPrefsKey + System.Convert.ToString(saveObjIndex) + kEditorPrefsObjectRefs);
							SafeDeleteEditorPref(kEditorPrefsKey + System.Convert.ToString(saveObjIndex) + kEditorPrefsObjectMaterialRefs);
						}
					}
				}

				private static Object GetObject(string scenePath, int identifier)
				{
					Scene scene;
					if (GetScene(scenePath, out scene))
					{
						foreach (GameObject rootObject in scene.GetRootGameObjects())
						{
							Object obj = CheckChildren(rootObject, identifier);

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

				private static bool GetScene(string scenePath, out Scene scene)
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

				private static Object CheckChildren(GameObject gameObject, int identifier)
				{
					//Check game object
					if (GetIdentifier(gameObject) == identifier)
						return gameObject;

					//Check components
					Component[] components = gameObject.GetComponents<Component>();

					foreach (Component component in components)
					{
						if (GetIdentifier(component) == identifier)
							return component;
					}

					//Check children
					foreach (Transform child in gameObject.transform)
					{
						Object obj = CheckChildren(child.gameObject, identifier);

						if (obj != null)
							return obj;
					}

					return null;
				}

				private static int GetComponentSaveIndex(int identifier, string scenePath)
				{
					int numSavedObjects = EditorPrefs.GetInt(kEditorPrefsObjectCountKey, 0);

					for (int i = 0; i < numSavedObjects; i++)
					{
						string sceneStr = EditorPrefs.GetString(kEditorPrefsKey + System.Convert.ToString(i) + kEditorPrefsObjectScene);
						string identifierStr = EditorPrefs.GetString(kEditorPrefsKey + System.Convert.ToString(i) + kEditorPrefsObjectId);

						if (sceneStr == scenePath && identifier == SafeConvertToInt(identifierStr))
						{
							return i;
						}
					}

					return -1;
				}

				private static RestoredObjectData GetComponentData(Component component)
				{
					RestoredObjectData data = new RestoredObjectData();

					bool unityType = ShouldUseEditorSerialiser(component);

					List<string> materials = new List<string>();
					List<ObjectRefProperty> objectProperties = unityType ? new List<ObjectRefProperty>() : null;

					GetSpecialCaseProperties(component, materials, objectProperties);

					//If its a Unity in built type we have to restore any scene links as they won't be serialized by EditorJsonUtility
					if (unityType)
					{
						data._json = EditorJsonUtility.ToJson(component);

						//Save any object ptr properties that point at scene objects
						data._missingObjectRefs = "";

						foreach (ObjectRefProperty prop in objectProperties)
						{
							if (!string.IsNullOrEmpty(data._missingObjectRefs))
								data._missingObjectRefs += kSplitChar;

							data._missingObjectRefs += System.Convert.ToString(prop._objectId) + ":" + prop._propertyPath;
						}
					}
					else
					{
						data._json = JsonUtility.ToJson(component);
						data._missingObjectRefs = "";
					}

					//Store material properties that now point at a runtime instance of a material (they will get reverted to original values)
					data._missingMaterials = "";

					foreach (string material in materials)
					{
						if (!string.IsNullOrEmpty(data._missingMaterials))
							data._missingMaterials += kSplitChar;

						data._missingMaterials += material;
					}

					return data;
				}
				
				private static void RestoreObjectFromData(Object obj, RestoredObjectData data)
				{
					bool unityType = ShouldUseEditorSerialiser(obj);

					//Find any lost material refs
					List<MaterialRef> materialRefs = FindRuntimeInstancedMaterials(obj, data._missingMaterials);

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

				private static int GetIdentifier(Object obj)
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

				private static bool ShouldUseEditorSerialiser(Object obj)
				{
					if ((obj is Component && !(obj is MonoBehaviour)) || (obj is GameObject))
						return true;

					return false;
				}

				private static void GetSpecialCaseProperties(Component component, List<string> materials, List<ObjectRefProperty> objectProperties)
				{
					SerializedObject serializedObject = new SerializedObject(component);
					SerializedProperty propertry = serializedObject.GetIterator();

					while (propertry.NextVisible(true))
					{
						if (propertry.type == "PPtr<Material>")
						{
							if (propertry.objectReferenceValue != null && propertry.objectReferenceValue.name.EndsWith("(Instance)"))
								materials.Add(propertry.propertyPath);
						}
						else if (objectProperties != null && propertry.type.StartsWith("PPtr<"))
						{
							//Only apply if reference is same scene 
							bool sameScene = false;

							Component compRef = propertry.objectReferenceValue as Component;

							if (compRef != null)
							{
								sameScene = component.gameObject.scene == component.gameObject.scene;
							}
							else
							{
								GameObject gameObject = propertry.objectReferenceValue as GameObject;

								if (gameObject != null)
									sameScene = gameObject.scene == component.gameObject.scene;
							}

							if (sameScene)
							{
								int objId = GetIdentifier(propertry.objectReferenceValue);

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

				private static List<MaterialRef> FindRuntimeInstancedMaterials(Object obj, string materialStr)
				{
					List<MaterialRef> materialRefs = new List<MaterialRef>();

					SerializedObject serializedObject = new SerializedObject(obj);
					string[] materials = materialStr.Split(kSplitChar);

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
					string[] objectRefs = objectRefStr.Split(kSplitChar);

					foreach (string objectRef in objectRefs)
					{
						int split = objectRef.IndexOf(':');

						if (split != -1)
						{
							int id = SafeConvertToInt(objectRef.Substring(0, split));

							if (id != -1)
							{
								string objPath = objectRef.Substring(split + 1, objectRef.Length - split - 1);

								SerializedProperty localIdProp = serializedObject.FindProperty(objPath);

								if (localIdProp != null)
								{
									localIdProp.objectReferenceValue = GetObject(sceneStr, id);
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