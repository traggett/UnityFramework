#if DEBUG

using Framework.UI;
using Framework.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Framework
{
	namespace Debug
	{
		public static class DebugMenuManager
		{
			private static Menu _rootMenu;
			private static DebugMenuInputActions _inputActions;
			private static DebugMenuVisuals _visuals;
			private static Menu _currentMenu;
			private static int _currentItemIndex;

			#region Menu Items
			private abstract class MenuItem
			{
				public readonly string _name;

				public MenuItem(string name)
				{
					_name = name;
				}

				public abstract string GetDisplayedText();
			}

			private class Menu : MenuItem
			{
				public Menu _parent;
				public List<MenuItem> _items;

				public Menu(string name, Menu parent = null) : base(name)
				{
					_parent = parent;
					_items = new List<MenuItem>();
				}

				public override string GetDisplayedText()
				{
					return "[" + _name + "]";
				}
			}

			private class MenuItemFunction : MenuItem
			{
				public readonly MethodInfo _function;

				public MenuItemFunction(string name, MethodInfo function) : base(name)
				{
					_function = function;
				}

				public override string GetDisplayedText()
				{
					return _name + "  [INVOKE]";
				}
			}

			private class MenuItemProperty : MenuItem
			{
				public readonly PropertyInfo _property;

				public MenuItemProperty(string name, PropertyInfo property) : base(name)
				{
					_property = property;
				}

				public override string GetDisplayedText()
				{
					return _name + "  [" + _property.GetValue(null).ToString() + "]";
				}
			}

			private class MenuItemField : MenuItem
			{
				public readonly FieldInfo _field;

				public MenuItemField(string name, FieldInfo field) : base(name)
				{
					_field = field;
				}

				public override string GetDisplayedText()
				{
					return _name + "  [" + _field.GetValue(null).ToString() + "]";
				}
			}

			private class MenuItemBack : MenuItem
			{
				public MenuItemBack() : base(null)
				{
					
				}

				public override string GetDisplayedText()
				{
					return "[Back]";
				}
			}

			private class MenuItemExit : MenuItem
			{
				public MenuItemExit() : base(null)
				{

				}

				public override string GetDisplayedText()
				{
					return "[Exit]";
				}
			}
			#endregion

			[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
			public static void Init()
			{
				BuildMenus();

				_inputActions = new DebugMenuInputActions();
				_inputActions.Menu.ToggleMenu.performed += OnInputToggleMenu;
				_inputActions.Menu.Enter.performed += OnInputEnterPressed;
				_inputActions.Menu.Left.performed += OnInputLeftPressed;
				_inputActions.Menu.Right.performed += OnInputRightPressed;
				_inputActions.Menu.Up.performed += OnInputUpPressed;
				_inputActions.Menu.Down.performed += OnInputDownPressed;
				_inputActions.Menu.Enable();
			}

			private static void BuildMenus()
			{
				_rootMenu = new Menu("Debug Menu");
				_rootMenu._items.Add(new MenuItemExit());

				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

				for (int i = 0; i < assemblies.Length; i++)
				{
					if (assemblies[i].ReflectionOnly)
						continue;

					Type[] types = null;

					try
					{
						types = assemblies[i].GetTypes();
					}
					catch
					{
						continue;
					}

					foreach (Type type in types)
					{
						//Check all methods for static paramterless functions marked with attribute
						MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

						foreach (MethodInfo method in methods)
						{
							if (method.GetParameters().Length == 0)
							{
								DebugMenuAttribute item = SystemUtils.GetAttribute<DebugMenuAttribute>(method);

								if (item != null)
								{
									AddFunction(item, method);
								}
							}
						}

						PropertyInfo[] properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

						foreach (PropertyInfo property in properties)
						{
							DebugMenuAttribute item = SystemUtils.GetAttribute<DebugMenuAttribute>(property);

							if (item != null)
							{
								AddProperty(item, property);
							}
						}

						FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

						foreach (FieldInfo field in fields)
						{
							DebugMenuAttribute item = SystemUtils.GetAttribute<DebugMenuAttribute>(field);

							if (item != null)
							{
								AddField(item, field);
							}
						}
					}
				}
			}

			private static Menu FindOrAddMenu(Menu parent, string name)
			{
				foreach (MenuItem item in parent._items)
				{
					if (item._name == name && item is Menu menu)
					{
						return menu;
					}
				}

				//Add menu
				{
					Menu menu = new Menu(name, parent);

					//Add item to go back to parent
					menu._items.Add(new MenuItemBack());

					//Add item in parent to open menu
					parent._items.Add(menu);

					return menu;
				}
			}

			private static Menu GetMenu(DebugMenuAttribute item, out string name)
			{
				if (string.IsNullOrEmpty(item.Path))
				{
					name = null;
					return _rootMenu;
				}
				else
				{
					string[] menus = item.Path.Split('/');
					Menu menu = _rootMenu;

					for (int i=0; i<menus.Length-1; i++)
					{
						menu = FindOrAddMenu(menu, menus[i]);
					}

					name = menus[menus.Length - 1];

					return menu;
				}
			}

			private static void AddFunction(DebugMenuAttribute item, MethodInfo method)
			{
				Menu menu = GetMenu(item, out string name);
				menu._items.Add(new MenuItemFunction(string.IsNullOrEmpty(name) ? method.Name : name, method));
			}

			private static void AddProperty(DebugMenuAttribute item, PropertyInfo property)
			{
				Menu menu = GetMenu(item, out string name);
				menu._items.Add(new MenuItemProperty(string.IsNullOrEmpty(name) ? property.Name : name, property));
			}

			private static void AddField(DebugMenuAttribute item, FieldInfo field)
			{
				Menu menu = GetMenu(item, out string name);
				menu._items.Add(new MenuItemField(string.IsNullOrEmpty(name) ? field.Name : name, field));
			}

			private static void RefreshMenu()
			{
				_visuals._title.text = _currentMenu._name;

				int itemIndex = 0;

				for (int i = _currentMenu._items.Count - 1; i >= 0; i--)
				{
					_visuals._items[itemIndex].text = i == _currentItemIndex ? "> " + _currentMenu._items[i].GetDisplayedText() : _currentMenu._items[i].GetDisplayedText();
					_visuals._items[itemIndex].color = i == _currentItemIndex ? _visuals._highlightedColor : _visuals._title.color;
					_visuals._items[itemIndex].gameObject.SetActive(true);
					itemIndex++;
				}

				for (int i = itemIndex; i < _visuals._items.Length; i++)
				{
					_visuals._items[i].gameObject.SetActive(false);
				}
			}

			#region Input
			private static void OnInputToggleMenu(InputAction.CallbackContext context)
			{
				if (_visuals == null)
				{
					GameObject prefab = Resources.Load<GameObject>("Debug Menu");
					_visuals = GameObject.Instantiate(prefab).GetComponent<DebugMenuVisuals>();
					GameObject.DontDestroyOnLoad(_visuals.gameObject);

					//Create items
					GameObject itemPrefab = _visuals._items[0].gameObject;

					TextMeshProUGUI[] items = new TextMeshProUGUI[_visuals._maxItems];
					items[0] = _visuals._items[0];
					
					float y = 0f;

					for  (int i=1; i< items.Length; i++)
					{
						items[i] = GameObject.Instantiate(itemPrefab, itemPrefab.transform.parent, false).GetComponent<TextMeshProUGUI>();
						y -= RectTransformUtils.GetHeight(items[i].rectTransform);
						RectTransformUtils.SetY(items[i].rectTransform, y);
					}

					_visuals._items = items;

					_visuals.gameObject.SetActive(false);
					_currentMenu = _rootMenu;
					_currentItemIndex = _currentMenu._items.Count -1;
				}

				if (_visuals.gameObject.activeSelf)
				{
					_visuals.gameObject.SetActive(false);
				}
				else
				{
					_visuals.gameObject.SetActive(true);
					RefreshMenu();
				}

			}

			private static void OnInputEnterPressed(InputAction.CallbackContext context)
			{
				if (_visuals != null && _visuals.gameObject.activeSelf)
				{
					MenuItem item = _currentMenu._items[_currentItemIndex];

					if (item is MenuItemFunction function)
					{
						function._function.Invoke(null, new object[0]);
						_visuals.gameObject.SetActive(false);
					}
					else if (item is Menu menu)
					{
						_currentMenu = menu;
						_currentItemIndex = _currentMenu._items.Count - 1;
						RefreshMenu();
					}
					else if (item is MenuItemBack)
					{
						_currentMenu = _currentMenu._parent;
						_currentItemIndex = _currentMenu._items.Count - 1;
						RefreshMenu();
					}
					else if (item is MenuItemExit)
					{
						_visuals.gameObject.SetActive(false);
					}
				}
			}

			private static void OnInputUpPressed(InputAction.CallbackContext context)
			{
				if (_visuals != null && _visuals.gameObject.activeSelf)
				{
					_currentItemIndex++;

					if (_currentItemIndex >= _currentMenu._items.Count)
						_currentItemIndex = 0;

					RefreshMenu();
				}
			}

			private static void OnInputDownPressed(InputAction.CallbackContext context)
			{
				if (_visuals != null && _visuals.gameObject.activeSelf)
				{
					_currentItemIndex--;

					if (_currentItemIndex < 0)
						_currentItemIndex = _currentMenu._items.Count - 1;

					RefreshMenu();
				}
			}

			private static void OnInputLeftPressed(InputAction.CallbackContext context)
			{
				if (_visuals != null && _visuals.gameObject.activeSelf)
				{
					MenuItem item = _currentMenu._items[_currentItemIndex];

					if (item is MenuItemField field)
					{
						if (field._field.FieldType == typeof(bool))
						{
							field._field.SetValue(null, !(bool)field._field.GetValue(null));
						}
						else if (field._field.FieldType == typeof(int))
						{
							int value = (int)field._field.GetValue(null);
							field._field.SetValue(null, value - 1);
						}
						else if (field._field.FieldType == typeof(float))
						{
							float value = (float)field._field.GetValue(null);
							field._field.SetValue(null, value - 1f);
						}
						else if (field._field.FieldType == typeof(double))
						{
							double value = (double)field._field.GetValue(null);
							field._field.SetValue(null, value - 1d);
						}
					}
					else if (item is MenuItemProperty property)
					{
						if (property._property.PropertyType == typeof(bool))
						{
							property._property.SetValue(null, !(bool)property._property.GetValue(null));
						}
						else if (property._property.PropertyType == typeof(int))
						{
							int value = (int)property._property.GetValue(null);
							property._property.SetValue(null, value - 1);
						}
						else if (property._property.PropertyType == typeof(float))
						{
							float value = (float)property._property.GetValue(null);
							property._property.SetValue(null, value - 1f);
						}
						else if (property._property.PropertyType == typeof(double))
						{
							double value = (double)property._property.GetValue(null);
							property._property.SetValue(null, value - 1d);
						}
					}

					RefreshMenu();
				}
			}

			private static void OnInputRightPressed(InputAction.CallbackContext context)
			{
				if (_visuals != null && _visuals.gameObject.activeSelf)
				{
					MenuItem item = _currentMenu._items[_currentItemIndex];

					if (item is MenuItemField field)
					{
						if (field._field.FieldType == typeof(bool))
						{
							field._field.SetValue(null, !(bool)field._field.GetValue(null));
						}
						else if (field._field.FieldType == typeof(int))
						{
							int value = (int)field._field.GetValue(null);
							field._field.SetValue(null, value + 1);
						}
						else if (field._field.FieldType == typeof(float))
						{
							float value = (float)field._field.GetValue(null);
							field._field.SetValue(null, value + 1f);
						}
						else if (field._field.FieldType == typeof(double))
						{
							double value = (double)field._field.GetValue(null);
							field._field.SetValue(null, value + 1d);
						}
					}
					else if (item is MenuItemProperty property)
					{
						if (property._property.PropertyType == typeof(bool))
						{
							property._property.SetValue(null, !(bool)property._property.GetValue(null));
						}
						else if (property._property.PropertyType == typeof(int))
						{
							int value = (int)property._property.GetValue(null);
							property._property.SetValue(null, value + 1);
						}
						else if (property._property.PropertyType == typeof(float))
						{
							float value = (float)property._property.GetValue(null);
							property._property.SetValue(null, value + 1f);
						}
						else if (property._property.PropertyType == typeof(double))
						{
							double value = (double)property._property.GetValue(null);
							property._property.SetValue(null, value + 1d);
						}
					}

					RefreshMenu();
				}
			}
			#endregion
		}
	}
}
#endif