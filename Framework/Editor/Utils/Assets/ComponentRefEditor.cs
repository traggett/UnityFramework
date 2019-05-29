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
				private static GUIContent kLabel = new GUIContent("Component");

				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
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

					bool editorCollapsed = !EditorGUILayout.Foldout(!componentRef._editorCollapsed, label);

					if (editorCollapsed != componentRef._editorCollapsed)
					{
						componentRef._editorCollapsed = editorCollapsed;
						dataChanged = true;
					}

					if (!editorCollapsed)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						//Show drop down for gameobject type.
						GameObjectRef.eSourceType sourceType = SerializationEditorGUILayout.ObjectField(componentRef.GetGameObjectRef().GetSourceType(), "Source Type", ref dataChanged);

						if (sourceType != componentRef.GetGameObjectRef().GetSourceType())
						{
							componentRef = new ComponentRef<T>(sourceType);
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

					Component currentComponent = componentRef.GetBaseComponent();

					//If T is a type of component can just use a normal object field
					if (typeof(Component).IsAssignableFrom(typeof(T)))
					{
						Component component = EditorGUILayout.ObjectField(kLabel, currentComponent, typeof(T), true) as Component;
						gameObject = component != null ? component.gameObject : null;
					}
					//Otherwise allow gameobject to be set and deal with typing when rendering index
					else
					{
						gameObject = (GameObject)EditorGUILayout.ObjectField(kLabel, currentComponent != null ? currentComponent.gameObject : null, typeof(GameObject), true);
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

								if (allComponents[i] == currentComponent)
								{
									currentIndex = validComponents.Count - 1;
								}
							}
						}

						if (validComponents.Count > 1)
						{
							int selectedIndex = EditorGUILayout.Popup(kLabel, currentIndex, validComponentLabels.ToArray());
							dataChanged = currentComponent != validComponents[selectedIndex] || componentRef.GetComponentIndex() != selectedIndex;
							if (dataChanged)
								componentRef = new ComponentRef<T>(componentRef.GetGameObjectRef().GetSourceType(), validComponents[selectedIndex], selectedIndex);
						}
						else if (validComponents.Count == 1)
						{
							dataChanged = currentComponent != validComponents[0] || componentRef.GetComponentIndex() != 0;
							if (dataChanged)
								componentRef = new ComponentRef<T>(componentRef.GetGameObjectRef().GetSourceType(), validComponents[0], 0);
						}
						else if (validComponents.Count == 0)
						{
							dataChanged = currentComponent != null || componentRef.GetComponentIndex() != 0;
							if (dataChanged)
								componentRef = new ComponentRef<T>(componentRef.GetGameObjectRef().GetSourceType());
						}
					}
					else
					{
						dataChanged = currentComponent != null || componentRef.GetComponentIndex() != 0;
						if (dataChanged)
							componentRef = new ComponentRef<T>(componentRef.GetGameObjectRef().GetSourceType());
					}

					return dataChanged;
				}

				private static void RenderSceneObjectField<T>(ref ComponentRef<T> componentRef, ref bool dataChanged) where T : class
				{
					//If current component is valid
					if (componentRef.IsValid())
					{
						//Check its scene is loaded
						Scene scene = componentRef.GetGameObjectRef().GetSceneRef().GetScene();

						if (scene.isLoaded)
						{
							//Then render component field
							if (RenderObjectField(ref componentRef))
							{
								dataChanged = true;
							}
						}
						//If the scene is not loaded show warning and allow clearing of the component
						else if (GameObjectRefEditor.RenderSceneNotLoadedField(componentRef.GetGameObjectRef()))
						{
							componentRef = new ComponentRef<T>(GameObjectRef.eSourceType.Scene);
							dataChanged = true;
						}
					}
					//Else don't have a valid component set, renderer object field
					else if (RenderObjectField(ref componentRef))
					{
						dataChanged = true;
					}
				}

				private static void RenderPrefabObjectField<T>(ref ComponentRef<T> componentRef, ref bool dataChanged) where T : class
				{
					if (RenderObjectField(ref componentRef))
					{
						dataChanged = true;
					}
				}

				private static void RenderLoadedObjectField<T>(ref ComponentRef<T> componentRef, ref bool dataChanged) where T : class
				{
					//If the component is valid
					if (componentRef.IsValid())
					{
						//Check its scene is loaded
						Scene scene = componentRef.GetGameObjectRef().GetSceneRef().GetScene();

						if (scene.isLoaded)
						{
							//If loaded and not tried finding editor loader, find it now
							GameObjectLoader gameObjectLoader = componentRef.GetGameObjectRef().GetEditorGameObjectLoader(scene);

							//If have a valid loader...
							if (gameObjectLoader != null)
							{
								//Check its loaded
								if (gameObjectLoader.IsLoaded())
								{
									//Then render component field
									if (RenderObjectField(ref componentRef))
									{
										dataChanged = true;
									}
								}
								//If the loader is not loaded show warning and allow clearing of the component
								else if (GameObjectRefEditor.RenderLoadedNotLoadedField(gameObjectLoader))
								{
									componentRef = new ComponentRef<T>(GameObjectRef.eSourceType.Loaded);
									dataChanged = true;
								}
							}
						}
						//If the scene is not loaded show warning and allow clearing of the component
						else if (GameObjectRefEditor.RenderSceneNotLoadedField(componentRef.GetGameObjectRef()))
						{
							componentRef = new ComponentRef<T>(GameObjectRef.eSourceType.Loaded);
							dataChanged = true;
						}
					}
					//Else don't have a component set, render component field
					else if (RenderObjectField(ref componentRef))
					{
						dataChanged = true;
					}
				}
			}
		}
	}
}