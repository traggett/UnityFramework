using System;

using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using Serialization;

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

				if (_gameObject._scene.IsSceneValid())
				{
					Scene scene = _gameObject._scene.GetScene();

					if (scene.isLoaded)
					{
						if (!_editorSceneLoaded)
						{
							_editorComponent = GetBaseComponent();
							_editorSceneLoaded = true;

							if (_editorComponent == null)
							{
								ClearComponent();
								dataChanged = true;
							}
						}

						dataChanged |= RenderSceneObjectField();
						dataChanged |= RenderComponentIndexField();
					}
					else
					{
						_editorSceneLoaded = false;

						if (_gameObject.RenderSceneNotLoadedField())
						{
							ClearComponent();
							dataChanged = true;
						}
					}
				}
				else
				{
					dataChanged |= RenderSceneObjectField();
				}

				return dataChanged;
			}

			private bool RenderSceneObjectField()
			{
				Component component = RenderObjectField();

				if (component != _editorComponent)
				{
					_gameObject.SetSceneGameObject(component != null ? component.gameObject : null);
					_editorComponent = component;
					return true;
				}

				return false;
			}

			private bool RenderLoadedObjectProperties()
			{
				bool dataChanged = false;

				if (_gameObject._scene.IsSceneValid())
				{
					Scene scene = _gameObject._scene.GetScene();

					if (scene.isLoaded)
					{
						if (!_editorSceneLoaded)
						{
							_editorLoaderGameObject = _gameObject.GetGameObjectLoader(scene);
							_editorComponent = GetBaseComponent();
							_editorSceneLoaded = true;
						}

						if (_editorLoaderGameObject != null)
						{
							if (_editorLoaderGameObject.IsLoaded())
							{
								if (!_editorLoaderIsLoaded)
								{
									_editorComponent = GetBaseComponent();
									_editorLoaderIsLoaded = true;
								}

								dataChanged |= RenderLoadedObjectField();
								dataChanged |= RenderComponentIndexField();
							}
							else
							{
								_editorLoaderIsLoaded = false;

								if (_gameObject.RenderLoadedNotLoadedField(_editorLoaderGameObject))
								{
									ClearComponent();
									dataChanged = true;
								}
							}
						}
					}
					else
					{
						_editorSceneLoaded = false;

						if (_gameObject.RenderSceneNotLoadedField())
						{
							ClearComponent();
							dataChanged = true;
						}
					}
				}
				else
				{
					dataChanged |= RenderLoadedObjectField();
				}

				return dataChanged;
			}

			private bool RenderLoadedObjectField()
			{
				Component component = RenderObjectField();

				if (component != _editorComponent)
				{
					_gameObject.SetLoadedGameObject(component != null ? component.gameObject : null);
					_editorComponent = component;
					return true;
				}

				return false;
			}

			private bool RenderPrefabProperties()
			{
				bool dataChanged = false;
				Component component = RenderObjectField();

				if (component != _editorComponent)
				{
					_gameObject.SetPrefabGameObject(component != null ? component.gameObject : null);
					_editorComponent = component;
					dataChanged = true;
				}

				dataChanged |= RenderComponentIndexField();

				return dataChanged;
			}

			private Component RenderObjectField()
			{
				Component component = null;

				if (typeof(T).IsInterface)
				{
					GameObject gameObject = (GameObject)EditorGUILayout.ObjectField("Object", _editorComponent != null ? _editorComponent.gameObject : null, typeof(GameObject), true);

					if (gameObject!= null)
					{
						Component[] components = gameObject.GetComponents<Component>();
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
				}
				else
				{
					component = EditorGUILayout.ObjectField("Object", _editorComponent, typeof(T), true) as Component;
				}

				return component;
			}

			private bool RenderComponentIndexField()
			{
				if (_editorComponent != null)
				{
					int numComponents = 0;
					Component[] components = _editorComponent.gameObject.GetComponents<Component>();

					for (int i = 0; i < components.Length; i++)
					{
						if (components[i] is T)
						{
							numComponents++;
						}
					}
					
					if (numComponents > 1)
					{
						int newComponentIndex = EditorGUILayout.IntSlider("Component Index", _componentIndex, 0, numComponents - 1);
						
						if (newComponentIndex != _componentIndex)
						{
							int index = 0;

							for (int i = 0; i < components.Length; i++)
							{
								if (components[i] is T)
								{
									if (newComponentIndex == index)
									{
										_editorComponent = components[i];
										_componentIndex = i;
										break;
									}

									index++;
								}
							}
							
							return true;
						}
					}
				}

				return false;
			}
#endif
		}
	}
}