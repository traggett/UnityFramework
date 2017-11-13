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
				private const string kEditorPrefRoot = "UnityPlayModeSaver.";
				private const string kEditorSaveObjectsCount = kEditorPrefRoot + "SavedObjects";
				private const char kSplitChar = '?';
				private const string kIdentifierProperty = "m_LocalIdentfierInFile";   //note the misspelling!
				private const string kInspectorProperty = "inspectorMode"; 
				#endregion

				#region Constructor
				static UnityPlayModeSaver()
				{
					EditorApplication.playModeStateChanged += ModeChanged;
				}
				#endregion

				#region Menus
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
				private static void ModeChanged(PlayModeStateChange state)
				{
					switch (state)
					{
						case PlayModeStateChange.EnteredEditMode:
							{
								RestoreSavedObjects();
							}
							break;
					}
				}

				private static void RestoreSavedObjects()
				{
					int numSavedObjects = EditorPrefs.GetInt(kEditorSaveObjectsCount, 0);
					List<Object> restoredObjects = new List<Object>();
					List<string> restoredObjectsJSONData = new List<string>();

					for (int i = 0; i < numSavedObjects; i++)
					{
						string objecKey = kEditorPrefRoot + System.Convert.ToString(i);
						string objectStr = EditorPrefs.GetString(objecKey, "");
						string jsonStr;

						Component component = GetComponent(objectStr, out jsonStr);

						if (component != null)
						{
							restoredObjects.Add(component);
							restoredObjectsJSONData.Add(jsonStr);
						}

						EditorPrefs.DeleteKey(objecKey);
					}

					if (restoredObjects.Count > 0)
					{
						Undo.RecordObjects(restoredObjects.ToArray(), kUndoText);

						for (int i = 0; i < restoredObjects.Count; i++)
						{
							if (UseEditorSerialiser(restoredObjects[i]))
								EditorJsonUtility.FromJsonOverwrite(restoredObjectsJSONData[i], restoredObjects[i]);
							else
								JsonUtility.FromJsonOverwrite(restoredObjectsJSONData[i], restoredObjects[i]);
						}
					}

					EditorPrefs.DeleteKey(kEditorSaveObjectsCount);
				}

				private static Component GetComponent(string objectKey, out string jsonStr)
				{
					string sceneStr, identifierStr;

					if (GetComponentStrings(objectKey, out sceneStr, out identifierStr, out jsonStr))
					{
						int identifier = SafeConvertToInt(identifierStr);
						Scene scene = SceneManager.GetSceneByPath(sceneStr);

						if (scene.IsValid())
						{
							foreach (GameObject rootObject in scene.GetRootGameObjects())
							{
								Component component = CheckComponents(rootObject, identifier);

								if (component != null)
								{
									return component;
								}
							}
						}
					}

					return null;
				}

				private static bool GetComponentStrings(string objectKey, out string sceneStr, out string identifierStr, out string jsonStr)
				{
					sceneStr = null;
					identifierStr = null;
					jsonStr = null;

					if (!string.IsNullOrEmpty(objectKey))
					{
						int split = objectKey.IndexOf(kSplitChar);

						if (split != -1)
						{
							sceneStr = objectKey.Substring(0, split);

							string idAndJson = objectKey.Substring(split + 1, objectKey.Length - split - 1);
							split = idAndJson.IndexOf(kSplitChar);

							if (split != -1)
							{
								identifierStr = idAndJson.Substring(0, split);
								jsonStr = idAndJson.Substring(split + 1, idAndJson.Length - split - 1);
								return true;
							}
						}
					}	

					return false;
				}

				private static int GetComponentSaveIndex(int identifier, string scenePath)
				{
					int numSavedObjects = EditorPrefs.GetInt(kEditorSaveObjectsCount, 0);

					for (int i=0; i<numSavedObjects; i++)
					{
						string objectStr = EditorPrefs.GetString(kEditorPrefRoot + System.Convert.ToString(i));
						string sceneStr, identifierStr, jsonStr;
						
						if (GetComponentStrings(objectStr, out sceneStr, out identifierStr, out jsonStr))
						{
							if (sceneStr == scenePath && identifier == SafeConvertToInt(identifierStr))
							{
								return i;
							}
						}
					}

					return -1;
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
							string objecKey = kEditorPrefRoot + System.Convert.ToString(saveObjIndex);
							EditorPrefs.DeleteKey(objecKey);
						}
					}
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
							int objectCount = EditorPrefs.GetInt(kEditorSaveObjectsCount);
							saveObjIndex = objectCount;
							objectCount++;
							EditorPrefs.SetInt(kEditorSaveObjectsCount, objectCount);
						}

						string json;

						if (UseEditorSerialiser(component))
							json = EditorJsonUtility.ToJson(component);
						else
							json = JsonUtility.ToJson(component);

						EditorPrefs.SetString(kEditorPrefRoot + System.Convert.ToString(saveObjIndex), scenePath + kSplitChar + System.Convert.ToString(identifier) + kSplitChar + json);
					}
				}

				private static bool UseEditorSerialiser(Object obj)
				{
					if (obj is Component && !(obj is MonoBehaviour))
						return true;

					return false;
				}

				private static Component CheckComponents(GameObject gameObject, int identifier)
				{
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
						Component component = CheckComponents(child.gameObject, identifier);

						if (component != null)
							return component;
					}

					return null;
				}

				private static int GetIdentifier(Component component)
				{
					if (component != null)
					{
						PropertyInfo inspectorModeInfo = typeof(SerializedObject).GetProperty(kInspectorProperty, BindingFlags.NonPublic | BindingFlags.Instance);

						SerializedObject serializedObject = new SerializedObject(component);
						inspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);

						SerializedProperty localIdProp = serializedObject.FindProperty(kIdentifierProperty);

						if (localIdProp != null && localIdProp.intValue != 0)
							return localIdProp.intValue;
					}

					return -1;
				}

				private static int SafeConvertToInt(string str)
				{
					int value = -1;

					try
					{
						value = System.Convert.ToInt32(str);
					}
					catch
					{

					}

					return value;
				}
				#endregion
			}
		}
	}
}