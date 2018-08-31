#if UNITY_EDITOR

using System;
using System.Collections;
using System.Reflection;
using UnityEditor;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			public static class SerializedPropertyUtils
			{
				public static Type GetSerializedPropertyType(SerializedProperty prop)
				{
					string path = prop.propertyPath.Replace(".Array.data[", "[");
					object obj = prop.serializedObject.targetObject;
					Type propertyType = null;

					if (obj != null)
					{
						string[] elements = path.Split('.');
						propertyType = obj.GetType();

						for (int i = 0; i < elements.Length; i++)
						{
							if (elements[i].Contains("["))
							{
								string elementName = elements[i].Substring(0, elements[i].IndexOf("["));
								propertyType = GetPropertyElementType(propertyType, elementName).GetElementType();
							}
							else
							{
								propertyType = GetPropertyElementType(propertyType, elements[i]);
							}
						}
					}

					return propertyType;
				}

				public static T GetSerializedPropertyValue<T>(SerializedProperty prop)
				{
					string path = prop.propertyPath.Replace(".Array.data[", "[");
					object obj = prop.serializedObject.targetObject;
					string[] elements = path.Split('.');

					for (int i = 0; i < elements.Length; i++)
					{
						if (elements[i].Contains("["))
						{
							string elementName = elements[i].Substring(0, elements[i].IndexOf("["));
							int index = Convert.ToInt32(elements[i].Substring(elements[i].IndexOf("[")).Replace("[", "").Replace("]", ""));
							obj = GetValue(obj, elementName, index);
						}
						else
						{
							obj = GetValue(obj, elements[i]);
						}
					}
					return (T)obj;
				}
                
                public static void SetSerializedPropertyValue(SerializedProperty prop, object value)
				{
					string path = prop.propertyPath.Replace(".Array.data[", "[");
					object obj = prop.serializedObject.targetObject;
					string[] elements = path.Split('.');

					if (elements.Length > 1)
					{
						for (int i = elements.Length - 2; i >= 0; i--)
						{
							if (elements[i].Contains("["))
							{
								string elementName = elements[i].Substring(0, elements[i].IndexOf("["));
								int index = Convert.ToInt32(elements[i].Substring(elements[i].IndexOf("[")).Replace("[", "").Replace("]", ""));
                                Array array = GetValue(obj, elementName) as Array;

                                object elementObj = array.GetValue(index);
                                value = SetValue(elementObj, elements[i + 1], value);
                                array.SetValue(value, index);
                            }
							else
							{
								object elementObj = GetValue(obj, elements[i]);
								value = SetValue(elementObj, elements[i + 1], value);
							}
						}
					}
                    else
                    {
                        SetValue(obj, elements[0], value);
                    }
				}

				public static T GetPropertyDrawerTargetObject<T>(PropertyDrawer propertyDrawer, SerializedProperty property)
				{
					return (T)propertyDrawer.fieldInfo.GetValue(property.serializedObject.targetObject);
				}

				public static T[] GetSelectedPropertyDrawerTargetObject<T>(PropertyDrawer propertyDrawer, SerializedProperty property)
				{
					T[] selectedStructs = new T[property.serializedObject.targetObjects.Length];

					for (int i = 0; i < property.serializedObject.targetObjects.Length; i++)
					{
						selectedStructs[i] = (T)propertyDrawer.fieldInfo.GetValue(property.serializedObject.targetObjects[i]);
					}

					return selectedStructs;
				}

				public static void SavePropertyDrawerTargetObject<T>(PropertyDrawer propertyDrawer, SerializedProperty property, T newValue)
				{
					propertyDrawer.fieldInfo.SetValue(property.serializedObject.targetObject, newValue);
				}

				public static void SaveSelectedPropertyDrawerTargetObject<T>(PropertyDrawer propertyDrawer, SerializedProperty property, T[] newValues)
				{
					for (int i = 0; i < property.serializedObject.targetObjects.Length; i++)
					{
						propertyDrawer.fieldInfo.SetValue(property.serializedObject.targetObjects[i], newValues[i]);
					}
				}

				public static void OnPropertyDrawerTargetObjectsChanged(SerializedProperty property, string info)
				{
					Undo.RecordObject(property.serializedObject.targetObject, info);

					for (int i = 0; i < property.serializedObject.targetObjects.Length; i++)
					{
						Undo.RecordObject(property.serializedObject.targetObjects[i], info);
					}
				}

				private static Type GetPropertyElementType(Type type, string elementName)
				{
					while (type != null)
					{
						FieldInfo f = type.GetField(elementName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
						if (f != null)
							return f.FieldType;

						PropertyInfo p = type.GetProperty(elementName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
						if (p != null)
							return p.PropertyType;

						type = type.BaseType;
					}

					return null;
				}

				private static object GetValue(object source, string elementName)
				{
					if (source == null)
						return null;
					Type type = source.GetType();

					while (type != null)
					{
						FieldInfo f = type.GetField(elementName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
						if (f != null)
							return f.GetValue(source);

						PropertyInfo p = type.GetProperty(elementName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
						if (p != null)
							return p.GetValue(source, null);

						type = type.BaseType;
					}
					return null;
				}

				private static object GetValue(object source, string elementName, int index)
				{
					IEnumerable enumerable = GetValue(source, elementName) as IEnumerable;
					if (enumerable == null) return null;
					IEnumerator enm = enumerable.GetEnumerator();

					for (int i = 0; i <= index; i++)
					{
						if (!enm.MoveNext())
							return null;
					}

					return enm.Current;
				}

				private static object SetValue(object obj, string elementName, object value)
				{
					if (obj == null)
						return obj;

					Type type = obj.GetType();

					while (type != null)
					{
						FieldInfo f = type.GetField(elementName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
						if (f != null)
						{
							f.SetValue(obj, value);
							return obj;
						}

						PropertyInfo p = type.GetProperty(elementName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
						if (p != null)
						{
							p.SetValue(obj, value, null);
							return obj;
						}

						type = type.BaseType;
					}

					return obj;
				}
			}
		}
	}
}

#endif