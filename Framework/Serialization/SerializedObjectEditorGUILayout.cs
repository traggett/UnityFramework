#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Utils;

	namespace Serialization
	{
		public static class SerializedObjectEditorGUILayout
		{
			#region Private Data
			private static Dictionary<Type, SerializedObjectEditorAttribute.RenderPropertiesDelegate> _editorMap;
			#endregion

			#region Public Interface
			public static T ObjectField<T>(T obj, string label)
			{
				bool dataChanged;
				return ObjectField(obj, label, out dataChanged);
			}

			public static T ObjectField<T>(T obj, string label, out bool dataChanged)
			{
				return ObjectField(obj, new GUIContent(label), out dataChanged);
			}

			//The equivalent of a SerializedPropertyField but for objects serialized using Xml.
			public static T ObjectField<T>(T obj, GUIContent label, out bool dataChanged)
			{
				//If object is an array show an editable array field
				if (typeof(T).IsArray)
				{
					bool arrayChanged = false;
					Array arrayObj = obj as Array;
					arrayObj = ArrayField(label, arrayObj, typeof(T).GetElementType(), ref arrayChanged);

					if (arrayChanged)
					{
						dataChanged = true;
						return (T)(arrayObj as object);
					}

					dataChanged = false;
					return obj;
				}

				//If the object is a ICustomEditable then just need to call its render properties function.
				if (obj != null && obj is ICustomEditorInspector)
				{
					dataChanged = ((ICustomEditorInspector)obj).RenderObjectProperties(label);
					return obj;
				}

				Type objType = obj == null ? typeof(T) : obj.GetType();

				//Otherwise check the class has a object editor class associated with it.
				SerializedObjectEditorAttribute.RenderPropertiesDelegate renderPropertiesDelegate = GetEditorDelegateForObject(objType);
				if (renderPropertiesDelegate != null)
				{
					//If it has one then just need to call its render properties function.
					return (T)renderPropertiesDelegate(obj, label, out dataChanged);
				}

				//Otherwise loop through each xml field in object and render each as a property field
				{
					dataChanged = false;
					SerializedFieldInfo[] serializedFields = SerializedFieldInfo.GetSerializedFields(objType);
					foreach (SerializedFieldInfo serializedField in serializedFields)
					{
						if (!serializedField.HideInEditor())
						{
							//Create GUIContent for label and optional tooltip
							string fieldName = StringUtils.FromCamelCase(serializedField.GetID());
							TooltipAttribute fieldToolTipAtt = SystemUtils.GetAttribute<TooltipAttribute>(serializedField);
							GUIContent labelContent = fieldToolTipAtt != null ? new GUIContent(fieldName, fieldToolTipAtt.tooltip) : new GUIContent(fieldName);

							bool fieldChanged;
							object nodeFieldObject = serializedField.GetValue(obj);

							if (serializedField.GetFieldType().IsArray)
							{
								fieldChanged = false;
								nodeFieldObject = ArrayField(labelContent, nodeFieldObject as Array, serializedField.GetFieldType().GetElementType(), ref fieldChanged);
							}
							else
							{
								nodeFieldObject = ObjectField(nodeFieldObject, labelContent, out fieldChanged);
							}
							
							if (fieldChanged)
							{
								dataChanged = true;
								serializedField.SetValue(obj, nodeFieldObject);
							}
						}
					}

					return obj;
				}
			}
			#endregion

			#region Private Functions
			private static Array ArrayField(GUIContent label, Array _array, Type arrayType, ref bool dataChanged)
			{
				dataChanged = false;
				
				label.text += " (" + arrayType.Name + ")";

				bool editorFoldout = EditorGUILayout.Foldout(true, label);

				if (editorFoldout)
				{
					int origIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel++;

					int origLength = _array != null ? _array.Length : 0;

					int length = EditorGUILayout.IntField("Length", origLength);
					length = Math.Max(length, 0);

					if (length < origLength)
					{
						Array newArray = Array.CreateInstance(arrayType, length);
						Array.Copy(_array, newArray, length);
						_array = newArray;
						dataChanged = true;
					}
					else if (length > origLength)
					{
						Array newArray = Array.CreateInstance(arrayType, length);

						if (origLength > 0)
							Array.Copy(_array, newArray, origLength);

						for (int i = origLength; i < length; i++)
						{
							if (!arrayType.IsInterface && !arrayType.IsAbstract)
								newArray.SetValue(Activator.CreateInstance(arrayType, true), i);
						}

						_array = newArray;
						dataChanged = true;
					}

					if (!dataChanged && _array != null)
					{
						for (int i = 0; i < _array.Length; i++)
						{
							bool elementChanged;
							_array.SetValue(ObjectField(_array.GetValue(i), "Element " + i, out elementChanged), i);
							dataChanged |= elementChanged;
						}
					}

					EditorGUI.indentLevel = origIndent;
				}

				return _array;
			}

			private static SerializedObjectEditorAttribute.RenderPropertiesDelegate GetEditorDelegateForObject(Type objectType)
			{
				BuildEditorMap();

				if (objectType.IsEnum)
				{
					if (objectType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
					{
						objectType = typeof(FlagsAttribute);
					}
					else
					{
						objectType = typeof(Enum);
					}
				}
				else if (objectType.IsGenericType)
				{
					objectType = objectType.GetGenericTypeDefinition();
				}

				SerializedObjectEditorAttribute.RenderPropertiesDelegate editor;

				if (_editorMap.TryGetValue(objectType, out editor))
				{
					return editor;
				}

				return null;
			}

			private static void BuildEditorMap()
			{
				if (_editorMap == null)
				{
					_editorMap = new Dictionary<Type, SerializedObjectEditorAttribute.RenderPropertiesDelegate>();

					Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

					foreach (Assembly assembly in assemblies)
					{
						Type[] types = assembly.GetTypes();

						foreach (Type type in types)
						{
							SerializedObjectEditorAttribute attribute = SystemUtils.GetAttribute<SerializedObjectEditorAttribute>(type);

							if (attribute != null)
							{
								if (_editorMap.ContainsKey(attribute.ObjectType))
								{
									throw new Exception("Can't initialize XmlObjectEditorAttribute for " + type.FullName + " as already have a editor for type " + attribute.ObjectType);
								}

								_editorMap[attribute.ObjectType] = SystemUtils.GetStaticMethodAsDelegate<SerializedObjectEditorAttribute.RenderPropertiesDelegate>(type, attribute.OnRenderPropertiesMethod);
							}
						}
					}
				}
			}
			#endregion
		}
	}
}

#endif