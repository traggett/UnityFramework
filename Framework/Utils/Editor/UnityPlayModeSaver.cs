using UnityEngine;
using UnityEditor;
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
				private const string kSaveComponentMenuString = "CONTEXT/Component/Save Component Values";
				private const string kSaveGameObjectMenuString = "GameObject/Save Game Object";
				private const string kEditorPrefRoot = "UnityPlayModeSaver.";
				private const string kEditorObjects = kEditorPrefRoot + "SavedObjects";

				static UnityPlayModeSaver()
				{
					EditorApplication.playModeStateChanged += ModeChanged;
				}

				[MenuItem(kSaveComponentMenuString)]
				private static void SaveComponent(MenuCommand command)
				{
					Component component = (Component)command.context;

					if (Application.isPlaying && component != null)
					{
						StoreObject(component);
					}
				}

				[MenuItem(kSaveComponentMenuString, true)]
				private static bool ValidateSaveComponent(MenuCommand command)
				{
					Component component = (Component)command.context;
					return Application.isPlaying && component != null;
				}

				/*
				[MenuItem(kSaveGameObjectMenuString, false, 12)]
				private static void SaveGameObject(MenuCommand command)
				{
					GameObject gameObject = (GameObject)command.context;

					if (Application.isPlaying && gameObject != null)
					{
						StoreObject(gameObject);
					}	
				}*/

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
					string savedObjects = EditorPrefs.GetString(kEditorObjects);

					if (!string.IsNullOrEmpty(savedObjects))
					{
						string[] objects = savedObjects.Split(',');

						if (objects != null && objects.Length > 0)
						{
							List<Object> restoredObjects = new List<Object>();
							List<string> restoredObjectsData = new List<string>();

							for (int i = 0; i < objects.Length; i++)
							{
								try
								{
									int objectId = System.Convert.ToInt32(objects[i]);
									Object obj = GetObject(objectId);

									if (obj != null && !restoredObjects.Contains(obj))
									{
										restoredObjects.Add(obj);
										restoredObjectsData.Add(EditorPrefs.GetString(kEditorPrefRoot + objects[i]));
									}

									EditorPrefs.DeleteKey(kEditorPrefRoot + objects[i]);
								}
								catch { }
							}

							if (restoredObjects.Count > 0)
							{
								Undo.RecordObjects(restoredObjects.ToArray(), "Set values from Play Mode");

								for (int i = 0; i < restoredObjects.Count; i++)
								{
									EditorJsonUtility.FromJsonOverwrite(restoredObjectsData[i], restoredObjects[i]);
								}
							}
						}
					}

					EditorPrefs.DeleteKey(kEditorObjects);
				}

				private static void StoreObject(Object obj)
				{
					int identifier = GetIdentifier(obj);

					if (identifier != -1)
					{
						string savedObjects = EditorPrefs.GetString(kEditorObjects);

						if (!string.IsNullOrEmpty(savedObjects))
							savedObjects += ",";

						savedObjects += identifier;

						EditorPrefs.SetString(kEditorObjects, savedObjects);


						string json = EditorJsonUtility.ToJson(obj);
						EditorPrefs.SetString(kEditorPrefRoot + identifier, json);
					}
				}

				private static Object GetObject(int identifier)
				{
					Scene scene = SceneManager.GetActiveScene();

					if (scene != null)
					{
						//Find object by id in scene
						foreach (GameObject rootObject in scene.GetRootGameObjects())
						{
							Object obj = CheckGameObject(rootObject, identifier);

							if (obj != null)
								return obj;
						}
					}

					return null;
				}

				private static Object CheckGameObject(GameObject gameObject, int identifier)
				{
					//Check main gameobject
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
						Object obj = CheckGameObject(child.gameObject, identifier);

						if (obj != null)
							return obj;
					}

					return null;
				}

				private static int GetIdentifier(Object obj)
				{
					if (obj == null)
					{
						return -1;
					}

					EditorUtility.SetDirty(obj);

					PropertyInfo inspectorModeInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

					SerializedObject serializedObject = new SerializedObject(obj);
					inspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);

					SerializedProperty localIdProp = serializedObject.FindProperty("m_LocalIdentfierInFile");   //note the misspelling!

					return localIdProp.intValue;
				}
			}
		}
	}
}