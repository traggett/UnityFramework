using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	namespace ValueSourceSystem
	{
		//Class that allows a value to either be defined as a member or be calculated from other source.
		public abstract class ValueSource<T> : IValueSource<T>
		{
			[SerializeField]
			protected T _value;
			[SerializeField]
			protected UnityEngine.Object _sourceObject;

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

			public ValueSource(T value = default(T), IValueSource<T> sourceObject = null)
			{
				_value = value;
				_sourceObject = sourceObject as UnityEngine.Object;

#if UNITY_EDITOR
				_editorType = sourceObject != null ? eEdtiorType.Source : eEdtiorType.Static;
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
				if (_sourceObject != null)
				{
					IValueSource<T> sourceObject = _sourceObject as IValueSource<T>;

					if (sourceObject != null)
					{
						return sourceObject.GetValue();
					}
					else
					{
						throw new Exception("Object not type of IValueSource<" + typeof(T).Name + ">");
					}
				}
				else
				{
					return _value;
				}
			}
			#endregion
		}
	}
}