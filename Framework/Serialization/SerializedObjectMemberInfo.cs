using Framework.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		public struct SerializedObjectMemberInfo
		{
			#region Private Data
			private readonly MemberInfo _memberInfo;
			private readonly string _id;
			private readonly bool _hideInEditor;
			#endregion

			#region Public Interface
			public SerializedObjectMemberInfo(FieldInfo fieldInfo, bool hideInEditor = false)
			{
				_memberInfo = fieldInfo;
				_id = GetId(fieldInfo);
				_hideInEditor = hideInEditor;
			}

			public SerializedObjectMemberInfo(PropertyInfo propertyInfo, bool hideInEditor = false)
			{
				_memberInfo = null;
				_id = GetId(propertyInfo);
				_hideInEditor = hideInEditor;
			}

			public static implicit operator MemberInfo(SerializedObjectMemberInfo field)
			{
				return field._memberInfo;
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
				if (_memberInfo != null)
				{
					if (_memberInfo is FieldInfo)
						return ((FieldInfo)_memberInfo).FieldType;
					else
						return ((PropertyInfo)_memberInfo).PropertyType;
				}

				return null;
			}

			public void SetValue(object obj, object value)
			{
				if (_memberInfo != null)
				{
					if (_memberInfo is FieldInfo)
						((FieldInfo)_memberInfo).SetValue(obj, value);
					else
						((PropertyInfo)_memberInfo).SetValue(obj, value, null);
				}
			}

			public object GetValue(object obj)
			{
				try
				{
					if (_memberInfo != null)
					{
						if (_memberInfo is FieldInfo)
							return ((FieldInfo)_memberInfo).GetValue(obj);
						else
							return ((PropertyInfo)_memberInfo).GetValue(obj, null);
					}
				}
				catch
				{
					
				}

				return null;
			}
			
			public static SerializedObjectMemberInfo[] GetSerializedFields(Type objType)
			{
				//Get all public variables that ARENT marked with NonSerialized
				//AND all non public variables that are marked with SerializeField

				//Find all fields in type
				List<SerializedObjectMemberInfo> serializedFields = new List<SerializedObjectMemberInfo>();

				BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

				//First find fields
				FieldInfo[] fields = objType.GetFields(bindingAttr);
				foreach (FieldInfo field in fields)
				{
					if ((field.IsPublic && !field.IsNotSerialized) || (!field.IsPublic && SystemUtils.GetAttribute<SerializeField>(field) != null))
					{
						bool hideInEditor = SystemUtils.GetAttribute<HideInInspector>(field) != null;
						SerializedObjectMemberInfo serializedField = new SerializedObjectMemberInfo(field, hideInEditor);
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
						SerializedObjectMemberInfo serializedField = new SerializedObjectMemberInfo(property, hideInEditor);
						serializedFields.Add(serializedField);
					}
				}

				return serializedFields.ToArray();
			}

			public static bool FindSerializedField(Type objType, string id, out SerializedObjectMemberInfo field)
			{
				SerializedObjectMemberInfo[] serializedFields = GetSerializedFields(objType);
				foreach (SerializedObjectMemberInfo serializedField in serializedFields)
				{
					if (serializedField.GetID() == id)
					{
						field = serializedField;
						return true;
					}
				}

				field = new SerializedObjectMemberInfo();
				return false;
			}

			public static object[] GetSerializedFieldInstances(object obj)
			{
				SerializedObjectMemberInfo[] serializedFields = GetSerializedFields(obj.GetType());
				List<object> fieldInstances = new List<object>();

				foreach (SerializedObjectMemberInfo serializedField in serializedFields)
				{
					if (serializedField.GetFieldType().IsArray)
					{
						Array array = (Array)serializedField.GetValue(obj);
						
						if (array != null)
						{
							for (int i=0; i<array.Length; i++)
							{
								fieldInstances.Add(array.GetValue(i));
							}
						}	
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
				SerializedObjectMemberInfo fieldInfo;

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