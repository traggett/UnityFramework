using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(GameObjectRef))]
			public class GameObjectRefPropertyDrawer : PropertyDrawer
			{
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);

					Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
					property.isExpanded = EditorGUI.Foldout(foldoutPosition, property.isExpanded, new GUIContent(label.text + " (Game Object Reference)"), true);

					if (property.isExpanded)
					{
						EditorGUI.indentLevel++;

						SerializedProperty sourceTypeProp = property.FindPropertyRelative("_sourceType");
						Rect sourceTypePosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

						EditorGUI.BeginChangeCheck();

						GameObjectRef.SourceType sourceType = (GameObjectRef.SourceType)EditorGUI.EnumPopup(sourceTypePosition, new GUIContent("Source Type"), (GameObjectRef.SourceType)sourceTypeProp.intValue);

						if (EditorGUI.EndChangeCheck())
						{
							//Reset data?
						}

						Rect objectPosition = new Rect(position.x, sourceTypePosition.y + sourceTypePosition.height, position.width, position.height - sourceTypePosition.height);

						switch (sourceType)
						{
							case GameObjectRef.SourceType.Scene:
								{
									DrawSceneObjectProperties(objectPosition, property);
								}
								break;
							case GameObjectRef.SourceType.Relative:
								{
									DrawRelativeObjectProperties(objectPosition, property);
								}
								break;
						}

						EditorGUI.indentLevel--;
					}
					
					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					return property.isExpanded ? EditorGUIUtility.singleLineHeight * 3f : EditorGUIUtility.singleLineHeight;
				}

				public static GameObject GetGameObject(SerializedProperty gameObjectRefProperty)
				{
					SerializedProperty sourceTypeProp = gameObjectRefProperty.FindPropertyRelative("_sourceType");

					switch ((GameObjectRef.SourceType)sourceTypeProp.intValue)
					{
						case GameObjectRef.SourceType.Relative:
							{
								return null;
							}
						default:
						case GameObjectRef.SourceType.Scene:
							{
								SerializedProperty sceneRefProp = gameObjectRefProperty.FindPropertyRelative("_scene");
								SerializedProperty pathProp = sceneRefProp.FindPropertyRelative("_scenePath");

								Scene scene = SceneManager.GetSceneByPath(pathProp.stringValue);

								if (scene.IsValid() && scene.isLoaded)
								{
									SerializedProperty sceneIdProp = gameObjectRefProperty.FindPropertyRelative("_sceneObjectID");

									return SceneIndexer.GetObject(scene, sceneIdProp.intValue);
								}

								return null;
							}
					}
				}

				private void DrawSceneObjectProperties(Rect position, SerializedProperty property)
				{
					Rect gameObjectPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
					GameObject gameObject = GetGameObject(property);

					EditorGUI.BeginChangeCheck();

					gameObject = (GameObject)EditorGUI.ObjectField(gameObjectPosition, gameObject, typeof(GameObject), true);

					if (EditorGUI.EndChangeCheck())
					{
						//Set scene ref data
						SerializedProperty sceneRefProp = property.FindPropertyRelative("_scene");
						{
							SerializedProperty pathProp = sceneRefProp.FindPropertyRelative("_scenePath");
							pathProp.stringValue = gameObject != null ? gameObject.scene.path : null;
						}

						//Set scene object id
						SerializedProperty sceneIdProp = property.FindPropertyRelative("_sceneObjectID");
						{
							sceneIdProp.intValue = SceneIndexer.GetIdentifier(gameObject);
						}
					}
				}

				private void DrawRelativeObjectProperties(Rect position, SerializedProperty property)
				{
					//Draw object path
				}
			}
		}
	}
}