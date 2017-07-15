using System;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		[Serializable]
		// T should be a type of Component or an interface
		public struct ComponentRef<T> : ISerializationCallbackReceiver where T : class
		{
			#region Public Data
			public GameObjectRef _gameObject;
			public int _componentIndex;
			#endregion

			#region Editor Data
#if UNITY_EDITOR
			[NonSerialized]
			public Component _editorComponent;
			[NonSerialized]
			public GameObjectLoader _editorLoaderGameObject;
			[NonSerialized]
			public bool _editorFoldout;
			[NonSerialized]
			public bool _editorSceneLoaded;
			[NonSerialized]
			public bool _editorLoaderIsLoaded;
#endif
			#endregion

			public static implicit operator string(ComponentRef<T> property)
			{
				if (!string.IsNullOrEmpty(property._gameObject._objectName))
				{
					return property._gameObject;
				}
				else
				{
					return SystemUtils.GetTypeName(typeof(T));
				}
			}

			public static implicit operator T(ComponentRef<T> property)
			{
				return property.GetComponent();
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

				if (_gameObject != null)
				{
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

#if UNITY_EDITOR
			public ComponentRef(GameObjectRef.eSourceType sourceType)
			{
				_gameObject = new GameObjectRef(sourceType);
				_componentIndex = 0;
				_editorComponent = null;
				_editorLoaderGameObject = null;
				_editorFoldout = true;
				_editorSceneLoaded = false;
				_editorLoaderIsLoaded = false;
			}

			public ComponentRef(GameObjectRef.eSourceType sourceType, Component component)
			{
				GameObject gameObject = component != null ? component.gameObject : null;
				_gameObject = new GameObjectRef(sourceType, gameObject);
				_componentIndex = 0;
				_editorComponent = null;
				_editorLoaderGameObject = null;
				_editorFoldout = true;
				_editorSceneLoaded = false;
				_editorLoaderIsLoaded = false;

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
#endif
		}
	}
}