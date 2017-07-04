using UnityEngine;
using System.Reflection;
using Framework.Utils;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
			private UnityEngine.Object _sourceObject;
			[SerializeField]
			private string _sourceObjectMemberName;
			[SerializeField]
			private int _sourceObjectMemberIndex = -1;
			[SerializeField]
			public eSourceType _sourceType = eSourceType.Static;

			//Runtime non serialized cached data (in union struct)
			[StructLayout(LayoutKind.Explicit)]
			private struct SourceObject
			{
				[FieldOffset(0)]
				public FieldInfo _fieldInfo;
				[FieldOffset(0)]
				public IValueSource<T> _valueSource;
				[FieldOffset(0)]
				public IDynamicValueSourceContainer _dynamicValueSource;
			}
			private SourceObject _sourceObjectData;


#if UNITY_EDITOR
			[SerializeField]
			public bool _editorFoldout;
			[SerializeField]
			public float _editorHeight;
#endif

			public ValueSource()
			{
#if UNITY_EDITOR
				_editorFoldout = false;
				_editorHeight = EditorGUIUtility.singleLineHeight * 3;
#endif
			}

			public static implicit operator T(ValueSource<T> property)
			{
				return property.GetValue();
			}

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

			#region IValueSource<T>
			public T GetValue()
			{
				switch (_sourceType)
				{
					case eSourceType.SourceObject: return _sourceObjectData._valueSource.GetValue();
					case eSourceType.SourceMember: return (_sourceObjectData._fieldInfo.GetValue(_sourceObject) as IValueSource<T>).GetValue();
					case eSourceType.SourceDynamicMember: return (_sourceObjectData._dynamicValueSource.GetValueSource(_sourceObjectMemberIndex) as IValueSource<T>).GetValue();
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
							if ((object)_sourceObject != null)
								_sourceObjectData._valueSource = _sourceObject as IValueSource<T>; 
						}
						break;
					case eSourceType.SourceMember:
						{
							if ((object)_sourceObject != null)
							{
								FieldInfo[] fields = GetValueSourceFields(_sourceObject);

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
							if ((object)_sourceObject != null)
								_sourceObjectData._dynamicValueSource = _sourceObject as IDynamicValueSourceContainer;
						}
						break;
					}
			}
			#endregion
		}
	}
}