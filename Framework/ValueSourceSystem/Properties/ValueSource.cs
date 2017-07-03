using System;
using UnityEngine;
using System.Reflection;
using Framework.Utils;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	namespace ValueSourceSystem
	{
		//Class that allows a value to either be defined as a member or be calculated from other source.
		public abstract class ValueSource<T> : IValueSource<T>, ISerializationCallbackReceiver
		{
			[SerializeField]
			protected T _value;
			[SerializeField]
			protected UnityEngine.Object _sourceObject;
			[SerializeField]
			protected string _sourceObjectMember;
			//Cached value source
			private IValueSource<T> _source;

#if UNITY_EDITOR
			public enum eEdtiorType
			{
				Static,
				Source,
			}
			[SerializeField]
			public eEdtiorType _editorType;
			[SerializeField]
			public bool _editorFoldout;
			[SerializeField]
			public float _editorHeight;
#endif

			public ValueSource()
			{
#if UNITY_EDITOR
				_editorType = eEdtiorType.Static;
				_editorFoldout = false;
				_editorHeight = EditorGUIUtility.singleLineHeight * 3;
#endif
			}

			public static implicit operator T(ValueSource<T> property)
			{
				return property.GetValue();
			}

			#region IValueSource<T>
			public T GetValue()
			{
				if (_source != null)
				{
					return _source.GetValue();
				}
				else
				{
					return _value;
				}
			}
			#endregion

			#region ISerializationCallbackReceiver
			public void OnBeforeSerialize()
			{

			}

			public void OnAfterDeserialize()
			{
				_source = FindSourceObject();
			}
			#endregion

			public static FieldInfo[] GetValueSourceFields(object obj)
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

			private IValueSource<T> FindSourceObject()
			{
				//Have to cast to system.object to stop unity's multi-threaded errors
				if ((System.Object)_sourceObject != null)
				{
					//If the member name is null/empty then the object itself must be an IValueSource<T>
					if (string.IsNullOrEmpty(_sourceObjectMember))
					{
						return _sourceObject as IValueSource<T>;
					}
					//Otherwise find the field name by id.
					else
					{
						FieldInfo[] fields = GetValueSourceFields(_sourceObject);

						foreach (FieldInfo field in fields)
						{
							if (field.Name == _sourceObjectMember)
							{
								return field.GetValue(_sourceObject) as IValueSource<T>;
							}
						}
					}
				}

				return null;
			}
		}
	}
}