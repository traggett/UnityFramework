
using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Serialization;

	namespace Utils
	{
		namespace Editor
		{
			[SerializedObjectEditor(typeof(SceneRef), "PropertyField")]
			public static class SceneRefEditor
			{
				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
				{
					SceneRef sceneRef = (SceneRef)obj;

					GUIContent[] sceneNames = new GUIContent[EditorBuildSettings.scenes.Length + 1];
					string[] scenePaths = new string[EditorBuildSettings.scenes.Length];

					sceneNames[0] = new GUIContent("(No Scene)");

					int currIndex = 0;
					for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
					{
						EditorBuildSettingsScene editorScene = EditorBuildSettings.scenes[i];
						sceneNames[i + 1] = new GUIContent(SceneUtils.GetSceneNameFromPath(editorScene.path));
						scenePaths[i] = editorScene.path;

						if (editorScene.path == sceneRef._scenePath)
						{
							currIndex = i + 1;
						}
					}

					EditorGUI.BeginChangeCheck();
					currIndex = EditorGUILayout.Popup(label, currIndex, sceneNames);
					if (EditorGUI.EndChangeCheck())
					{
						sceneRef._scenePath = currIndex == 0 ? null : scenePaths[currIndex - 1];
						dataChanged = true;
					}
					
					return sceneRef;
				}
				#endregion

			}
		}
	}
}