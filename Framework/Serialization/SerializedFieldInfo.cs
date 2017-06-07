using Framework.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		public struct SerializedFieldInfo
		{
			#region Private Data
			private readonly FieldInfo _fieldInfo;
			private readonly PropertyInfo _propertyInfo;
			private string _id;
			private bool _hideInEditor;
			#endregion

			#region Public Interface
			public SerializedFieldInfo(FieldInfo fieldInfo, bool hideInEditor = false)
			{
				_fieldInfo = fieldInfo;
				_propertyInfo = null;
				_id = GetId(fieldInfo);
				_hideInEditor = hideInEditor;
			}

			public SerializedFieldInfo(PropertyInfo propertyInfo, bool hideInEditor = false)
			{
				_fieldInfo = null;
				_propertyInfo = propertyInfo;
				_id = GetId(propertyInfo);
				_hideInEditor = hideInEditor;
			}

			public static implicit operator MemberInfo(SerializedFieldInfo field)
			{
				if (field._fieldInfo != null)
					return field._fieldInfo;
				else
					return field._propertyInfo;
			}

			public string GetID()
			{
				return _id;
			}

			public bool HideInEditor()
			{
				return _hideInEditor;
			}

			public Type GetFieldType()
			{
				if (_fieldInfo != null)
					return _fieldInfo.FieldType;
				else
					return _propertyInfo.PropertyType;
			}

			public void SetValue(object obj, object value)
			{
				if (_fieldInfo != null)
					_fieldInfo.SetValue(obj, value);
				else
					_propertyInfo.SetValue(obj, value, null);
			}

			public object GetValue(object obj)
			{
				try
				{
					if (_fieldInfo != null)
						return _fieldInfo.GetValue(obj);
					else
						return _propertyInfo.GetValue(obj, null);
				}
				catch
				{
					return null;
				}
			}
			
			public static SerializedFieldInfo[] GetSerializedFields(Type objType)
			{
				//Get all public variables that ARENT marked with NonSerialized
				//AND all non public variables that are marked with SerializeField

				//Find all fields in type
				List<SerializedFieldInfo> serializedFields = new List<SerializedFieldInfo>();

				BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

				//First find fields
				FieldInfo[] fields = objType.GetFields(bindingAttr);
				foreach (FieldInfo field in fields)
				{
					if ((field.IsPublic && !field.IsNotSerialized) || (!field.IsPublic && SystemUtils.GetAttribute<SerializeField>(field) != null))
					{
						bool hideInEditor = SystemUtils.GetAttribute<HideInInspector>(field) != null;
						SerializedFieldInfo serializedField = new SerializedFieldInfo(field, hideInEditor);
						serializedFields.Add(serializedField);
					}
				}

				//Then find all properties marked with SerializeField attribute?
				PropertyInfo[] properties = objType.GetProperties(bindingAttr);
				foreach (PropertyInfo property in properties)
				{
					if (SystemUtils.GetAttribute<SerializeField>(property) != null)
					{
						bool hideInEditor = SystemUtils.GetAttribute<HideInInspector>(property) != null;
						SerializedFieldInfo serializedField = new SerializedFieldInfo(property, hideInEditor);
						serializedFields.Add(serializedField);
					}
				}

				return serializedFields.ToArray();
			}

			public static bool FindSerializedField(Type objType, string id, out SerializedFieldInfo field)
			{
				SerializedFieldInfo[] serializedFields = GetSerializedFields(objType);
				foreach (SerializedFieldInfo serializedField in serializedFields)
				{
					if (serializedField.GetID() == id)
					{
						field = serializedField;
						return true;
					}
				}

				field = new SerializedFieldInfo();
				return false;
			}

			public static object[] GetSerializedFieldInstances(object obj)
			{
				SerializedFieldInfo[] serializedFields = GetSerializedFields(obj.GetType());
				List<object> fieldInstances = new List<object>();

				foreach (SerializedFieldInfo serializedField in serializedFields)
				{
					if (serializedField.GetFieldType().IsArray)
					{
						object[] array = (object[])serializedField.GetValue(obj);
						if (array != null)
							fieldInstances.AddRange(array);
					}
					else
					{
						object newObj = serializedField.GetValue(obj);
						if (newObj != null)
							fieldInstances.Add(newObj);
					}
				}

				return fieldInstances.ToArray();
			}

			public static object GetSerializedFieldInstance(object obj, string id)
			{
				SerializedFieldInfo fieldInfo;

				if (FindSerializedField(obj.GetType(), id, out fieldInfo))
				{
					return fieldInfo.GetValue(obj);
				}

				return null;
			}
			#endregion

			#region Private Functions
			private static string GetId(MemberInfo info)
			{
				string id = info.Name;
				return id.TrimStart('_');
			}
			#endregion
		}
	}
}