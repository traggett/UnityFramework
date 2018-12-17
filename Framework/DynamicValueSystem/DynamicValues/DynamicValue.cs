using UnityEngine;

using System.Reflection;
using System.Collections.Generic;

namespace Framework
{
	using Utils;

	namespace DynamicValueSystem
	{
		//Class that allows a value to either be defined normally as a static member or be dynamically fetched from an IValueSource
		public abstract class DynamicValue<T> : IValueSource<T>, ISerializationCallbackReceiver
		{
			public enum eSourceType
			{
				Static,
				SourceObject,
				SourceMember,
				SourceDynamicMember,
			}

			[SerializeField]
			protected T _value;
			[SerializeField]
			private Object _sourceObject = null;
			[SerializeField]
			private string _sourceObjectMemberName = null;
			[SerializeField]
			private int _sourceObjectMemberIndex = -1;
			[SerializeField]
			private eSourceType _sourceType = eSourceType.Static;

			//Runtime non serialized cached data (ideally this would be a union)
			private struct SourceObject
			{
				public FieldInfo _fieldInfo;
				public IValueSource<T> _valueSource;
				public IValueSourceContainer _dynamicValueSource;
			}
			private SourceObject _sourceObjectData;

			public static implicit operator T(DynamicValue<T> property)
			{
				return property.GetValue();
			}

			public static FieldInfo[] GetDynamicValueFields(object obj)
			{
				if (obj != null)
				{
					List<FieldInfo> fieldInfo = new List<FieldInfo>();
					FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

					foreach (FieldInfo field in fields)
					{
						if (SystemUtils.IsTypeOf(typeof(IValueSource<T>), field.FieldType))
						{
							fieldInfo.Add(field);
						}
					}

					return fieldInfo.ToArray();
				}

				return null;
			}

			#region IValueSource<T>
			public T GetValue()
			{
				switch (_sourceType)
				{
					case eSourceType.SourceObject: return _sourceObjectData._valueSource.GetValue();
					case eSourceType.SourceMember: return ((IValueSource<T>)_sourceObjectData._fieldInfo.GetValue(_sourceObject)).GetValue();
					case eSourceType.SourceDynamicMember: return ((IValueSource<T>)_sourceObjectData._dynamicValueSource.GetValueSource(_sourceObjectMemberIndex)).GetValue();
					default: return _value;
				}
			}
			#endregion

			#region ISerializationCallbackReceiver
			public void OnBeforeSerialize()
			{

			}

			public void OnAfterDeserialize()
			{
				//Cache casted objects or field info
				switch (_sourceType)
				{
					case eSourceType.Static: break;
					case eSourceType.SourceObject:
						{
							_sourceObjectData._valueSource = (IValueSource<T>)_sourceObject; 
						}
						break;
					case eSourceType.SourceMember:
						{
							FieldInfo[] fields = GetDynamicValueFields(_sourceObject);

							if (fields != null)
							{
								foreach (FieldInfo field in fields)
								{
									if (field.Name == _sourceObjectMemberName)
									{
										_sourceObjectData._fieldInfo = field;
										break;
									}
								}
							}
						}
						break;
					case eSourceType.SourceDynamicMember:
						{
							_sourceObjectData._dynamicValueSource = (IValueSourceContainer)_sourceObject;
						}
						break;
					}
			}
			#endregion
		}
	}
}