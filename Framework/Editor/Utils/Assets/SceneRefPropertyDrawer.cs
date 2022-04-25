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

					SerializedProperty assetProp = property.FindPropertyRelative("_sceneAsset");
					SerializedProperty pathProp = property.FindPropertyRelative("_scenePath");

					EditorGUI.BeginChangeCheck();

					Object sceneAsset = EditorGUI.ObjectField(filePosition, label, assetProp.objectReferenceValue, typeof(SceneAsset), false);
		
					if (EditorGUI.EndChangeCheck())
					{
						assetProp.objectReferenceValue = sceneAsset;
					}

					//Check path has changed (scene asset has moved)
					string scenePath = AssetDatabase.GetAssetPath(assetProp.objectReferenceValue);

					if (pathProp.stringValue != scenePath)
					{
						pathProp.stringValue = scenePath;
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