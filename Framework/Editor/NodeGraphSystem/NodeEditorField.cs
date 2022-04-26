using UnityEngine;

using System;
using System.Reflection;

namespace Framework
{
	namespace NodeGraphSystem
	{
		namespace Editor
		{
			public sealed class NodeEditorField
			{
				public NodeEditorGUI _nodeEditorGUI;
				public Vector2 _position;
				public FieldInfo _fieldInfo;
				public Type _type;
				public string _name;

				public object GetValue()
				{
					object inputValueInstance = _fieldInfo.GetValue(_nodeEditorGUI.Asset);

					if (inputValueInstance == null)
					{
						if (_fieldInfo.FieldType.IsValueType)
						{
							inputValueInstance = Activator.CreateInstance(_fieldInfo.FieldType);
						}
						else if (_fieldInfo.FieldType.IsClass)
						{
							ConstructorInfo constructor = _fieldInfo.FieldType.GetConstructor(Type.EmptyTypes);
							if (constructor != null)
								inputValueInstance = constructor.Invoke(null);
							else
								throw new Exception("need a parameterless constructor");
						}
					}

					return inputValueInstance;
				}

				public void SetValue(object value)
				{
					_fieldInfo.SetValue(_nodeEditorGUI.Asset, value);
				}
			}
		}
	}
}