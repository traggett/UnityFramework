
using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Serialization;
	using UnityEngine.SceneManagement;

	namespace Utils
	{
		namespace Editor
		{
			[SerializedObjectEditor(typeof(GameObjectRef), "PropertyField")]
			public static class GameObjectRefEditor
			{
				private static GUIContent kLabel = new GUIContent("Object");

				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
				{
					GameObjectRef gameObjectRef = (GameObjectRef)obj;

					if (label == null)
						label = new GUIContent();

					label.text += " (" + gameObjectRef + ")";

					bool foldOut = EditorGUILayout.Foldout(gameObjectRef._editorFoldout, label);

					if (foldOut != gameObjectRef._editorFoldout)
					{
						gameObjectRef._editorFoldout = foldOut;
						dataChanged = true;
					}

					if (foldOut)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						//Show drop down
						GameObjectRef.eSourceType sourceType = SerializationEditorGUILayout.ObjectField(gameObjectRef._sourceType, "Source Type", ref dataChanged);

						if (sourceType != gameObjectRef._sourceType)
						{
							gameObjectRef = new GameObjectRef(sourceType);
							dataChanged = true;
						}

						switch (sourceType)
						{
							case GameObjectRef.eSourceType.Scene:
								{
									RenderSceneGameObjectField(ref gameObjectRef, ref dataChanged);
								}
								break;
							case GameObjectRef.eSourceType.Prefab:
								{
									RenderPrefabGameObjectField(ref gameObjectRef, ref dataChanged);
								}
								break;
							case GameObjectRef.eSourceType.Loaded:
								{
									RenderLoadedGameObjectField(ref gameObjectRef, ref dataChanged);
								}
								break;
						}

						EditorGUI.indentLevel = origIndent;
					}

					return gameObjectRef;
				}
				#endregion

				private static void RenderSceneGameObjectField(ref GameObjectRef gameObjectRef, ref bool dataChanged)
				{
					if (gameObjectRef._scene.IsSceneRefValid())
					{
						Scene scene = gameObjectRef._scene.GetScene();

						if (scene.IsValid() && scene.isLoaded)
						{
							if (!gameObjectRef._editorSceneLoaded)
							{
								gameObjectRef._editorGameObject = gameObjectRef.GetGameObject();
								gameObjectRef._editorSceneLoaded = true;

								if (gameObjectRef._editorGameObject == null)
								{
									gameObjectRef = new GameObjectRef(GameObjectRef.eSourceType.Scene);
									dataChanged = true;
								}
							}

							GameObject gameObj = (GameObject)EditorGUILayout.ObjectField(kLabel, gameObjectRef._editorGameObject, typeof(GameObject), true);

							if (gameObj != gameObjectRef._editorGameObject)
							{
								gameObjectRef = new GameObjectRef(GameObjectRef.eSourceType.Scene, gameObj);
								dataChanged = true;
							}
						}
						else
						{
							gameObjectRef._editorSceneLoaded = false;

							if (RenderSceneNotLoadedField(gameObjectRef._scene))
							{
								gameObjectRef = new GameObjectRef(GameObjectRef.eSourceType.Scene);
								dataChanged = true;
							}
						}
					}
					else
					{
						GameObject gameObj = (GameObject)EditorGUILayout.ObjectField(kLabel, gameObjectRef._editorGameObject, typeof(GameObject), true);

						if (gameObj != gameObjectRef._editorGameObject)
						{
							gameObjectRef = new GameObjectRef(GameObjectRef.eSourceType.Scene, gameObj);
							dataChanged = true;
						}
					}
				}

				private static void RenderPrefabGameObjectField(ref GameObjectRef gameObjectRef, ref bool dataChanged)
				{
					GameObject prefabGameObject = (GameObject)EditorGUILayout.ObjectField(kLabel, gameObjectRef._editorGameObject, typeof(GameObject), true);

					if (prefabGameObject != gameObjectRef._editorGameObject)
					{
						gameObjectRef = new GameObjectRef(GameObjectRef.eSourceType.Prefab, prefabGameObject);
						dataChanged = true;
					}
				}

				private static void RenderLoadedGameObjectField(ref GameObjectRef gameObjectRef, ref bool dataChanged)
				{
					if (gameObjectRef._scene.IsSceneRefValid())
					{
						Scene scene = gameObjectRef._scene.GetScene();

						if (scene.IsValid() && scene.isLoaded)
						{
							if (!gameObjectRef._editorSceneLoaded)
							{
								gameObjectRef._editorLoaderGameObject = gameObjectRef.GetEditorGameObjectLoader(scene);
								gameObjectRef._editorGameObject = gameObjectRef.GetGameObject();
								gameObjectRef._editorSceneLoaded = true;
							}

							if (gameObjectRef._editorLoaderGameObject != null)
							{
								if (gameObjectRef._editorLoaderGameObject.IsLoaded())
								{
									if (!gameObjectRef._editorLoaderIsLoaded)
									{
										gameObjectRef._editorGameObject = gameObjectRef.GetGameObject();
										gameObjectRef._editorLoaderIsLoaded = true;
									}

									GameObject obj = (GameObject)EditorGUILayout.ObjectField(kLabel, gameObjectRef._editorGameObject, typeof(GameObject), true);

									if (obj != gameObjectRef._editorGameObject)
									{
										gameObjectRef = new GameObjectRef(GameObjectRef.eSourceType.Loaded, obj);
										dataChanged = true;
									}
								}
								else
								{
									gameObjectRef._editorLoaderIsLoaded = false;

									if (RenderLoadedNotLoadedField(gameObjectRef._editorLoaderGameObject))
									{
										gameObjectRef = new GameObjectRef(GameObjectRef.eSourceType.Loaded);
										dataChanged = true;
									}
								}
							}
						}
						else
						{
							gameObjectRef._editorSceneLoaded = false;

							if (RenderSceneNotLoadedField(gameObjectRef._scene))
							{
								gameObjectRef = new GameObjectRef(GameObjectRef.eSourceType.Loaded);
								dataChanged = true;
							}
						}
					}
					else
					{
						GameObject obj = (GameObject)EditorGUILayout.ObjectField(kLabel, gameObjectRef._editorGameObject, typeof(GameObject), true);

						if (obj != gameObjectRef._editorGameObject)
						{
							gameObjectRef = new GameObjectRef(GameObjectRef.eSourceType.Loaded, obj);
							dataChanged = true;
						}
					}
				}

				public static bool RenderSceneNotLoadedField(SceneRef scene)
				{
					bool clear = false;

					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Scene '" + scene + "' not loaded");

						if (GUILayout.Button("Load", GUILayout.ExpandWidth(false)))
						{
							scene.OpenSceneInEditor();
						}

						clear = GUILayout.Button("Clear", GUILayout.ExpandWidth(false));
					}
					EditorGUILayout.EndHorizontal();

					return clear;
				}

				public static bool RenderLoadedNotLoadedField(GameObjectLoader loader)
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
			}
		}
	}
}