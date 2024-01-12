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
		public static class SerializationEditorGUILayout
		{
			#region Private Data
			private static Dictionary<Type, SerializedObjectEditorAttribute.RenderPropertiesDelegate> _editorMap;
			#endregion

			#region Public Interface
			public static T ObjectField<T>(T obj, string label, GUIStyle style = null, params GUILayoutOption[] options)
			{
				bool dataChanged = false;
				return ObjectField(obj, label, ref dataChanged);
			}

			public static T ObjectField<T>(T obj, GUIContent label, GUIStyle style = null, params GUILayoutOption[] options)
			{
				bool dataChanged = false;
				return ObjectField(obj, label, ref dataChanged);
			}

			public static T ObjectField<T>(T obj, string label, ref bool dataChanged, GUIStyle style = null, params GUILayoutOption[] options)
			{
				return ObjectField(obj, new GUIContent(label), ref dataChanged);
			}

			//The equivalent of a SerializedPropertyField but for objects serialized using Xml.
			public static T ObjectField<T>(T obj, GUIContent label, ref bool dataChanged, GUIStyle style = null, params GUILayoutOption[] options)
			{
				return (T)ObjectField(obj, obj != null ? obj.GetType() : typeof(T), label, ref dataChanged, style, options);
			}

			public static object ObjectField(object obj, Type objType, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
			{
				//If object is an array show an editable array field
				if (objType.IsArray)
				{
					bool arrayChanged = false;
					Array arrayObj = obj as Array;
					arrayObj = ArrayField(label, arrayObj, objType.GetElementType(), ref arrayChanged, style, options);

					if (arrayChanged)
					{
						dataChanged = true;
						return arrayObj as object;
					}

					return obj;
				}

				//If the object is a ICustomEditable then just need to call its render properties function.
				if (typeof(ICustomEditorInspector).IsAssignableFrom(objType))
				{
					//If obj is null then need to create new instance for editor
					if (obj == null)
					{
						ConstructorInfo constructor = objType.GetConstructor(Type.EmptyTypes);
						if (constructor != null)
							obj = constructor.Invoke(null);
						else
							throw new Exception("Classes that implement ICustomEditorInspector need a parameterless constructor");
					}

					if (obj != null)
						dataChanged |= ((ICustomEditorInspector)obj).RenderObjectProperties(label);

					return obj;
				}

				//Otherwise check the class has a object editor class associated with it.
				SerializedObjectEditorAttribute.RenderPropertiesDelegate renderPropertiesDelegate = GetEditorDelegateForObject(objType);
				if (renderPropertiesDelegate != null)
				{
					//If it has one then just need to call its render properties function.
					return renderPropertiesDelegate(obj, label, ref dataChanged, style, options);
				}

				//Otherwise render all the objects memebers as object fields
				return RenderObjectMemebers(obj, objType, ref dataChanged, style, options);
			}

			public static object RenderObjectMemebers(object obj, Type objType, ref bool dataChanged, GUIStyle style = null, params GUILayoutOption[] options)
			{
				SerializedObjectMemberInfo[] serializedFields = SerializedObjectMemberInfo.GetSerializedFields(objType);

				for (int i = 0; i < serializedFields.Length; i++)
				{
					if (!serializedFields[i].HideInEditor())
					{
						//Create GUIContent for label and optional tooltip
						string fieldName = StringUtils.FromCamelCase(serializedFields[i].GetName());
						TooltipAttribute fieldToolTipAtt = SystemUtils.GetAttribute<TooltipAttribute>(serializedFields[i]);
						GUIContent labelContent = fieldToolTipAtt != null ? new GUIContent(fieldName, fieldToolTipAtt.tooltip) : new GUIContent(fieldName);

						bool fieldChanged = false;
						object nodeFieldObject = serializedFields[i].GetValue(obj);

						if (serializedFields[i].GetFieldType().IsArray)
						{
							nodeFieldObject = ArrayField(labelContent, nodeFieldObject as Array, serializedFields[i].GetFieldType().GetElementType(), ref fieldChanged, style, options);
						}
						else
						{
							nodeFieldObject = ObjectField(nodeFieldObject, nodeFieldObject != null ? nodeFieldObject.GetType() : serializedFields[i].GetFieldType(), labelContent, ref fieldChanged, style, options);
						}

						if (fieldChanged)
						{
							dataChanged = true;
							serializedFields[i].SetValue(obj, nodeFieldObject);
						}
					}
				}

				return obj;
			}
			#endregion

			#region Private Functions	
			private static Array ArrayField(GUIContent label, Array array, Type arrayType, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
			{
				label.text += " (" + SystemUtils.GetTypeName(arrayType) + ")";

				bool editorFoldout = EditorGUILayout.Foldout(true, label);

				if (editorFoldout)
				{
					int origIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel++;

					int origLength = array != null ? array.Length : 0;

					int length = EditorGUILayout.IntField("Length", origLength);
					length = Math.Max(length, 0);

					if (length < origLength)
					{
						Array newArray = Array.CreateInstance(arrayType, length);
						Array.Copy(array, newArray, length);
						array = newArray;
						dataChanged = true;
					}
					else if (length > origLength)
					{
						Array newArray = Array.CreateInstance(arrayType, length);

						if (origLength > 0)
							Array.Copy(array, newArray, origLength);

						for (int i = origLength; i < length; i++)
						{
							if (!arrayType.IsInterface && !arrayType.IsAbstract)
								newArray.SetValue(Activator.CreateInstance(arrayType, true), i);
						}

						array = newArray;
						dataChanged = true;
					}

					if (!dataChanged && array != null)
					{
						for (int i = 0; i < array.Length; i++)
						{
							bool elementChanged = false;
							object elementObj = array.GetValue(i);
							Type elementType = elementObj != null ? elementObj.GetType() : arrayType;
							elementObj = ObjectField(elementObj, elementType, new GUIContent("Element " + i), ref elementChanged, style, options);
							if (elementChanged)
							{
								array.SetValue(elementObj, i);
								dataChanged = true;
							}
						}
					}

					EditorGUI.indentLevel = origIndent;
				}

				return array;
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

					Type[] types = SystemUtils.GetAllTypes();

					foreach (Type type in types)
					{
						SerializedObjectEditorAttribute attribute = SystemUtils.GetAttribute<SerializedObjectEditorAttribute>(type);

						if (attribute != null)
						{
							SerializedObjectEditorAttribute.RenderPropertiesDelegate func = SystemUtils.GetStaticMethodAsDelegate<SerializedObjectEditorAttribute.RenderPropertiesDelegate>(type, attribute.OnRenderPropertiesMethod);

							_editorMap[attribute.ObjectType] = func;


							if (attribute.UseForChildTypes)
							{
								Type[] childTypes = SystemUtils.GetAllSubTypes(attribute.ObjectType);

								foreach (Type childType in childTypes)
								{
									if (!_editorMap.ContainsKey(childType))
									{
										_editorMap[childType] = func;
									}
								}
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