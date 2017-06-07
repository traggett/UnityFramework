using System;
using System.Reflection;

namespace Engine
{
	namespace JSON
	{
		//Represents either a field or property on an object that has the JSONField attribute.
		public struct JSONField
		{
			private readonly JSONFieldAttribute _attribute;
			private readonly FieldInfo _fieldInfo;
			private readonly PropertyInfo _propertyInfo;
			private string _id;

			public JSONField(JSONFieldAttribute attribute, FieldInfo fieldInfo)
			{
				_attribute = attribute;
				_fieldInfo = fieldInfo;
				_propertyInfo = null;
				_id = GetId(attribute, fieldInfo);
			}

			public JSONField(JSONFieldAttribute attribute, PropertyInfo propertyInfo)
			{
				_attribute = attribute;
				_fieldInfo = null;
				_propertyInfo = propertyInfo;
				_id = GetId(attribute, propertyInfo);
			}

			public static implicit operator MemberInfo(JSONField field)
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
				return _attribute.HideInEditor;
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
				if (_fieldInfo != null)
					return _fieldInfo.GetValue(obj);
				else
					return _propertyInfo.GetValue(obj, null);
			}

			private static string GetId(JSONFieldAttribute attribute, MemberInfo info)
			{
				if (string.IsNullOrEmpty(attribute.ID))
				{
					string id = info.Name;
					return id.TrimStart('_');
				}
				else
					return attribute.ID;
			}
		}
	}
}