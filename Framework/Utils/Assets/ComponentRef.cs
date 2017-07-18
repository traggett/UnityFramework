using System;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		[Serializable]		
		public struct ComponentRef<T> where T : class // T should be a type of Component or an interface
		{
			#region Public Data
			[SerializeField]
			private GameObjectRef _gameObject;
			[SerializeField]
			private int _componentIndex;
			#endregion

			#region Editor Data
#if UNITY_EDITOR
			[NonSerialized]
			public bool _editorCollapsed;
#endif
			#endregion

			public static implicit operator string(ComponentRef<T> property)
			{
				if (!string.IsNullOrEmpty(property._gameObject.GetGameObjectName()))
				{
					return property._gameObject.GetGameObjectName();
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

			public GameObjectRef GetGameObjectRef()
			{
				return _gameObject;
			}

#if UNITY_EDITOR
			public ComponentRef(GameObjectRef.eSourceType sourceType)
			{
				_gameObject = new GameObjectRef(sourceType);
				_componentIndex = 0;
				_editorCollapsed = false;
			}

			public ComponentRef(GameObjectRef.eSourceType sourceType, Component component)
			{
				GameObject gameObject = component != null ? component.gameObject : null;
				_gameObject = new GameObjectRef(sourceType, gameObject);
				_componentIndex = 0;
				_editorCollapsed = false;

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

			public ComponentRef(GameObjectRef.eSourceType sourceType, Component component, int componentIndex)
			{
				GameObject gameObject = component != null ? component.gameObject : null;
				_gameObject = new GameObjectRef(sourceType, gameObject);
				_componentIndex = componentIndex;
				_editorCollapsed = false;
			}

			public int GetComponentIndex()
			{
				return _componentIndex;
			}
#endif
		}
	}
}