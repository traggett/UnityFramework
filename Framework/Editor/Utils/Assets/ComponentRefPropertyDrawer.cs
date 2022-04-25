using System;
using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(ComponentRef<>), true)]
			public class ComponentRefPropertyDrawer : PropertyDrawer
			{
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);

					Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
					property.isExpanded = EditorGUI.Foldout(foldoutPosition, property.isExpanded, new GUIContent(label.text + " ("+ GetComponentType().Name + " Reference)"), true);

					if (property.isExpanded)
					{
						EditorGUI.indentLevel++;

						SerializedProperty gameObjectRefProp = property.FindPropertyRelative("_gameObject");
						SerializedProperty sourceTypeProp = gameObjectRefProp.FindPropertyRelative("_sourceType");
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

				private void DrawSceneObjectProperties(Rect position, SerializedProperty property)
				{
					Type componentType = GetComponentType();

					//Type is kind of component
					if (SystemUtils.IsTypeOf(typeof(UnityEngine.Object), componentType))
					{
						//Draw object field for component
						Component component = GetComponent(property);
						Rect componentPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

						EditorGUI.BeginChangeCheck();

						component = (Component)EditorGUI.ObjectField(componentPosition, component, componentType, true);

						if (EditorGUI.EndChangeCheck())
						{
							//Set game object ref
							SerializedProperty gameObjectRefProp = property.FindPropertyRelative("_gameObject");
							{
								//Set scene ref data
								SerializedProperty sceneRefProp = gameObjectRefProp.FindPropertyRelative("_scene");
								{
									SerializedProperty pathProp = sceneRefProp.FindPropertyRelative("_scenePath");
									pathProp.stringValue = component != null ? component.gameObject.scene.path : null;
								}

								//Set scene object id
								SerializedProperty sceneIdProp = gameObjectRefProp.FindPropertyRelative("_sceneObjectID");
								{
									sceneIdProp.intValue = SceneIndexer.GetIdentifier(component != null ? component.gameObject : null);
								}
							}

							//Set component index
							SerializedProperty componentIndexProp = property.FindPropertyRelative("_componentIndex");
							{
								componentIndexProp.intValue = GetComponentIndex(component);
							}
						}
					}
					//Type is interface
					else
					{
						//Draw game object field
					}
				}

				private void DrawRelativeObjectProperties(Rect position, SerializedProperty property)
				{
					
				}

				private Type GetComponentType()
				{
					return fieldInfo.FieldType.GenericTypeArguments[0];
				}

				private Component GetComponent(SerializedProperty property)
				{
					//Need to get gameobject and then component via index!
					SerializedProperty gameObjectRefProp = property.FindPropertyRelative("_gameObject");
					GameObject gameObject = GameObjectRefPropertyDrawer.GetGameObject(gameObjectRefProp);

					if (gameObject != null)
					{
						SerializedProperty componentIndexProp = property.FindPropertyRelative("_componentIndex");

						return GetComponent(gameObject, componentIndexProp.intValue);
					}

					return null;
				}

				private int GetComponentIndex(Component component)
				{
					if (component != null)
					{
						Type componentType = GetComponentType();
						Component[] components = component.gameObject.GetComponents<Component>();

						int index = 0;

						for (int i = 0; i < components.Length; i++)
						{
							if (SystemUtils.IsTypeOf(componentType, components[i].GetType()))
							{
								if (components[i] == component)
								{
									return index;
								}

								index++;
							}
						}
					}

					return -1;
				}

				private Component GetComponent(GameObject gameObject, int componentIndex)
				{
					if (gameObject != null && componentIndex != -1)
					{
						Type componentType = GetComponentType();
						Component[] components = gameObject.GetComponents<Component>();

						int index = 0;

						for (int i = 0; i < components.Length; i++)
						{
							if (SystemUtils.IsTypeOf(componentType, components[i].GetType()))
							{
								if (index == componentIndex)
								{
									return components[i];
								}

								index++;
							}
						}
					}

					return null;
				}
			}
		}
	}
}