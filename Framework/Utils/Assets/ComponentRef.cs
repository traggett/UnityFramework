using System;

using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using Serialization;
	using System.Collections.Generic;

	namespace Utils
	{
		[Serializable]
		public sealed class ComponentRef<T> : ISerializationCallbackReceiver, ICustomEditorInspector where T : class
		{
			#region Public Data
			public GameObjectRef _gameObject = new GameObjectRef();
			public int _componentIndex = 0;
			#endregion

			#region Private Data
#if UNITY_EDITOR
			private Component _editorComponent = null;
			private GameObjectLoader _editorLoaderGameObject;
			private bool _editorFoldout = true;
			private bool _editorSceneLoaded = false;
			private bool _editorLoaderIsLoaded = false;
#endif
			#endregion

			public ComponentRef()
			{
				if (typeof(T) != typeof(Component) && !typeof(T).IsSubclassOf(typeof(Component)) && !typeof(T).IsInterface)
					throw new InvalidCastException("T should be a type of Component or an interface");
			}

			public static implicit operator string(ComponentRef<T> property)
			{
				if (!string.IsNullOrEmpty(property._gameObject._objectName))
				{
					return property._gameObject;
				}
				else
				{
					return typeof(T).Name;
				}
			}

			public static implicit operator T(ComponentRef<T> property)
			{
				if (property != null)
					return property.GetComponent();

				return null;
			}

			public override string ToString()
			{
				Component component = GetBaseComponent();

				if (component != null)
				{
					return component.gameObject.name;
				}

				return typeof(T).Name;
			}

			public T GetComponent()
			{
				return GetBaseComponent() as T;
			}

			public Component GetBaseComponent()
			{
				Component component = null;
				GameObject obj = _gameObject.GetGameObject();

				if (obj != null)
				{
					Component[] components = obj.GetComponents<Component>();
					int index = 0;

					for (int i = 0; i < components.Length; i++)
					{
						if (components[i] is T)
						{
							if (_componentIndex == index)
							{
								component = components[i];
								break;
							}
							
							index++;
						}
					}
				}

				return component;
			}

			public bool IsValid()
			{
				return _gameObject.IsValid();
			}

			#region ISerializationCallbackReceiver
			public void OnBeforeSerialize()
			{

			}

			public void OnAfterDeserialize()
			{
#if UNITY_EDITOR
				_editorComponent = GetComponent() as Component;
#endif
			}
			#endregion

			#region ICustomEditable
#if UNITY_EDITOR
			public bool RenderObjectProperties(GUIContent label)
			{
				bool dataChanged = false;

				if (label == null)
					label = new GUIContent();

				label.text += " (" + this + ")";

				_editorFoldout = EditorGUILayout.Foldout(_editorFoldout, label);
				if (_editorFoldout)
				{
					int origIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel++;

					//Show drop down
					GameObjectRef.eSourceType prevType = _gameObject._sourceType;
					_gameObject._sourceType = SerializedObjectEditorGUILayout.ObjectField(_gameObject._sourceType, "Source Type", out dataChanged);

					if (prevType != _gameObject._sourceType)
					{
						ClearComponent();
						dataChanged = true;
					}

					switch (_gameObject._sourceType)
					{
						case GameObjectRef.eSourceType.Scene:
							dataChanged |= RenderSceneObjectProperties();
							break;
						case GameObjectRef.eSourceType.Prefab:
							dataChanged |= RenderPrefabProperties();
							break;
						case GameObjectRef.eSourceType.Loaded:
							dataChanged |= RenderLoadedObjectProperties();
							break;
					}

					EditorGUI.indentLevel = origIndent;
				}

				return dataChanged;
			}
#endif
			#endregion

#if UNITY_EDITOR
			public T GetEditorComponent()
			{
				return _editorComponent as T;
			}

			public void ClearComponent()
			{
				_gameObject.ClearGameObject();
				_componentIndex = 0;

				_editorComponent = null;
				_editorLoaderGameObject = null;
				_editorSceneLoaded = false;
				_editorLoaderIsLoaded = false;
			}

			public void SetComponent(Component component, GameObjectRef.eSourceType gameObjectType)
			{
				GameObject gameObject = component != null ? component.gameObject : null;

				switch (gameObjectType)
				{
					case GameObjectRef.eSourceType.Scene:
						_gameObject.SetSceneGameObject(gameObject);
						break;
					case GameObjectRef.eSourceType.Prefab:
						_gameObject.SetPrefabGameObject(gameObject);
						break;
					case GameObjectRef.eSourceType.Loaded:
						_gameObject.SetLoadedGameObject(gameObject);
						break;
				}

				_editorComponent = component;
				_componentIndex = 0;

				if (component != null)
				{
					Component[] components = gameObject.GetComponents<Component>();
					int index = 0;
					for (int i = 0; i < components.Length; i++)
					{
						if (components[i] is T)
						{
							if (components[i] == component)
							{
								_componentIndex = index;
								break;
							}

							index++;
						}
					}
				}
			}

			private bool RenderSceneObjectProperties()
			{
				bool dataChanged = false;

				//If current component is valid
				if (_gameObject._scene.IsSceneValid())
				{
					//Check its scene is loaded
					Scene scene = _gameObject._scene.GetScene();

					if (scene.isLoaded)
					{
						//If loaded and not tried finding editor component, find it now
						if (!_editorSceneLoaded)
						{
							_editorComponent = GetBaseComponent();
							_editorSceneLoaded = true;

							//If can no longer find the component, clear it
							if (_editorComponent == null)
							{
								ClearComponent();
								dataChanged = true;
							}
						}

						//Then render component field
						dataChanged |= RenderSceneObjectField();
					}
					//If the scene is not loaded show warning...
					else
					{
						_editorSceneLoaded = false;

						//...and allow clearing of the component
						if (_gameObject.RenderSceneNotLoadedField())
						{
							ClearComponent();
							dataChanged = true;
						}
					}
				}
				//Else don't have a component set, render component field
				else
				{
					_editorSceneLoaded = false;
					dataChanged |= RenderSceneObjectField();
				}

				return dataChanged;
			}

			private bool RenderSceneObjectField()
			{
				if (RenderObjectField())
				{
					_gameObject.SetSceneGameObject(_editorComponent != null ? _editorComponent.gameObject : null);
					return true;
				}

				return false;
			}

			private bool RenderLoadedObjectProperties()
			{
				bool dataChanged = false;

				//If the component is valid
				if (_gameObject._scene.IsSceneValid())
				{
					//Check its scene is loaded
					Scene scene = _gameObject._scene.GetScene();

					if (scene.isLoaded)
					{
						//If loaded and not tried finding editor loader, find it now
						if (!_editorSceneLoaded)
						{
							_editorLoaderGameObject = _gameObject.GetGameObjectLoader(scene);
							_editorSceneLoaded = true;

							//If can no longer find the editor loader, clear it
							if (_editorLoaderGameObject == null)
							{
								ClearComponent();
								dataChanged = true;
							}
						}

						//If have a valid loader...
						if (_editorLoaderGameObject != null)
						{
							//Check its loaded
							if (_editorLoaderGameObject.IsLoaded())
							{
								//If loaded and not tried finding component, find it now
								if (!_editorLoaderIsLoaded)
								{
									_editorComponent = GetBaseComponent();
									_editorLoaderIsLoaded = true;

									//If can no longer find the component, clear it
									if (_editorComponent == null)
									{
										ClearComponent();
										dataChanged = true;
									}
								}

								//Then render component field
								dataChanged |= RenderLoadedObjectField();
							}
							//If the loader is not loaded show warning...
							else
							{
								_editorLoaderIsLoaded = false;

								//...and allow clearing of the component
								if (_gameObject.RenderLoadedNotLoadedField(_editorLoaderGameObject))
								{
									ClearComponent();
									dataChanged = true;
								}
							}
						}
					}
					//If the scene is not loaded show warning...
					else
					{
						_editorSceneLoaded = false;

						//...and allow clearing of the component
						if (_gameObject.RenderSceneNotLoadedField())
						{
							ClearComponent();
							dataChanged = true;
						}
					}
				}
				//Else don't have a component set, render component field
				else
				{
					dataChanged |= RenderLoadedObjectField();
				}

				return dataChanged;
			}

			private bool RenderLoadedObjectField()
			{
				if (RenderObjectField())
				{
					_gameObject.SetLoadedGameObject(_editorComponent != null ? _editorComponent.gameObject : null);
					return true;
				}

				return false;
			}

			private bool RenderPrefabProperties()
			{
				if (RenderObjectField())
				{
					_gameObject.SetPrefabGameObject(_editorComponent != null ? _editorComponent.gameObject : null);
					return true;
				}

				return false;
			}

			private bool RenderObjectField()
			{
				bool dataChanged = false;
				GameObject gameObject = null;

				//If T is a type of component can just use a normal object field
				if (typeof(Component).IsAssignableFrom(typeof(T)))
				{
					Component component = EditorGUILayout.ObjectField("Component", _editorComponent, typeof(T), true) as Component;
					gameObject = component != null ? component.gameObject : null;
				}
				//Otherwise allow gameobject to be set and deal with typing when rendering index
				else
				{
					gameObject = (GameObject)EditorGUILayout.ObjectField("Component", _editorComponent != null ? _editorComponent.gameObject : null, typeof(GameObject), true);
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

							if (allComponents[i] == _editorComponent || _editorComponent == null)
							{
								currentIndex = validComponents.Count - 1;
								_editorComponent = allComponents[i];
							}
						}
					}

					if (validComponents.Count > 1)
					{
						int selectedIndex = EditorGUILayout.Popup(new GUIContent(" "), currentIndex, validComponentLabels.ToArray());
						dataChanged = _editorComponent != validComponents[selectedIndex] || _componentIndex != selectedIndex;
						_editorComponent = validComponents[selectedIndex];
						_componentIndex = selectedIndex;
					}
					else if (validComponents.Count == 1)
					{
						dataChanged = _editorComponent != null || _componentIndex != 0;
						_editorComponent = validComponents[0];
						_componentIndex = 0;
					}
					else if (validComponents.Count == 0)
					{
						dataChanged = _editorComponent != null || _componentIndex != 0;
						_editorComponent = null;
						_componentIndex = 0;
					}
				}

				return dataChanged;
			}
#endif
		}
	}
}