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
		public static class DebugMenu
		{

			private static DebugItemMenu _rootMenu;
			private static DebugMenuInputActions _inputActions;
			private static DebugMenuVisuals _visuals;
			private static DebugItemMenu _currentMenu;
			private static int _currentItemIndex;

			#region Menu Items
			private abstract class DebugItem
			{
				public readonly string _name;

				public DebugItem(string name)
				{
					_name = name;
				}

				public abstract string GetDisplayedText();
			}

			private class DebugItemMenu : DebugItem
			{
				public DebugItemMenu _parent;
				public List<DebugItem> _items;

				public DebugItemMenu(string name, DebugItemMenu parent = null) : base(name)
				{
					_parent = parent;
					_items = new List<DebugItem>();
				}

				public override string GetDisplayedText()
				{
					return "[" + _name + "]";
				}
			}

			private class DebugItemFunction : DebugItem
			{
				public readonly MethodInfo _function;

				public DebugItemFunction(string name, MethodInfo function) : base(name)
				{
					_function = function;
				}

				public override string GetDisplayedText()
				{
					return _name + "  [INVOKE]";
				}
			}

			private class DebugItemProperty : DebugItem
			{
				public readonly PropertyInfo _property;

				public DebugItemProperty(string name, PropertyInfo property) : base(name)
				{
					_property = property;
				}

				public override string GetDisplayedText()
				{
					return _name + "  [" + _property.GetValue(null).ToString() + "]";
				}
			}

			private class DebugItemField : DebugItem
			{
				public readonly FieldInfo _field;

				public DebugItemField(string name, FieldInfo field) : base(name)
				{
					_field = field;
				}

				public override string GetDisplayedText()
				{
					return _name + "  [" + _field.GetValue(null).ToString() + "]";
				}
			}

			private class DebugItemBack : DebugItem
			{
				public DebugItemBack() : base(null)
				{
					
				}

				public override string GetDisplayedText()
				{
					return "[Back]";
				}
			}

			private class DebugItemExit : DebugItem
			{
				public DebugItemExit() : base(null)
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
				_rootMenu = new DebugItemMenu("Debug Menu");
				_rootMenu._items.Add(new DebugItemExit());

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
					catch (Exception e)
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
								DebugMenuItemAttribute item = SystemUtils.GetAttribute<DebugMenuItemAttribute>(method);

								if (item != null)
								{
									AddFunction(item, method);
								}
							}
						}

						PropertyInfo[] properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

						foreach (PropertyInfo property in properties)
						{
							DebugMenuItemAttribute item = SystemUtils.GetAttribute<DebugMenuItemAttribute>(property);

							if (item != null)
							{
								AddProperty(item, property);
							}
						}

						FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

						foreach (FieldInfo field in fields)
						{
							DebugMenuItemAttribute item = SystemUtils.GetAttribute<DebugMenuItemAttribute>(field);

							if (item != null)
							{
								AddField(item, field);
							}
						}
					}
				}
			}

			private static DebugItemMenu FindOrAddMenu(DebugItemMenu parent, string name)
			{
				foreach (DebugItem item in parent._items)
				{
					if (item._name == name && item is DebugItemMenu menu)
					{
						return menu;
					}
				}

				//Add menu
				{
					DebugItemMenu menu = new DebugItemMenu(name, parent);

					//Add item to go back to parent
					menu._items.Add(new DebugItemBack());

					//Add item in parent to open menu
					parent._items.Add(menu);

					return menu;
				}
			}

			private static DebugItemMenu GetMenu(DebugMenuItemAttribute item, out string name)
			{
				if (string.IsNullOrEmpty(item.Path))
				{
					name = null;
					return _rootMenu;
				}
				else
				{
					string[] menus = item.Path.Split('/');
					DebugItemMenu menu = _rootMenu;

					for (int i=0; i<menus.Length-1; i++)
					{
						menu = FindOrAddMenu(menu, menus[i]);
					}

					name = menus[menus.Length - 1];

					return menu;
				}
			}

			private static void AddFunction(DebugMenuItemAttribute item, MethodInfo method)
			{
				DebugItemMenu menu = GetMenu(item, out string name);
				menu._items.Add(new DebugItemFunction(string.IsNullOrEmpty(name) ? method.Name : name, method));
			}

			private static void AddProperty(DebugMenuItemAttribute item, PropertyInfo property)
			{
				DebugItemMenu menu = GetMenu(item, out string name);
				menu._items.Add(new DebugItemProperty(string.IsNullOrEmpty(name) ? property.Name : name, property));
			}

			private static void AddField(DebugMenuItemAttribute item, FieldInfo field)
			{
				DebugItemMenu menu = GetMenu(item, out string name);
				menu._items.Add(new DebugItemField(string.IsNullOrEmpty(name) ? field.Name : name, field));
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
					DebugItem item = _currentMenu._items[_currentItemIndex];

					if (item is DebugItemFunction function)
					{
						function._function.Invoke(null, new object[0]);
					}
					else if (item is DebugItemMenu menu)
					{
						_currentMenu = menu;
						_currentItemIndex = _currentMenu._items.Count - 1;
					}
					else if (item is DebugItemBack)
					{
						_currentMenu = _currentMenu._parent;
						_currentItemIndex = _currentMenu._items.Count - 1;
					}
					else if (item is DebugItemExit)
					{
						_visuals.gameObject.SetActive(false);
					}
				}

				RefreshMenu();
			}

			private static void OnInputUpPressed(InputAction.CallbackContext context)
			{
				_currentItemIndex++;

				if (_currentItemIndex >= _currentMenu._items.Count)
					_currentItemIndex = 0;

				RefreshMenu();
			}

			private static void OnInputDownPressed(InputAction.CallbackContext context)
			{
				_currentItemIndex--;

				if (_currentItemIndex < 0)
					_currentItemIndex = _currentMenu._items.Count - 1;

				RefreshMenu();
			}

			private static void OnInputLeftPressed(InputAction.CallbackContext context)
			{
				if (_visuals != null && _visuals.gameObject.activeSelf)
				{
					DebugItem item = _currentMenu._items[_currentItemIndex];

					if (item is DebugItemField field)
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
					else if (item is DebugItemProperty property)
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
				}

				RefreshMenu();
			}

			private static void OnInputRightPressed(InputAction.CallbackContext context)
			{
				if (_visuals != null && _visuals.gameObject.activeSelf)
				{
					DebugItem item = _currentMenu._items[_currentItemIndex];

					if (item is DebugItemField field)
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
					else if (item is DebugItemProperty property)
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
				}

				RefreshMenu();
			}
			#endregion
		}
	}
}

#endif