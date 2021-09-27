#if UNITY_EDITOR

using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			public static class SerializedPropertyUtils
			{
				private const string ARRAY_START_TAG = ".Array.data[";

				#region Public Interface
				public static T GetSerializedPropertyValue<T>(SerializedProperty prop) where T : new ()
				{
					string path = prop.propertyPath.Replace(ARRAY_START_TAG, "[");
					object obj = prop.serializedObject.targetObject;
					string[] elements = path.Split('.');
					
					for (int i = 0; i < elements.Length; i++)
					{
						int arrayStartIndex = elements[i].IndexOf('[');
						int arrayEndIndex = arrayStartIndex != -1 ? elements[i].IndexOf(']') : -1;

						if (arrayEndIndex != -1)
						{
							string elementName = elements[i].Substring(0, arrayStartIndex);
							int index = Convert.ToInt32(elements[i].Substring(arrayStartIndex + 1, arrayEndIndex - arrayStartIndex - 1));

							obj = GetValue(obj, elementName, index);
						}
						else
						{
							obj = GetValue(obj, elements[i]);
						}
					}

					if (obj == null)
						obj = new T();

					return (T)obj;
				}
				
				public static void SetSerializedPropertyValue(SerializedProperty prop, object value)
				{
					Undo.RecordObjects(prop.serializedObject.targetObjects, "Change " + prop.name);

					string path = prop.propertyPath.Replace(ARRAY_START_TAG, "[");

					foreach (UnityEngine.Object obj in prop.serializedObject.targetObjects)
					{
						SetSerializedPropertyValue(obj, path, value);
						EditorUtility.SetDirty(obj);
					}
				}

				public static Type GetSerializedPropertyType(SerializedProperty prop)
				{
					string path = prop.propertyPath.Replace(ARRAY_START_TAG, "[");
					object obj = prop.serializedObject.targetObject;
					Type propertyType = null;

					if (obj != null)
					{
						string[] elements = path.Split('.');
						propertyType = obj.GetType();

						for (int i = 0; i < elements.Length; i++)
						{
							int arrayStartIndex = elements[i].IndexOf('[');
							int arrayEndIndex = arrayStartIndex != -1 ? elements[i].IndexOf(']') : -1;

							if (arrayEndIndex != -1)
							{
								string elementName = elements[i].Substring(0, arrayStartIndex);
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

				public static T GetPropertyDrawerTargetObject<T>(PropertyDrawer propertyDrawer, SerializedProperty property)
				{
					return (T)propertyDrawer.fieldInfo.GetValue(property.serializedObject.targetObject);
				}

				public static T[] GetPropertyDrawerTargetObjects<T>(PropertyDrawer propertyDrawer, SerializedProperty property)
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

				public static void SavePropertyDrawerTargetObjects<T>(PropertyDrawer propertyDrawer, SerializedProperty property, T[] newValues)
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

				public static void FlagsField(SerializedProperty property, params GUILayoutOption[] options)
				{
					if (property != null)
						property.intValue = EditorGUILayout.MaskField(new GUIContent(property.displayName), property.intValue, property.enumDisplayNames, options);
				}

				public static bool DrawSerialisedObjectProperties(SerializedObject obj, Rect rect)
				{
					EditorGUI.BeginChangeCheck();
					obj.UpdateIfRequiredOrScript();

					SerializedProperty iterator = obj.GetIterator();
					bool enterChildren = true;
					while (iterator.NextVisible(enterChildren))
					{
						using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
						{
							rect.height = EditorGUI.GetPropertyHeight(iterator);
							EditorGUI.PropertyField(rect, iterator, true);
							rect.y += rect.height;
						}
						enterChildren = false;
					}
					obj.ApplyModifiedProperties();

					return EditorGUI.EndChangeCheck();
				}

				public static float GetSerialisedObjectPropertiesHeight(SerializedObject obj)
				{
					float height = 0f;

					obj.UpdateIfRequiredOrScript();

					SerializedProperty iterator = obj.GetIterator();
					bool enterChildren = true;
					while (iterator.NextVisible(enterChildren))
					{
						using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
						{
							height += EditorGUI.GetPropertyHeight(iterator);
						}
						enterChildren = false;
					}

					return height;
				}
				#endregion

				#region Private Functinos
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

				private static object SetValue(object obj, string propertyName, object value)
				{
					if (obj == null)
						return obj;

					Type type = obj.GetType();

					while (type != null)
					{
						FieldInfo f = type.GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
						if (f != null)
						{
							f.SetValue(obj, value);
							return obj;
						}

						PropertyInfo p = type.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
						if (p != null)
						{
							p.SetValue(obj, value, null);
							return obj;
						}

						type = type.BaseType;
					}

					return obj;
				}

				private static object SetSerializedPropertyValue(object sourceObj, string propertyPath, object value)
				{
					string[] elements = propertyPath.Split('.');
					string propertyName = elements[0];

					int arrayStartIndex = propertyName.IndexOf('[');
					int arrayEndIndex = arrayStartIndex != -1 ? propertyName.IndexOf(']') : -1;

					//If this property is an array need to set values on actual element
					if (arrayEndIndex != -1)
					{
						//First get array
						string arrayPropertyName = propertyName.Substring(0, arrayStartIndex);
						Array array = GetValue(sourceObj, arrayPropertyName) as Array;
						
						//Then find index of object
						int index = Convert.ToInt32(propertyName.Substring(arrayStartIndex + 1, arrayEndIndex - arrayStartIndex - 1));
						
						//If there is a child object, update its value
						if (elements.Length > 1)
						{
							object childObject = array.GetValue(index);
							string childPropertyPath = propertyPath.Substring(propertyPath.IndexOf('.') + 1);

							value = SetSerializedPropertyValue(childObject, childPropertyPath, value);
						}

						array.SetValue(value, index);

						return SetValue(sourceObj, arrayPropertyName, array);
					}
					else
					{
						//If there is a child object, update its value
						if (elements.Length > 1)
						{
							object childObject = GetValue(sourceObj, propertyName);
							string childPropertyPath = propertyPath.Substring(propertyPath.IndexOf('.') + 1);

							value = SetSerializedPropertyValue(childObject, childPropertyPath, value);
						}

						return SetValue(sourceObj, propertyName, value);
					}
				}
				#endregion
			}
		}
	}
}

#endif