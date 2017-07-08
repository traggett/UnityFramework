using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[Serializable]
		public class NodeInputField<T> : NodeInputFieldBase<T>
		{
			public T _value;
			
			public NodeInputField()
			{
				if (typeof(T).IsClass && typeof(T) != typeof(string))
				{
					try
					{
						_value = (T)Activator.CreateInstance(typeof(T), true);
					}
					catch
					{
						throw new Exception("Node Input Field Type " + typeof(T) + " should either be a struct or have a default parameterless constructor");
					}
				}
				else
				{
					_value = default(T);
				}
			}

			public static implicit operator NodeInputField<T>(T value)
			{
				NodeInputField<T> nodeInput = new NodeInputField<T>();
				nodeInput._value = value;
				return nodeInput;
			}

			protected override T GetStaticValue()
			{
				return _value;
			}

#if UNITY_EDITOR
			protected override void ClearStaticValue()
			{
				_value = default(T);
			}
#endif
		}
	}
}