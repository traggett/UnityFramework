using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(SceneRef))]
			public sealed class SceneRefPropertyDrawer : PropertyDrawer
			{
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);

					Rect filePosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

					SerializedProperty fileProp = property.FindPropertyRelative("_scenePath");

					SceneAsset sceneAsset = null;

					if (!string.IsNullOrEmpty(fileProp.stringValue))
						sceneAsset = AssetDatabase.LoadAssetAtPath(fileProp.stringValue, typeof(SceneAsset)) as SceneAsset;

					EditorGUI.BeginChangeCheck();
					sceneAsset = EditorGUI.ObjectField(filePosition, "Scene", sceneAsset, typeof(SceneAsset), false) as SceneAsset;
					if (EditorGUI.EndChangeCheck())
					{
						fileProp.stringValue = AssetDatabase.GetAssetPath(sceneAsset);
					}

					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					return EditorGUIUtility.singleLineHeight;
				}
			}
		}
	}
}