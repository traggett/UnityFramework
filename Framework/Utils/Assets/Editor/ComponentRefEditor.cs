using System;
using System.Reflection;

using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Serialization;
	using System.Collections.Generic;
	using UnityEngine.SceneManagement;

	namespace Utils
	{
		namespace Editor
		{
			[SerializedObjectEditor(typeof(ComponentRef<>), "PropertyField")]
			public static class ComponentRefEditor
			{
				private static GUIContent kLabel = new GUIContent("Object");

				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
				{
					Type componentType = SystemUtils.GetGenericImplementationType(typeof(ComponentRef<>), obj.GetType());

					if (componentType != null)
					{
						MethodInfo genericFieldMethod = typeof(ComponentRefEditor).GetMethod("ComponentField", BindingFlags.Static | BindingFlags.NonPublic);
						MethodInfo typedFieldMethod = genericFieldMethod.MakeGenericMethod(componentType);

						if (typedFieldMethod != null)
						{
							object[] args = new object[] { obj, label, dataChanged };
							obj = typedFieldMethod.Invoke(null, args);

							if ((bool)args[2])
								dataChanged = true;
						}
					}

					return obj;
				}
				#endregion

				private static ComponentRef<T> ComponentField<T>(ComponentRef<T> componentRef, GUIContent label, ref bool dataChanged) where T : class
				{
					if (label == null)
						label = new GUIContent();

					label.text += " (" + componentRef + ")";

					bool foldOut = EditorGUILayout.Foldout(componentRef._editorFoldout, label);

					if (foldOut != componentRef._editorFoldout)
					{
						componentRef._editorFoldout = foldOut;
						dataChanged = true;
					}

					if (foldOut)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						//Show drop down for gameobject type.
						GameObjectRef.eSourceType sourceType = SerializationEditorGUILayout.ObjectField(componentRef._gameObject._sourceType, "Source Type", ref dataChanged);

						if (sourceType != componentRef._gameObject._sourceType)
						{
							componentRef._gameObject = new GameObjectRef(sourceType);
							dataChanged = true;
						}

						switch (sourceType)
						{
							case GameObjectRef.eSourceType.Scene:
								RenderSceneObjectField(ref componentRef, ref dataChanged);
								break;
							case GameObjectRef.eSourceType.Prefab:
								RenderPrefabObjectField(ref componentRef, ref dataChanged);
								break;
							case GameObjectRef.eSourceType.Loaded:
								RenderLoadedObjectField(ref componentRef, ref dataChanged);
								break;
						}

						EditorGUI.indentLevel = origIndent;
					}


					return componentRef;
				}

				private static bool RenderObjectField<T>(ref ComponentRef<T> componentRef) where T : class
				{
					bool dataChanged = false;
					GameObject gameObject = null;

					//If T is a type of component can just use a normal object field
					if (typeof(Component).IsAssignableFrom(typeof(T)))
					{
						Component component = EditorGUILayout.ObjectField("Component", componentRef._editorComponent, typeof(T), true) as Component;
						gameObject = component != null ? component.gameObject : null;
					}
					//Otherwise allow gameobject to be set and deal with typing when rendering index
					else
					{
						gameObject = (GameObject)EditorGUILayout.ObjectField("Component", componentRef._editorComponent != null ? componentRef._editorComponent.gameObject : null, typeof(GameObject), true);
					}

					//Render drop down for typed components on the gameobject
					if (gameObject != null)
					{
						//Show drop down to allow selecting different components on same game object
						int currentIndex = 0;
						Component[] allComponents = gameObject.GetComponents<Component>();

						List<GUIContent> validComponentLabels = new List<GUIContent>();
						List<Component> validComponents = new List<Component>();
						List<Type> validComponentTypes = new List<Type>();

						for (int i = 0; i < allComponents.Length; i++)
						{
							T typedComponent = allComponents[i] as T;

							if (typedComponent != null)
							{
								int numberComponentsTheSameType = 0;
								foreach (Type type in validComponentTypes)
								{
									if (type == allComponents[i].GetType())
									{
										numberComponentsTheSameType++;
									}
								}

								validComponentLabels.Add(new GUIContent(allComponents[i].GetType().Name + (numberComponentsTheSameType > 0 ? " (" + numberComponentsTheSameType + ")" : "")));
								validComponents.Add(allComponents[i]);
								validComponentTypes.Add(allComponents[i].GetType());

								if (allComponents[i] == componentRef._editorComponent || componentRef._editorComponent == null)
								{
									currentIndex = validComponents.Count - 1;
								}
							}
						}

						if (validComponents.Count > 1)
						{
							int selectedIndex = EditorGUILayout.Popup(new GUIContent(" "), currentIndex, validComponentLabels.ToArray());
							dataChanged = componentRef._editorComponent != validComponents[selectedIndex] || componentRef._componentIndex != selectedIndex;
							componentRef._editorComponent = validComponents[selectedIndex];
							componentRef._componentIndex = selectedIndex;
						}
						else if (validComponents.Count == 1)
						{
							dataChanged = componentRef._editorComponent != validComponents[0] || componentRef._componentIndex != 0;
							componentRef._editorComponent = validComponents[0];
							componentRef._componentIndex = 0;
						}
						else if (validComponents.Count == 0)
						{
							dataChanged = componentRef._editorComponent != null || componentRef._componentIndex != 0;
							componentRef._editorComponent = null;
							componentRef._componentIndex = 0;
						}
					}
					else
					{
						dataChanged = componentRef._editorComponent != null || componentRef._componentIndex != 0;
						componentRef._editorComponent = null;
						componentRef._componentIndex = 0;
					}

					return dataChanged;
				}

				private static void RenderSceneObjectField<T>(ref ComponentRef<T> componentRef, ref bool dataChanged) where T : class
				{
					//If current component is valid
					if (componentRef._gameObject._scene.IsSceneRefValid())
					{
						//Check its scene is loaded
						Scene scene = componentRef._gameObject._scene.GetScene();

						if (scene.isLoaded)
						{
							//If loaded and not tried finding editor component, find it now
							if (!componentRef._editorSceneLoaded)
							{
								componentRef._editorComponent = componentRef.GetBaseComponent();
								componentRef._editorSceneLoaded = true;

								//If can no longer find the component, clear it
								if (componentRef._editorComponent == null)
								{
									componentRef = new ComponentRef<T>(GameObjectRef.eSourceType.Scene);
									dataChanged = true;
								}
							}

							//Then render component field
							if (RenderObjectField(ref componentRef))
							{
								componentRef._gameObject = new GameObjectRef(GameObjectRef.eSourceType.Scene, componentRef._editorComponent != null ? componentRef._editorComponent.gameObject : null);
								dataChanged = true;
							}
						}
						//If the scene is not loaded show warning...
						else
						{
							componentRef._editorSceneLoaded = false;

							//...and allow clearing of the component
							if (GameObjectRefEditor.RenderSceneNotLoadedField(componentRef._gameObject._scene))
							{
								componentRef = new ComponentRef<T>(GameObjectRef.eSourceType.Scene);
								dataChanged = true;
							}
						}
					}
					//Else don't have a component set, render component field
					else
					{
						componentRef._editorSceneLoaded = false;

						if (RenderObjectField(ref componentRef))
						{
							componentRef._gameObject = new GameObjectRef(GameObjectRef.eSourceType.Scene, componentRef._editorComponent != null ? componentRef._editorComponent.gameObject : null);
							dataChanged = true;
						}
					}
				}

				private static void RenderPrefabObjectField<T>(ref ComponentRef<T> componentRef, ref bool dataChanged) where T : class
				{
					if (RenderObjectField(ref componentRef))
					{
						componentRef._gameObject = new GameObjectRef(GameObjectRef.eSourceType.Prefab, componentRef._editorComponent != null ? componentRef._editorComponent.gameObject : null);
						dataChanged = true;
					}
				}

				private static void RenderLoadedObjectField<T>(ref ComponentRef<T> componentRef, ref bool dataChanged) where T : class
				{
					//If the component is valid
					if (componentRef._gameObject._scene.IsSceneRefValid())
					{
						//Check its scene is loaded
						Scene scene = componentRef._gameObject._scene.GetScene();

						if (scene.isLoaded)
						{
							//If loaded and not tried finding editor loader, find it now
							if (!componentRef._editorSceneLoaded)
							{
								componentRef._editorLoaderGameObject = componentRef._gameObject.GetEditorGameObjectLoader(scene);
								componentRef._editorSceneLoaded = true;

								//If can no longer find the editor loader, clear it
								if (componentRef._editorLoaderGameObject == null)
								{
									componentRef = new ComponentRef<T>(GameObjectRef.eSourceType.Loaded);
									dataChanged = true;
								}
							}

							//If have a valid loader...
							if (componentRef._editorLoaderGameObject != null)
							{
								//Check its loaded
								if (componentRef._editorLoaderGameObject.IsLoaded())
								{
									//If loaded and not tried finding component, find it now
									if (!componentRef._editorLoaderIsLoaded)
									{
										componentRef._editorComponent = componentRef.GetBaseComponent();
										componentRef._editorLoaderIsLoaded = true;

										//If can no longer find the component, clear it
										if (componentRef._editorComponent == null)
										{
											componentRef = new ComponentRef<T>(GameObjectRef.eSourceType.Loaded);
											dataChanged = true;
										}
									}

									//Then render component field
									if (RenderObjectField(ref componentRef))
									{
										componentRef._gameObject = new GameObjectRef(GameObjectRef.eSourceType.Loaded, componentRef._editorComponent != null ? componentRef._editorComponent.gameObject : null);
										dataChanged = true;
									}
								}
								//If the loader is not loaded show warning...
								else
								{
									componentRef._editorLoaderIsLoaded = false;

									//...and allow clearing of the component
									if (GameObjectRefEditor.RenderLoadedNotLoadedField(componentRef._editorLoaderGameObject))
									{
										componentRef = new ComponentRef<T>(GameObjectRef.eSourceType.Loaded);
										dataChanged = true;
									}
								}
							}
						}
						//If the scene is not loaded show warning...
						else
						{
							componentRef._editorSceneLoaded = false;

							//...and allow clearing of the component
							if (GameObjectRefEditor.RenderSceneNotLoadedField(componentRef._gameObject._scene))
							{
								componentRef = new ComponentRef<T>(GameObjectRef.eSourceType.Loaded);
								dataChanged = true;
							}
						}
					}
					//Else don't have a component set, render component field
					else
					{
						if (RenderObjectField(ref componentRef))
						{
							componentRef._gameObject = new GameObjectRef(GameObjectRef.eSourceType.Loaded, componentRef._editorComponent != null ? componentRef._editorComponent.gameObject : null);
							dataChanged = true;
						}
					}
				}
			}
		}
	}
}