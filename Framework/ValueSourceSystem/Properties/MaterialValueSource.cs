using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using Utils;

	namespace ValueSourceSystem
	{
		[Serializable]
		public struct MaterialValueSource : IValueSource<Material>
		{
			[SerializeField]
			private MaterialRefProperty _value;
			[SerializeField]
			private UnityEngine.Object _sourceObject;

#if UNITY_EDITOR
			public enum eEdtiorType
			{
				Static,
				Source,
			}
			[SerializeField]
			private eEdtiorType _editorType;
			[SerializeField]
			private bool _editorFoldout;
			[SerializeField]
			private float _editorHeight;
#endif

			public MaterialValueSource(MaterialRefProperty value = default(MaterialRefProperty), IValueSource<float> sourceObject = null)
			{
				_value = value;
				_sourceObject = sourceObject as UnityEngine.Object;

#if UNITY_EDITOR
				_editorType = _sourceObject != null ? eEdtiorType.Source : eEdtiorType.Static;
				_editorFoldout = false;
				_editorHeight = EditorGUIUtility.singleLineHeight * 3;
#endif
			}

			public static implicit operator Material(MaterialValueSource property)
			{
				return property.GetValue();
			}

			public static implicit operator MaterialValueSource(MaterialRefProperty value)
			{
				return new MaterialValueSource(value);
			}

			#region IValueSource<Material>
			public Material GetValue()
			{
				if (_sourceObject != null)
				{
					IValueSource<Material> sourceObject = _sourceObject as IValueSource<Material>;

					if (sourceObject != null)
					{
						return sourceObject.GetValue();
					}
					else
					{
						throw new Exception("Object not type of IValueSource<" + typeof(Material).Name + ">");
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