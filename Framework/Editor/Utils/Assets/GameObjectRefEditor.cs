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
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
				{
					GameObjectRef gameObjectRef = (GameObjectRef)obj;

					if (label == null)
						label = new GUIContent();

					label.text += " (" + gameObjectRef + ")";

					bool editorCollapsed = !EditorGUILayout.Foldout(!gameObjectRef._editorCollapsed, label);

					if (editorCollapsed != gameObjectRef._editorCollapsed)
					{
						gameObjectRef._editorCollapsed = editorCollapsed;
						dataChanged = true;
					}

					if (!editorCollapsed)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						//Show drop down
						GameObjectRef.SourceType sourceType = SerializationEditorGUILayout.ObjectField(gameObjectRef.GetSourceType(), "Source Type", ref dataChanged);

						if (sourceType != gameObjectRef.GetSourceType())
						{
							gameObjectRef = new GameObjectRef(sourceType);
							dataChanged = true;
						}

						switch (sourceType)
						{
							case GameObjectRef.SourceType.Scene:
								{
									RenderSceneGameObjectField(ref gameObjectRef, ref dataChanged);
								}
								break;
							case GameObjectRef.SourceType.Relative:
								{
									RenderRelativeGameObjectField(ref gameObjectRef, ref dataChanged);
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
					if (gameObjectRef.IsValid())
					{
						Scene scene = gameObjectRef.GetSceneRef().GetScene();

						if (scene.IsValid() && scene.isLoaded)
						{
							EditorGUI.BeginChangeCheck();
							GameObject gameObj = (GameObject)EditorGUILayout.ObjectField(kLabel, gameObjectRef.GetGameObject(), typeof(GameObject), true);
							if (EditorGUI.EndChangeCheck())
							{
								gameObjectRef = new GameObjectRef(GameObjectRef.SourceType.Scene, gameObj);
								dataChanged = true;
							}
						}
						else if (RenderSceneNotLoadedField(gameObjectRef))
						{
							gameObjectRef = new GameObjectRef(GameObjectRef.SourceType.Scene);
							dataChanged = true;
						}
					}
					else
					{
						EditorGUI.BeginChangeCheck();
						GameObject gameObj = (GameObject)EditorGUILayout.ObjectField(kLabel, gameObjectRef.GetGameObject(), typeof(GameObject), true);
						if (EditorGUI.EndChangeCheck())
						{
							gameObjectRef = new GameObjectRef(GameObjectRef.SourceType.Scene, gameObj);
							dataChanged = true;
						}
					}
				}

				private static void RenderRelativeGameObjectField(ref GameObjectRef gameObjectRef, ref bool dataChanged)
				{
					EditorGUI.BeginChangeCheck();
					GameObject prefabGameObject = (GameObject)EditorGUILayout.ObjectField(kLabel, gameObjectRef.GetGameObject(), typeof(GameObject), false);

					if (EditorGUI.EndChangeCheck())
					{
						gameObjectRef = new GameObjectRef(GameObjectRef.SourceType.Relative, prefabGameObject);
						dataChanged = true;
					}
				}

				public static bool RenderSceneNotLoadedField(GameObjectRef gameObjectRef)
				{
					bool clear = false;

					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Scene '" + gameObjectRef.GetSceneRef() + "' not loaded");

						if (GUILayout.Button("Load", GUILayout.ExpandWidth(false)))
						{
							gameObjectRef.GetSceneRef().OpenSceneInEditor();
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